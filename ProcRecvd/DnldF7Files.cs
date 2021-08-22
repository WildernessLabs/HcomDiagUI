using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HcomDiagUi
{
    // Not implemented
    public class DnldF7Files
    {
        private CommsManager commsManager;
        private UiSupport uiSupport;

        public DnldF7Files(CommsManager commsManager, UiSupport uiSupport)
        {
            this.commsManager = commsManager;
            this.uiSupport = uiSupport;
        }

        public void ProcessRcvdF7Dnld(HcomProtoHdrMessage hdrMsg,
                HostRqstData hostRqstData, HostRqstType hostRqstType,
                byte[] messageBuffer, int messageLength, uint payloadLength)
        {

        }


    }
}
