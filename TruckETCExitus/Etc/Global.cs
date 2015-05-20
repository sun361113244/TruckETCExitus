using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TruckETCExitus.Device;
using TruckETCExitus.Model;
using Util;

namespace TruckETCExitus.Etc
{
    class Global
    {
        #region 消息

        public static uint WM_DATA = SystemUnit.RegisterWindowMessage("LOWNEWDATACOME");                // 仪表来称重数据消息
        public static uint WM_DATAFRAMECOME = SystemUnit.RegisterWindowMessage("LOWNEWDATAFRAMECOME");  // 仪表数据帧到来消息
        public static uint WM_RASTER_COME = SystemUnit.RegisterWindowMessage("RASTERCOME");             // 光栅数据到来消息
        public static uint WM_VEH_RASTER_COME = SystemUnit.RegisterWindowMessage("VEHRASTERCOME");      // 车辆到位消息

        #endregion

        public static System.Data.Common.DbConnection SQLDBConn;                                        // SQLServer数据库连接

        public static DataCollector dataCollector;                                                      // 数据采集器
        public static Raster raster;                                                                    // 光栅控制

        public static LocalServer localServer;                                                          // 本地服务器

        public static int COIL_COUNT = 8;                                                               // 线圈数量
        public static Coils coils;                                                                      // 线圈

        public static Antenna PreAntenna;                                                               // 预读天线

        public static Antenna TrdAntenna;                                                               // 交易天线

        public static ExchangeQueue exchangeQueue = new ExchangeQueue();                          // 交易队列

        /// ///////////////////////////////////////////////////////////////////////////////////

        public static Queue<OBUData> preOBUQueue = new Queue<OBUData>();                        // 预读OBU队列
        public static ExchangeQueue preQueue = new ExchangeQueue();                             // 待交易队列

        public enum AlartStat
        {
            NonAlarmed = 1,
            Alarmed,
            HalfAlarmed
        };
        public static  AlartStat prdAlartStat1 = AlartStat.NonAlarmed;

        public static AlartStat trdAlartStat1 = AlartStat.NonAlarmed;

        /////////////////////////////////////////////////////////////////////////////////////////
    }
}
