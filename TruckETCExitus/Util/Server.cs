using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util
{
    public abstract class Server
    {
        #region 事件

        public delegate void ServerConnHandler(CSUnit csUnit);

        public delegate void ServerCloseHandler(CSUnit csUnit);

        public delegate void ServerRecvDataHandler(CSUnit csUnit);

        public delegate void ServerFull(CSUnit csUnit);

        /// <summary>
        /// 服务端连接事件
        /// </summary>
        public ServerConnHandler serverConnHandler;

        /// <summary>
        /// 服务端关闭事件
        /// </summary>
        public ServerCloseHandler serverCloseHandler;

        /// <summary>
        /// 服务端接收数据事件
        /// </summary>
        public ServerRecvDataHandler serverRecvDataHandler;

        /// <summary>
        /// 服务端满事件
        /// </summary>
        public ServerFull serverFullHandler;

        #endregion

        #region 服务器继承方法
        public abstract void Connect();

        public abstract int GetConnectionCount();

        public abstract bool IsConnect();
        public abstract void Close();

        public abstract void Send(byte[] datagram);

        #endregion
    }
}
