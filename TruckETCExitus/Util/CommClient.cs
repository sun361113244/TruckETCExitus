using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util
{
    public class CommClient : Client
    {
        #region 属性

        public SerialPortTools serialPort;

        /// <summary>
        /// 报文解析器
        /// </summary>
        private DatagramResolver _resolver;

        #endregion

        #region 构造函数
        public CommClient()
        {
            serialPort = new SerialPortTools();

            _resolver = new DatagramResolver(0xff);

            serialPort.dataReceived += Recvfunc;
        }
        public CommClient(string address, int port)
        {
            serialPort = new SerialPortTools();

            _resolver = new DatagramResolver(0xff);

            serialPort.dataReceived += Recvfunc;

            serialPort.Open(address, port);
        }

        #endregion

        #region 函数句柄
        void Recvfunc(byte[] buffer)
        {
            CSUnit csUnit = GetRemoteCSUnit(buffer);

            List<byte> receivedData = new List<byte>(csUnit.Buffer);

            List<byte>[] recvDatagrams = _resolver.Resolve(ref receivedData);

            foreach (List<byte> newDatagram in recvDatagrams)
            {
                CSUnit tempCSUnit = new CSUnit(csUnit.IpOrCommPort, csUnit.PortOrBaud, newDatagram.ToArray());

                this.recvDataHandler(tempCSUnit);
            }
            
        }

        private CSUnit GetRemoteCSUnit(byte[] buffer)
        {
            CSUnit csUnit = new CSUnit();
            csUnit.IpOrCommPort = serialPort.GetPortNo();
            csUnit.PortOrBaud = serialPort.GetBaudRate();
            csUnit.Buffer = new byte[buffer.Length];
            Array.Copy(buffer, csUnit.Buffer, buffer.Length);

            return csUnit;
        }

        #endregion

        #region 抽象方法
        public override void Connect(string address, int port)
        {
            serialPort.Open(address, port);
        }

        public override bool IsConnect()
        {
            return serialPort.IsOpen() == 1;
        }

        public override void Close()
        {
            serialPort.Close();
        }

        public override void Send(byte[] datagram)
        {
            serialPort.Write(datagram, 0, datagram.Length);
        }

        #endregion

    }
}
