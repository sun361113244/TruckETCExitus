using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Util;

namespace TruckETCExitus.Device
{
    public class LocalServer
    {
        #region 天线数据常量

        /// <summary>
        /// C1帧数据长度
        /// </summary>
        public static int C1_LENGTH = 10;

        /// <summary>
        /// C2帧数据长度
        /// </summary>
        public static int C2_LENGTH = 10;

        /// <summary>
        /// CD帧数据长度
        /// </summary>
        public static int CD_LENGTH = 13;

        /// <summary>
        /// 命令字位置
        /// </summary>
        public static int CMD_LOC = 3;

        /// <summary>
        /// 帧头
        /// </summary>
        public static byte[] bof = new byte[2] { 0xff, 0xff };

        /// <summary>
        /// 帧尾
        /// </summary>
        public static byte[] eof = new byte[1] { 0xff };

        #endregion

        #region 事件
        public delegate void LocSrvConnHandler(CSUnit server);

        public delegate void LocSrvCloseHandler(CSUnit server);

        public delegate void LocSrvRecvDataHandler(CSUnit server);


        /// <summary>
        /// 客户端连接事件
        /// </summary>
        public LocSrvConnHandler locSrvConnHandler;

        /// <summary>
        /// 客户端关闭事件
        /// </summary>
        public LocSrvCloseHandler locSrvCloseHandler;

        /// <summary>
        /// 服务器接收数据事件
        /// </summary>
        public LocSrvRecvDataHandler locSrvRecvDataHandler;

        #endregion

        #region 属性

        /// <summary>
        /// 服务器类
        /// </summary>
        public Server locSrv;

        #endregion

        #region 构造函数

        public LocalServer(int accessType , string address, int port)
        {
            switch (accessType)
            {
                case 1:
                    throw new Exception(string.Format("accessType错误,无法创建Antenna:{0}", accessType));
                case 2:
                    locSrv = new TCPServer(address, port);
                    break;
                default:
                    throw new Exception(string.Format("accessType错误,无法创建Antenna:{0}", accessType));
            }
            locSrv.serverConnHandler = ClientConnHandler;
            locSrv.serverCloseHandler = ClientCloseHandler;
            locSrv.serverRecvDataHandler = RecvDataHandler;
        }

        #endregion

        #region 方法

        public void Connect()
        {
            locSrv.Connect();
        }
        public bool IsConnect()
        {
            return locSrv.IsConnect();
        }

        public void Close()
        {
            locSrv.Close();
        }
        public int GetConnectionCount()
        {
            return locSrv.GetConnectionCount();
        }
        public void Send(byte[] buffer)
        {
            if (locSrv.IsConnect())
            {
                if (buffer[0] == Antenna.bof[0] && buffer[1] == Antenna.bof[1] &&
                    buffer[buffer.Length - 1] == Antenna.eof[0] && buffer.Length >= 10 &&
                    buffer[Antenna.HEARTBEAT_LOC] == Antenna.HEARTBEAT_CONTENT &&
                    buffer[Antenna.CMD_LOC] == 0xB2)
                {
                    //心跳包
                    locSrv.Send(buffer);
                }
                else
                {
                    LogTools.WriteLocSrvMonitorLog("LocalServer send:" + SystemUnit.byteToHexStr(buffer));
                    locSrv.Send(buffer);
                }
            }
        }

        #endregion

        #region 事件函数

        private void ClientCloseHandler(CSUnit csUnit)
        {
            if (this.locSrvCloseHandler != null)
            {
                this.locSrvCloseHandler(csUnit);
            }
        }
        private void ClientConnHandler(CSUnit csUnit)
        {
            if (this.locSrvConnHandler != null)
            {
                this.locSrvConnHandler(csUnit);
            }
        }
        private void RecvDataHandler(CSUnit csUnit)
        {
            if (this.locSrvRecvDataHandler != null)
            {
                LogTools.WriteLocSrvMonitorLog("LocalServer receive:" + SystemUnit.byteToHexStr(csUnit.Buffer));
                this.locSrvRecvDataHandler(csUnit);
            }
        }

        #endregion
    }
}
