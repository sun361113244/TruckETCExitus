using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using TruckETCExitus.Etc;


namespace Util
{
    public static class ReadWritePara
    {
        // XML配置文件的路径
        public static string ConfigFilePath = Application.StartupPath + "\\config.xml";
        #region 读取软件参数
        // 从配置文件config.xml中读取参数
        public static void ReadXMLPara()
        {
            if (File.Exists(ConfigFilePath))
            {
                InitParams.InitModuleCount = 1;

                // 读取本地数据库参数
                ReadLocalDBParams();

                // 读取接收数据参数
                ReadDataCollectorParams();

                // 读取本地服务器参数
                ReadLocalServerParams();

                // 读取接收IO参数
                ReadIOParams();

                // 读取预读天线参数
                ReadPreAntennaParams();

                // 读取交易天线参数
                ReadExchangeAntennaParams();

                // 读取运行参数
                ReadRunParams();

                // 设置启动模块数量
                InitParams.SetInitModuleCount();
            }
            else
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, "读配置文件错误, 文件不存在:" + ConfigFilePath);
                throw new Exception("配置文件不存在.");
            }
        }

        private static void ReadRunParams()
        {
            try
            {
                InitParams.RunMode = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "SystemMode", "RunMode"));

            }
            catch (Exception ex)
            {
                Util.LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("读取运行参数类型转换失败:" + ex.Message);
            }
        }
        private static void ReadIOParams()
        {
            try
            {
                IOModuleParams.IOModuleAccessType = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "IOControl", "Type"));
                IOModuleParams.IOModuleIp = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "IOControl", "IP");
                IOModuleParams.IOModulePort = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "IOControl", "Port"));
                IOModuleParams.IOModuleSerialPort = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "IOControl", "CommPort");
                IOModuleParams.IOModuleBaud = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "IOControl", "ComBps"));

            }
            catch (Exception ex)
            {
                Util.LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("读取IO参数类型转换失败:" + ex.Message);
            }
        }


        private static void ReadExchangeAntennaParams()
        {
            try
            {
                TrdAntennaParams.TrdAntennaAccessType = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "ExchangeAntenna", "Type"));
                TrdAntennaParams.TrdAntennaIp = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "ExchangeAntenna", "IP");
                TrdAntennaParams.TrdAntennaPort = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "ExchangeAntenna", "Port"));
                TrdAntennaParams.TrdAntennaSerialPort = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "ExchangeAntenna", "CommPort");
                TrdAntennaParams.TrdAntennaBaud = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "ExchangeAntenna", "ComBps"));                
            }
            catch (Exception ex)
            {
                Util.LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("读取预处理天线参数类型转换失败:" + ex.Message);
            }
        }

        private static void ReadPreAntennaParams()
        {
            try
            {
                PreAntennaParams.PreAntennaAccessType = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "PreAntenna", "Type"));
                PreAntennaParams.PreAntennaIp = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "PreAntenna", "IP");
                PreAntennaParams.PreAntennaPort = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "PreAntenna", "Port"));
                PreAntennaParams.PreAntennaSerialPort = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "PreAntenna", "CommPort");
                PreAntennaParams.PreAntennaBaud = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "PreAntenna", "ComBps"));                
            }
            catch (Exception ex)
            {
                Util.LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("读取预处理天线参数类型转换失败:" + ex.Message);
            }
        }

        private static void ReadLocalServerParams()
        {
            try
            {
                LocalServerParams.LocalServerMode = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalServer", "SendFlag"));
                LocalServerParams.IP = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalServer", "IP");
                LocalServerParams.Port = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalServer", "Port"));                
            }
            catch (Exception ex)
            {
                Util.LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("读取本地服务器参数类型转换失败:" + ex.Message);
            }
        }

        private static void ReadDataCollectorParams()
        {
            try
            {
                DataCollectorParams.localAccessType = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalDataCollector", "DataType"));
                DataCollectorParams.localIp = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalDataCollector", "DataIP");
                DataCollectorParams.localPort = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalDataCollector", "DataPort"));
                DataCollectorParams.localSerialPort = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalDataCollector", "CommPort");
                DataCollectorParams.localBaud = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalDataCollector", "ComBps"));

                DataCollectorParams.remoteAccessType = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "RemoteDataCollector", "DataType"));
                DataCollectorParams.remoteIp = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "RemoteDataCollector", "DataIP");
                DataCollectorParams.remotePort = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "RemoteDataCollector", "DataPort"));
                DataCollectorParams.remoteSerialPort = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "RemoteDataCollector", "CommPort");
                DataCollectorParams.remoteBaud = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "RemoteDataCollector", "ComBps"));
                
            }
            catch (Exception ex)
            {
                Util.LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("读取称重数据接收参数类型转换失败:" + ex.Message);
            }
        }


        private static void ReadLocalDBParams()
        {
            try
            {
                LocalDBParams.DBLocation = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalDataBase", "DBLocation");
                LocalDBParams.DBPort = Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalDataBase", "DBPort"));
                LocalDBParams.DBType = (DataBaseType)Int32.Parse(XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalDataBase", "DBType"));
                LocalDBParams.DBName = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalDataBase", "DBName");
                LocalDBParams.DBUser = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalDataBase", "DBUser");
                LocalDBParams.DBPwd = XMLUnit.XmlGetValue(ConfigFilePath, "TruckETC", "LocalDataBase", "DBPwd");

            }
            catch (Exception ex)
            {
                Util.LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("读取本地数据库参数类型转换失败:" + ex.Message);
            }
        }
        #endregion
        #region 写入XML配置文件
        // 将配置参数写入到XML配置文件中
        public static void WriteXMLPara()
        {
            if (File.Exists(ConfigFilePath))
            {
                // 写入本地数据库参数
                WriteXMLLocalDBPara();

                // 写入接收数据串口参数
                WriteDataCollectorParams();

                // 写入本地服务器参数
                WriteLocalServerParams();

                // 写入接收IO参数
                WriteIOParams();

                // 写入预读天线参数
                WritePreAntennaParams();

                // 写入交易天线参数
                WriteExchangeAntennaParams();

                // 写入运行参数
                WriteRunParams();
            }
            else
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, "写配置文件错误, 文件不存在:" + ConfigFilePath);
                throw new Exception("写配置文件错误, 文件不存在:" + ConfigFilePath);
            }
        }

        private static void WriteRunParams()
        {
            try
            {
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "SystemMode", "RunMode", InitParams.RunMode.ToString());
            }
            catch (Exception ex)
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("写运行参数错误:" + ex.Message);
            }
        }

        private static void WriteIOParams()
        {
            try
            {
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "IOControl", "Type", IOModuleParams.IOModuleAccessType.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "IOControl", "IP", IOModuleParams.IOModuleIp);
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "IOControl", "Port", IOModuleParams.IOModulePort.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "IOControl", "CommPort", IOModuleParams.IOModuleSerialPort);
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "IOControl", "ComBps", IOModuleParams.IOModuleBaud.ToString());
            }
            catch (Exception ex)
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("写IO设备错误:" + ex.Message);
            }
        }

        private static void WriteExchangeAntennaParams()
        {
            try
            {
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "ExchangeAntenna", "Type", TrdAntennaParams.TrdAntennaAccessType.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "ExchangeAntenna", "IP", TrdAntennaParams.TrdAntennaIp);
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "ExchangeAntenna", "Port", TrdAntennaParams.TrdAntennaPort.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "ExchangeAntenna", "CommPort", TrdAntennaParams.TrdAntennaSerialPort);
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "ExchangeAntenna", "ComBps", TrdAntennaParams.TrdAntennaBaud.ToString());
            }
            catch (Exception ex)
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("写交易天线信息错误:" + ex.Message);
            }
        }
        private static void WritePreAntennaParams()
        {
            try
            {
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "PreAntenna", "Type", PreAntennaParams.PreAntennaAccessType.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "PreAntenna", "IP", PreAntennaParams.PreAntennaIp.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "PreAntenna", "Port", PreAntennaParams.PreAntennaPort.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "PreAntenna", "CommPort", PreAntennaParams.PreAntennaSerialPort.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "PreAntenna", "ComBps", PreAntennaParams.PreAntennaBaud.ToString());

            }
            catch (Exception ex)
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("写预读天线信息错误:" + ex.Message);
            }
        }

        private static void WriteLocalServerParams()
        {
            try
            {
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalServer", "SendFlag", LocalServerParams.LocalServerMode.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalServer", "IP", LocalServerParams.IP);
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalServer", "Port", LocalServerParams.Port.ToString());
            }
            catch (Exception ex)
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("写本地服务器错误:" + ex.Message);
            }
        }

        private static void WriteDataCollectorParams()
        {
            try
            {
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalDataCollector", "DataType", DataCollectorParams.localAccessType.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalDataCollector", "DataIP", DataCollectorParams.localIp);
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalDataCollector", "DataPort", DataCollectorParams.localPort.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalDataCollector", "CommPort", DataCollectorParams.localSerialPort.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalDataCollector", "ComBps", DataCollectorParams.localBaud.ToString());

                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "RemoteDataCollector", "DataType", DataCollectorParams.remoteAccessType.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "RemoteDataCollector", "DataIP", DataCollectorParams.remoteIp);
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "RemoteDataCollector", "DataPort", DataCollectorParams.remotePort.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "RemoteDataCollector", "CommPort", DataCollectorParams.remoteSerialPort.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "RemoteDataCollector", "ComBps", DataCollectorParams.remoteBaud.ToString());
            }
            catch (Exception ex)
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("写称重数据接收设备错误:" + ex.Message);
            }
        }

        private static void WriteXMLLocalDBPara()
        {
            try
            {
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalDataBase", "DBLocation", LocalDBParams.DBLocation);
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalDataBase", "DBPort", LocalDBParams.DBPort.ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalDataBase", "DBType", ((int)LocalDBParams.DBType).ToString());
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalDataBase", "DBName", LocalDBParams.DBName);
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalDataBase", "DBPwd", LocalDBParams.DBPwd);
                XMLUnit.XMLSetValue(ConfigFilePath, "TruckETC", "LocalDataBase", "DBUser", LocalDBParams.DBUser);
            }
            catch (Exception ex)
            {
                LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, ex.Message);
                throw new Exception("写本地数据库错误:" + ex.Message);
            }
        }
        #endregion
    }
}

