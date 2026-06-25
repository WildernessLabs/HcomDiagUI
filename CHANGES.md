# HcomDiagUI — Uncommitted Changes Analysis

This document summarises all differences between the committed version (`HEAD`/`main`) and
the current working tree. There are **12 modified files** and **1 new untracked file**.

---

## Important Context: Two Divergent Branches, One Lost Ancestor

Both the `main` branch and the uncommitted working tree evolved independently from a single
**common ancestor project that is now lost**. This changes the nature of the analysis
fundamentally:

- **Neither version is authoritative.** `main` is not the "base" that the working tree
  modifies — both are co-equal forks.
- **A line present in `main` but absent from the working tree is not necessarily a
  "deletion"** — it may be something `main` added that the working tree branch never adopted.
- **The goal is reconciliation**, not "apply working tree changes to main." You are merging
  two parallel evolutions, each of which may have unique value.

### Character of each branch

| `main` branch | Working tree branch |
|---|---|
| More defensive — adds guards, locks, error handling | Simpler — removes guards and try/catch wrappers |
| Verbose logging (`WriteSendRecvTimeLine` everywhere) | Quieter; uses `WriteDebugLine` or removes calls |
| Two-method send architecture (`BuildMsgIterations` + helper) | Single consolidated method |
| `ManualResetEvent` (`.WaitOne`) | `ManualResetEventSlim` (`.Wait`) — a deliberate API upgrade |
| Has `sendButtonEnabled` interlock to prevent double-click | No send-button interlock |
| Has `_isFileDnldActive` lock to prevent duplicate file-download | No such lock |
| `CurrentProcState` enum name | `MessageComplex` — cleaner name |

The working tree reads like a **simplification / cleanup pass** from the original. `main`
reads like a **defensive robustness pass**. The ideal merged result should combine: the
working tree's cleaner naming and architecture **plus** `main`'s thread-safety guards and
UX interlocks.

---

## New / Untracked File

### `DecodeRecvd.cs` ⚠️ Dependency Issue

This file exists only in the working tree branch. It contains a `MeadowSerialDataProcessor`
class and `MeadowMessageType` enum adapted from the **MeadowCLI** project. It references
external namespaces (`Meadow.CLI.Internals.MeadowComms.RecvClasses`,
`MeadowCLI.DeviceManagement`) not present in this project's references.

Given the diverged-ancestor context, this likely represents **active feature development**
— a replacement or alternative receive path being built in the working tree branch — rather
than casual experimentation.

**Recommendation**: Do not discard. Locate the MeadowCLI dependency, or extract the logic
that does not depend on it. The file will not compile as-is.

---

## Modified Files

### 1. `ProtocolHelper.cs` — Naming Improvement (Use Working Tree)

| `main` | Working tree |
|--------|-------------|
| `enum CurrentProcState` | `enum MessageComplex` |
| `CurrentProcState.F7FileDnldRdy` | `MessageComplex.F7FileDnld` |
| `MeadowRqstData.procState` field | `MeadowRqstData.msgComplex` field |
| `HostRqstData.procState` field | `HostRqstData.msgComplex` field |

`MessageComplex` is a clearer name than `CurrentProcState` — the latter implies runtime
state; the former describes structural message complexity. All call sites updated consistently.

Also: `HCOM_MDOW_REQUEST_START_FILE_TRANSFER` now maps to `MessageComplex.F7FileDnld`
(was `Normal` in `main`) — a **semantic fix** that the working tree branch got right.

**Recommendation**: Use the working tree's naming throughout.

---

### 2. `Comms/CommsManager.cs` — Threading Consolidation + Event Type Change

#### Event type (working tree is the improvement)

`main` uses `ManualResetEvent` (`.WaitOne()`). The working tree uses `ManualResetEventSlim`
(`.Wait()`). `ManualResetEventSlim` is the modern .NET replacement — lighter weight, supports
`CancellationToken`, appropriate for in-process waits. The working tree declaration and usage
are self-consistent. Verify no other code holds a reference typed as `ManualResetEvent`.

#### Two-method → one-method consolidation (working tree is cleaner)

`main` has two methods:
- `BuildMsgIterations` (thread-pool thread): loops over iterations, waits for concluded event
- `BuildMsgToSendHelper`: sends one message

The working tree merges both into a single `BuildMsgToSendHelper`. Functionally equivalent,
simpler design.

