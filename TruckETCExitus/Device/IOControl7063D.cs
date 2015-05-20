using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Util;

namespace TruckETCExitus.Device
{
    public class IOControl7063D : IOControl
    {
        #region 属性

        private SerialPortTools serialPort;                     // 串口控制类        

        #endregion

        #region 构造函数
        public IOControl7063D()
        {
            serialPort = new SerialPortTools();
        }

        #endregion

        #region 方法

        private int convertToInt(byte value)
        {
            switch (value)
            {
                case 0x30:
                    return 0;
                case 0x31:
                    return 1;
                case 0x32:
                    return 2;
                case 0x33:
                    return 3;
                case 0x34:
                    return 4;
                case 0x35:
                    return 5;
                case 0x36:
                    return 6;
                case 0x37:
                    return 7;
                case 0x38:
                    return 8;
                case 0x39:
                    return 9;
                case 0x41:
                    return 10;
                case 0x42:
                    return 11;
                case 0x43:
                    return 12;
                case 0x44:
                    return 13;
                case 0x45:
                    return 14;
                case 0x46:
                    return 15;
                default:
                    return 15;
            }
        }

        protected override void IODataReceived(byte[] buffer)
        {
            int value = -1;

            if (ioRecvHandler != null)
            {
                if (buffer.Length == 6 && buffer[0] == 0x3E && buffer[1] == 0x30 && buffer[2] == 0x30 && buffer[5] == 0x0d)
                {
                    value = convertToInt(buffer[3]) * 16 + convertToInt(buffer[4]);
                    ioRecvHandler(value);
                }
                else
                    ioRecvHandler(value);
            }
        }
        public override int Initialize(int IOType, string CommPort, int baud)
        {
            if (IOType == 1)
            {
                serialPort.dataReceived = IODataReceived;
                return serialPort.Open(CommPort, baud);
            }
            return 0;
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            serialPort.Write(buffer, offset, count);
        }

        public override bool IsOpen()
        {
            return serialPort.IsOpen() == 1;
        }
        public override void Close()
        {
            if (serialPort.IsOpen() == 1)
                serialPort.Close();
        }
        #endregion
    }
}
