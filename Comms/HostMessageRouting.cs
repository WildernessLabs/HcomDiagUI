using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HcomDiagUi
{
    //==========================================================================
    // One message received from Meadow
    public sealed class ReceivedMessage
    {
        public HcomProtoHdrMessage Header { get; }
        public HostRqstType Type { get; }
        public HostRqstData Info { get; }       // Name and display class (HostMessages)
        public byte[] Buffer { get; }           // Decoded message, header included
        public int Length { get; }              // Bytes used in Buffer
        public uint PayloadLength { get; }      // Bytes after the header

        public ReceivedMessage(HcomProtoHdrMessage header, HostRqstType type, HostRqstData info,
            byte[] buffer, int length, uint payloadLength)
        {
            Header = header;
            Type = type;
            Info = info;
            Buffer = buffer;
            Length = length;
            PayloadLength = payloadLength;
        }

        // Payload as text, e.g. for text messages; empty if there is none
        public string Text
        {
            get
            {
                int hdrLen = Protocol.HcomProtoHdrMessageSize();
                return Length > hdrLen ? Encoding.ASCII.GetString(Buffer, hdrLen, Length - hdrLen) : "";
            }
        }
    }

    //==========================================================================
    // Handles the messages of one exchange with Meadow (e.g. file download).
    // Handles lists the message types it owns; CommsManager routes those to
    // it. Messages no processor claims go to NormalRcvdProc, which displays
    // them.
    //
    // To add an exchange: write a class implementing this interface, list its
    // message types in Handles, and add it to the processor list in
    // CommsManager.CreateHandlers().
    public interface IHostMessageProcessor
    {
        IEnumerable<HostRqstType> Handles { get; }
        void Process(ReceivedMessage msg);
    }

    public static class HostMessageRouter
    {
        // Map each claimed message type to its processor. Two processors
        // claiming the same type is a programming error.
        public static Dictionary<HostRqstType, IHostMessageProcessor> BuildRoutes(
            IEnumerable<IHostMessageProcessor> processors)
        {
            var routes = new Dictionary<HostRqstType, IHostMessageProcessor>();
            foreach (IHostMessageProcessor processor in processors)
            {
                foreach (HostRqstType type in processor.Handles)
                {
                    if (routes.TryGetValue(type, out IHostMessageProcessor other))
                    {
                        throw new InvalidOperationException($"{type} is claimed by both " +
                            $"{other.GetType().Name} and {processor.GetType().Name}");
                    }
                    routes.Add(type, processor);
                }
            }
            return routes;
        }
    }
}
