using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Util
{
    public abstract class Client
    {
        #region 事件

        public delegate void ClientConnHandler(CSUnit csUnit);

        public delegate void ClientCloseHandler(CSUnit csUnit);

        public delegate void RecvDataHandler(CSUnit csUnit);

        /// <summary>
        /// 客户端连接事件
        /// </summary>
        public ClientConnHandler clientConnHandler;

        /// <summary>
        /// 客户端关闭事件
        /// </summary>
        public ClientCloseHandler clientCloseHandler;

        /// <summary>
        /// 客户端接收数据事件事件
        /// </summary>
        public RecvDataHandler recvDataHandler;

        #endregion

        #region 方法

        public abstract void Connect(string address, int port);

        public abstract bool IsConnect();

        public abstract void Close();

        public abstract void Send(byte[] datagram);

        #endregion

    }
}