#### Send-button interlock — **restore from `main`**

`main` disables the send button before queuing work (preventing double-click) and re-enables
it in the finally block. The working tree never added this — it evolved from the original
before this feature existed. Since `main` deliberately added it as a robustness improvement,
**the merged result should restore `SendButtonEnable(false)` before `QueueUserWorkItem` and
`SendButtonEnable(true)` in the finally block**.

#### Logging

Working tree replaces many `WriteSendRecvTimeLine` calls with `WriteDebugLine` or removes
them. This is correct — the timeline log was being overloaded for general debug output.

**Recommendation**: Use working tree architecture (consolidated method, `ManualResetEventSlim`)
but **re-add the send-button enable/disable** from `main`.

---

### 3. `Comms/SendHcom.cs` — Method Rename + Code Cleanup (Use Working Tree)

- `BuildAndSendF7FileDnldCommand` → `BuildAndSendFileRelatedCommand` (better: not F7-specific).
- `EncodeAndSendPacket`: `MeadowRqstType requestType` parameter removed. It was never used
  for routing — only appeared in deleted commented-out code. Correct to remove.
- Large block of commented-out dead code (RJCP, alternative send strategies) removed.
- Error handling simplified: `while(true)` loop on exception replaced with `Thread.Sleep(500)`
  + rethrow. New generic `catch(Exception)` block added — an improvement.

#### Caution: COBS `Debug.WriteLine` is now active

This was commented out in both branches but uncommented in the working tree. It will log
every encoded packet's hex to debug output — very verbose during file downloads.

**Recommendation**: Use working tree version but wrap the COBS `Debug.WriteLine` in
`#if DEBUG` or re-comment it before release.

---

### 4. `Comms/RecvHcom.cs` — Using Reorder Only (Use Working Tree)

`using System.IO.Ports` moved to the top; commented RJCP `using` directives removed. No
functional change.

**Recommendation**: Merge as-is.

---

### 5. `ProcRecvd/DnldF7Files.cs` — Method Renames + Removed Thread-Safety Guard

#### Renames and loop restructure (use working tree)

| `main` | Working tree |
|--------|-------------|
| `_commsMgr` field | `commsManager` |
| `DnldStartFileInfo(...)` | `FindDownloadFileInformation(...)` |
| `DnldChkF7ReadyMsg(...)` | `ProcessRcvdF7Dnld(...)` |
| `DnldF7DataPackets(Object state)` | `SendFilePackets(Object state)` |
| `BuildAndSendF7FileDnldCommand(...)` | `BuildAndSendFileRelatedCommand(...)` |
| `userData` parameter | `partition` (clearer intent) |

Loop restructured from `while(true)/break` to `do...while` — cleaner and functionally
equivalent. Sequence number behaviour unchanged (first packet sent is still 1).

#### `_isFileDnldActive` guard — **restore from `main`**

`main` added `_isFileDnldActive` + `lock(this)` to prevent a second download thread from
starting if `HCOM_HOST_REQUEST_INIT_DOWNLOAD_OKAY` arrived twice (a real protocol race
condition). The working tree never added this. Since `main` deliberately added it,
**re-add it to the merged result**.

`Debug.Assert(false)` in the `default:` case was removed in the working tree — acceptable.

**Recommendation**: Use working tree method names and loop structure, but **re-add the
`_isFileDnldActive` + `lock(this)` guard** from `main`.

---

### 6. `ProcRecvd/NormalRcvdProc.cs` — Removed Dead Code and Error Pause (Use Working Tree)

- `static readonly writeMsgLock`: declared in `main` but never used in either branch. Correct
  to remove.
- `try/catch` with `Thread.Sleep(10 * 1000)` on error: the 10-second pause was a debugging
  convenience, not production behaviour. Correct to remove.
- Verbose header label: `"Processing [Header-"` → `"Received [Header-"` — minor, keep.

**Recommendation**: Use working tree version.

---

### 7. `HcomDiagUi.cs` — Send-Button Interlock Removed — **Restore from `main`**

Working tree removes `SafeCallDelegateBool` delegate, `sendButtonEnabled(bool)` method, and
the `SendButtonEnable(false)` call before `BuildMsgToSend()`. These were added by `main` as
a UX safety measure that the working tree branch never adopted. As noted under
`CommsManager.cs`, **the merged version must restore the send-button interlock**.

