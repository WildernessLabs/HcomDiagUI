using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HcomDiagUi
{
    // Not implemented
    public class DnldEsp32Files
    {
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
