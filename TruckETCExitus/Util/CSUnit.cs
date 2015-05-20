using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util
{
    public class CSUnit : ICloneable
    {
        private string ipOrCommPort;

        private int portOrBaud;

        private byte[] buffer;

        public string IpOrCommPort
        {
            get { return ipOrCommPort; }
            set { ipOrCommPort = value; }
        }

        public int PortOrBaud
        {
            get { return portOrBaud; }
            set { portOrBaud = value; }
        }

        public byte[] Buffer
        {
            get { return buffer; }
            set { buffer = value; }
        }
        public CSUnit()
        {

        }
        public CSUnit(string address, int port, byte[] buff)
        {
            this.ipOrCommPort = address;
            this.portOrBaud = port;
            this.buffer = new byte[buff.Length];

            Array.Copy(buff, 0, this.buffer, 0, buff.Length);
        }
        public object Clone()
        {
            CSUnit csUnit = new CSUnit();
            csUnit.ipOrCommPort = this.ipOrCommPort;
            csUnit.portOrBaud = this.portOrBaud;
            csUnit.buffer = new byte[this.buffer.Length];
            Array.Copy(this.buffer, 0, csUnit.Buffer, 0, this.buffer.Length);

            return csUnit;
        }
    }
}