Removing the unused `using static System.Net.Mime.MediaTypeNames` import is safe to take
from the working tree.

**Recommendation**: Take working tree's import cleanup but **restore the send-button
disable/enable logic** from `main`.

---

### 8. `HcomDiagUi.Designer.cs` — UI Defaults and Layout (Use Working Tree)

- Default COM port: `"Com9"` → `"Com10"` — machine-specific; set to match your hardware.
- `textForMsgTextBox` default text removed (was `"test_TestFileGen1-26.txt"`) — correct;
  a hardcoded test filename should not be the default.
- `execTimeTextBox` label: `"Info / Time"` → `"Execution Time"` — clearer.
- `label2` text updated to include a command-line usage example — more informative.
- Minor pixel-level repositioning of several controls.

**Recommendation**: Use working tree. Adjust the COM port default to match your hardware.

---

### 9. `HcomDiagUi.csproj` — Removed Assembly Reference (Verify Then Merge)

`System.Management` reference removed. Since both branches diverged from the same original,
it is unclear whether `main` added this or the original had it and the working tree dropped
it. A build after merging will immediately confirm whether anything still depends on it.

**Recommendation**: Use working tree and confirm the build passes.

---

### 10. `Misc/CirBuffer.cs` — Removed Method + Comment Fix

- Comment clarification: `"first delimiter found (0)"` → `"first byte whose value is 0"` — clearer.
- `GetCachedByteCount()` removed. `main` has this method; working tree does not. Since both
  branches diverged from the same original, `main` may have **added** this utility.

**Recommendation**: Search the entire codebase for `GetCachedByteCount` calls. If nothing
calls it, omit it. If any code in `main` calls it, keep it.

---

### 11. `UiSupport.cs` — Comment Cleanup (Use Working Tree)

Trivial comment text changes. No functional impact.

---

### 12. `README.md` — Text Polish (Use Working Tree)

Minor prose rewrite; `Peter` signature removed.

---

## Summary Table

| File | Recommendation |
|------|----------------|
| `DecodeRecvd.cs` (new) | Investigate MeadowCLI dependency — do not discard |
| `ProtocolHelper.cs` | Working tree (better names + semantic fix) |
| `Comms/CommsManager.cs` | Working tree + **restore** send-button interlock from `main` |
| `Comms/SendHcom.cs` | Working tree + guard COBS `Debug.WriteLine` with `#if DEBUG` |
| `Comms/RecvHcom.cs` | Working tree |
| `ProcRecvd/DnldF7Files.cs` | Working tree + **restore** `_isFileDnldActive` guard from `main` |
| `ProcRecvd/NormalRcvdProc.cs` | Working tree |
| `HcomDiagUi.cs` | Working tree cleanup + **restore** send-button interlock from `main` |
| `HcomDiagUi.Designer.cs` | Working tree (adjust COM port for hardware) |
| `HcomDiagUi.csproj` | Working tree (verify build) |
| `Misc/CirBuffer.cs` | Working tree + verify `GetCachedByteCount` has no callers |
| `UiSupport.cs` | Working tree |
| `README.md` | Working tree |

---

## Action Items Before the Merge is Complete

### Restore from `main` (safety features the working tree branch never adopted):

1. **Send-button interlock** — `sendButtonEnabled(bool)` + `SafeCallDelegateBool` in
   `HcomDiagUi.cs`; `SendButtonEnable(false/true)` around `QueueUserWorkItem` in
   `CommsManager.cs`. Prevents concurrent sends from a double-click.

2. **`_isFileDnldActive` + `lock(this)` guard** in `DnldF7Files.cs`. Prevents a second
   download thread from starting if the device sends `INIT_DOWNLOAD_OKAY` more than once.

### Resolve before the build is clean:

3. **`DecodeRecvd.cs`** — References MeadowCLI types not in this project. Add the
   dependency or port the logic before adding to the project.

4. **`ManualResetEvent` → `ManualResetEventSlim`** — Confirm no other code holds the field
   typed as `ManualResetEvent`. The change is a genuine improvement.

### Verify with a search:

5. **`GetCachedByteCount()`** — One call anywhere in the codebase means it must be kept.

6. **COBS `Debug.WriteLine`** in `SendHcom.cs` — Was previously commented out; now active.
   Wrap in `#if DEBUG` if it should not run in release builds.
