using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Util
{
    public class TCPClient : Client
    {
        #region 属性

        private TcpCli cli;

        #endregion

        #region 构造函数

        public TCPClient()
        {
            cli = new TcpCli();

            cli.Resovlver = new DatagramResolver(0xff);

            //新客户端连接
            cli.ConnectedServer += new NetEvent(ClientConn);

            //客户端关闭
            cli.DisConnectedServer += new NetEvent(ClientClose);

            //接收到数据
            cli.ReceivedDatagram += new NetEvent(RecvData);
        }


        #endregion

        #region 抽象方法
        public override void Connect(string address, int port)
        {
            cli.Connect(address, port);
            
            Thread.Sleep(100);
        }

        public override bool IsConnect()
        {
            if (cli != null)
            {
                return cli.IsConnected;
            }
            else
                return false;
        }

        public override void Close()
        {
            cli.Close();            
        }

        public override void Send(byte[] datagram)
        {
            cli.Send(datagram);            
        }

        #endregion

        #region 事件函数
        private CSUnit GetRemoteCSUnit(Session clientSession, byte[] datagram)
        {
            CSUnit csUnit = new CSUnit();
            IPEndPoint ipEndPoint = (IPEndPoint)clientSession.ClientSocket.RemoteEndPoint;
            csUnit.IpOrCommPort = ipEndPoint.Address.ToString();
            csUnit.PortOrBaud = ipEndPoint.Port;
            if (datagram != null && datagram.Length != 0)
            {
                csUnit.Buffer = new byte[datagram.Length];
                Array.Copy(datagram, csUnit.Buffer, datagram.Length);
            }
            return csUnit;
        }

        private CSUnit GetRemoteCSUnit(ref NetEventArgs e)
        {
            CSUnit csUnit = new CSUnit();
            IPEndPoint ipEndPoint = (IPEndPoint)e.Client.ClientSocket.RemoteEndPoint;
            csUnit.IpOrCommPort = ipEndPoint.Address.ToString();
            csUnit.PortOrBaud = ipEndPoint.Port;
            csUnit.Buffer = new byte[e.Client.Datagram.Count];
            Array.Copy(e.Client.Datagram.ToArray(), csUnit.Buffer, e.Client.Datagram.Count);

            return csUnit;
        }
        private void ClientConn(object sender, NetEventArgs e)
        {
            CSUnit csUnit = GetRemoteCSUnit(ref e);

            if (this.clientConnHandler != null)
            {
                this.clientConnHandler(csUnit);
            }            
        }
        private void ClientClose(object sender, NetEventArgs e)
        {
            CSUnit csUnit = GetRemoteCSUnit(ref e);

            if (this.clientCloseHandler != null)
            {
                this.clientCloseHandler(csUnit);

            }           

            cli.Close();
        }
        private void RecvData(object sender, NetEventArgs e)
        {
            CSUnit csUnit = GetRemoteCSUnit(ref e);

            if (this.recvDataHandler != null)
            {
                this.recvDataHandler(csUnit);
            }            
        }
        #endregion
    }
}
