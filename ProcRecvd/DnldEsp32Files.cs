using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HcomDiagUi
{
    // Not implemented yet: placeholder for ESP32 file download. Claims no
    // messages until then.
    public class DnldEsp32Files : IHostMessageProcessor
    {
        public IEnumerable<HostRqstType> Handles => Array.Empty<HostRqstType>();

        public void Process(ReceivedMessage msg)
        {
            ProcessRcvdEsp32Dnld(msg.Header, msg.Info, msg.Type, msg.Buffer, msg.Length,
                msg.PayloadLength);
        }

        private CommsManager commsManager;
        private UiSupport uiSupport;

        public DnldEsp32Files(CommsManager commsManager, UiSupport uiSupport)
        {
            this.commsManager = commsManager;
            this.uiSupport = uiSupport;
        }

        public void ProcessRcvdEsp32Dnld(HcomProtoHdrMessage hdrMsg,
                HostRqstData hostRqstData, HostRqstType hostRqstType,
                byte[] messageBuffer, int messageLength, uint payloadLength)
        {

        }

    }
}
