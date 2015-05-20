using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TruckETCExitus.Device
{
    public abstract class IOControl
    {
        #region 事件
        public delegate void IORecvHandler(int value);
        protected IORecvHandler ioRecvHandler;                                                      // 收到数据时触发

        public IORecvHandler IoRecvHandler
        {
            set { ioRecvHandler = value; }
        }
        #endregion
        protected abstract void IODataReceived(byte[] buffer);
        public abstract int Initialize(int IOType, string addressOrCommPort, int portOrBaud);

        public abstract void Write(byte[] buffer, int offset, int count);

        public abstract bool IsOpen();

        public abstract void Close();
    }
}
