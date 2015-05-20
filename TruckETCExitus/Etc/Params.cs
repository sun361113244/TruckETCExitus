using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Util;

namespace TruckETCExitus.Etc
{
    public struct LocalDBParams
    {
        /// <summary>
        /// 本地数据库实例名或IP
        /// </summary>
        public static string DBLocation;                  

        /// <summary>
        /// 本地数据库监听端口
        /// </summary>
        public static int DBPort;                        

        /// <summary>
        /// 本地数据库类型
        /// </summary>
        public static DataBaseType DBType;               

        /// <summary>
        /// 本地数据库名称
        /// </summary>
        public static string DBName;                      

        /// <summary>
        /// 用户名
        /// </summary>
        public static string DBUser;                      

        /// <summary>
        /// 登录密码
        /// </summary>
        public static string DBPwd;                      

    }

    public struct DataCollectorParams
    {
        /// <summary>
        /// 本地dll连接类型.
        /// 0:不启用.1:串口.2:网口.
        /// </summary>
        public static int localAccessType;

        /// <summary>
        /// 本地dll ip
        /// </summary>
        public static string localIp;

        /// <summary>
        /// 本地dll 端口号
        /// </summary>
        public static int localPort;

        /// <summary>
        /// 本地dll 串口号
        /// </summary>
        public static string localSerialPort;

        /// <summary>
        /// 本地dll 波特率
        /// </summary>
        public static int localBaud;

        /// <summary>
        /// 远程dll连接类型.
        /// 0:不启用.1:串口.2:网口.
        /// </summary>
        public static int remoteAccessType;

        /// <summary>
        /// 远程dll ip
        /// </summary>
        public static string remoteIp;

        /// <summary>
        /// 远程dll 端口号
        /// </summary>
        public static int remotePort;

        /// <summary>
        /// 远程dll 串口号
        /// </summary>
        public static string remoteSerialPort;

        /// <summary>
        /// 远程dll 波特率
        /// </summary>
        public static int remoteBaud;

    }

    public struct LocalServerParams
    {
        /// <summary>
        /// 本地服务器模式 0：不启用，1：tcp模式
        /// </summary>
        public static int LocalServerMode;                    

        /// <summary>
        /// 本地服务器IP（没用）
        /// </summary>
        public static string IP;                       

        /// <summary>
        /// 本地服务器端口
        /// </summary>
        public static int Port;     
         
    }
    
    public struct IOModuleParams
    {
        /// <summary>
        /// IO模块连接类型.
        /// 0:不启用.1:串口.2:网口.
        /// </summary>
        public static int IOModuleAccessType;

        /// <summary>
        /// IO模块 ip
        /// </summary>
        public static string IOModuleIp;

        /// <summary>
        /// IO模块 端口号
        /// </summary>
        public static int IOModulePort;

        /// <summary>
        /// IO模块 串口号
        /// </summary>
        public static string IOModuleSerialPort;

        /// <summary>
        /// IO模块 波特率
        /// </summary>
        public static int IOModuleBaud;

    }

    public struct PreAntennaParams
    {
        /// <summary>
        /// 预读天线连接类型.
        /// 0:不启用.1:串口.2:网口.
        /// </summary>
        public static int PreAntennaAccessType;

        /// <summary>
        /// 预读天线 ip
        /// </summary>
        public static string PreAntennaIp;

        /// <summary>
        /// 预读天线 端口号
        /// </summary>
        public static int PreAntennaPort;

        /// <summary>
        /// 预读天线 串口号
        /// </summary>
        public static string PreAntennaSerialPort;

        /// <summary>
        /// 预读天线 波特率
        /// </summary>
        public static int PreAntennaBaud;

    }

    public struct TrdAntennaParams
    {
        /// <summary>
        /// 交易天线连接类型.
        /// 0:不启用.1:串口.2:网口.
        /// </summary>
        public static int TrdAntennaAccessType;

        /// <summary>
        /// 交易天线 ip
        /// </summary>
        public static string TrdAntennaIp;

        /// <summary>
        /// 交易天线 端口号
        /// </summary>
        public static int TrdAntennaPort;

        /// <summary>
        /// 交易天线 串口号
        /// </summary>
        public static string TrdAntennaSerialPort;

        /// <summary>
        /// 交易天线 波特率
        /// </summary>
        public static int TrdAntennaBaud;

    }

    public class InitParams
    {
        /// <summary>
        /// 运行模式，目前4种:0:入口有线圈。1.入口无线圈。2.出口有线圈。3.出口无线圈。
        /// </summary>
        public static int RunMode;

        /// <summary>
        /// 初始化模块数量,会在两个地方设置.
        /// 1.读配置文件(一定有)
        /// 2.确认配置参数(不一定有)
        /// </summary>
        public static int InitModuleCount = 1;

        /// <summary>
        /// 是否启动软件UI
        /// </summary>
        public static bool UIEnabled = true;

        public static void SetInitModuleCount()
        {
            InitModuleCount = 1;

            if (DataCollectorParams.localAccessType != 0 || DataCollectorParams.remoteAccessType != 0)
                InitModuleCount += 1;

            if (LocalServerParams.LocalServerMode != 0)
                InitModuleCount += 1;


            if (IOModuleParams.IOModuleAccessType != 0)
                InitModuleCount += 1;

            if (PreAntennaParams.PreAntennaAccessType != 0)
                InitModuleCount += 1;

            if (TrdAntennaParams.TrdAntennaAccessType != 0)
                InitModuleCount += 1;
        }
    }
}
