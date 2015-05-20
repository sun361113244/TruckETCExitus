using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

[assembly: log4net.Config.XmlConfigurator()]
namespace Util
{
    public class LogTools
    {
        #region 系统日志

        public static void WriteSystemErrorLog(Type t, string method, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("System Logger");
            string err = string.Format("错误方法:{0}()::错误信息:{1}", method, msg);
            log.Error(err);
        }
        public static void WriteSystemMonitorLog(string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("System Logger");
            log.Info(msg);
        }

        #endregion

        #region 数据采集日志

        public static void WriteDataCollectorLog(Type t, string method, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("DataCollector Logger");
            string err = string.Format("错误方法:{0}()::错误信息:{1}", method, msg);
            log.Error(err);
        }
        public static void WriteDataCollectorMonitorLog(string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("DataCollector Logger");
            log.Info(msg);
        }

        #endregion

        #region 天线日志

        public static void WriteAntennaLog(Type t, string method, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("Antenna Logger");
            string err = string.Format("错误方法:{0}()::错误信息:{1}", method, msg);
            log.Error(err);
        }
        public static void WriteAntennaMonitorLog(string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("Antenna Logger");
            log.Info(msg);
        }

        #endregion

        #region 线圈日志

        public static void WriteCoilLog(Type t, string method, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("Coil Logger");
            string err = string.Format("错误方法:{0}()::错误信息:{1}", method, msg);
            log.Error(err);
        }
        public static void WriteCoilMonitorLog(string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("Coil Logger");
            log.Info(msg);
        }

        #endregion

        #region 本地服务器日志

        public static void WriteLocSrvLog(Type t, string method, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("Server Logger");
            string err = string.Format("错误方法:{0}()::错误信息:{1}", method, msg);
            log.Error(err);
        }
        public static void WriteLocSrvMonitorLog(string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("Server Logger");
            log.Info(msg);
        }

        #endregion

        #region 串口基类日志

        public static void WriteSerialPortLog(Type t, string method, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("SerialPort Class Logger");
            string err = string.Format("错误方法:{0}()::错误信息:{1}", method, msg);
            log.Error(err);
        }
        public static void WriteSerialPortMonitorLog(string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("SerialPort Class Logger");
            log.Info(msg);
        }

        #endregion

        #region TCP客户端基类日志

        public static void WriteTCPCliLog(Type t, string method, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("TCP Client Logger");
            string err = string.Format("错误方法:{0}()::错误信息:{1}", method, msg);
            log.Error(err);
        }
        public static void WriteTCPCliMonitorLog(string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("TCP Client Logger");
            log.Info(msg);
        }

        #endregion

        #region TCP服务器基类日志

        public static void WriteTCPSrvLog(Type t, string method, string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("TCP Server Logger");
            string err = string.Format("错误方法:{0}()::错误信息:{1}", method, msg);
            log.Error(err);
        }
        public static void WriteTCPSrvMonitorLog(string msg)
        {
            log4net.ILog log = log4net.LogManager.GetLogger("TCP Server Logger");
            log.Info(msg);
        }

        #endregion
    }
}
