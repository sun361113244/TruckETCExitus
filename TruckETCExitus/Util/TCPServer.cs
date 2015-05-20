using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace Util
{
    public class TCPServer : Server
    {
        #region 属性

        private TcpSvr svr;

        #endregion

        #region 构造函数
        public TCPServer(string address, int port)
        {
            svr = new TcpSvr((ushort)port);

            svr.Resovlver = new DatagramResolver(0xff);

            //新客户端连接
            svr.ClientConn += new NetEvent(ClientConn);

            //客户端关闭
            svr.ClientClose += new NetEvent(ClientClose);

            //接收到数据
            svr.RecvData += new NetEvent(RecvData);
        }

        #endregion

        #region 抽象方法
        public override void Connect()
        {
            svr.Start();            
        }
        public override int GetConnectionCount()
        {
            return svr.SessionTable.Count;
        }
        public override bool IsConnect()
        {
            return svr.IsRun;
        }
        public override void Close()
        {
            try
            {
                Control.CheckForIllegalCrossThreadCalls = false;
            }
            catch { }
            try
            {
                if (svr != null)
                    svr.Stop();
            }
            catch { }            
        }
        public override void Send(byte[] datagram)
        {
            Hashtable ht = svr.SessionTable;

            foreach (DictionaryEntry de in ht)
            {
                Session clientSession = (Session)de.Value;

                svr.Send(clientSession, datagram);                
            }
        }

        #endregion

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
            if (e.Client.Datagram != null && e.Client.Datagram.Count != 0)
            {
                csUnit.Buffer = new byte[e.Client.Datagram.Count];
                Array.Copy(e.Client.Datagram.ToArray(), csUnit.Buffer, e.Client.Datagram.Count);
            }

            return csUnit;
        }

        #region 绑定事件
        private void ClientConn(object sender, NetEventArgs e)
        {           
            CSUnit csUnit = GetRemoteCSUnit(ref e);
            if (this.serverConnHandler != null)
            {
                this.serverConnHandler(csUnit);                
            }
        }
        private void ClientClose(object sender, NetEventArgs e)
        {
            CSUnit csUnit = GetRemoteCSUnit(ref e);

            if (this.serverCloseHandler != null)
            {
                this.serverCloseHandler(csUnit);                
            }            
        }

        void RecvData(object sender, NetEventArgs e)
        {
            CSUnit csUnit = GetRemoteCSUnit(ref e);

            if (this.serverRecvDataHandler != null)
            {
                this.serverRecvDataHandler(csUnit);
            }            
        }

        #endregion

    }
}
