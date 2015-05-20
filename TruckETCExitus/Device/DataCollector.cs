using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TruckETCExitus.Model;
using Util;

namespace TruckETCExitus.Device
{
    /// <summary>
    /// 数据采集类：版本1.1 去掉增加方法
    /// </summary>
    class DataCollector
    {
        #region 常量

        private static int[] g_nWhole_Limit = new int[7] { 20000, 30000, 40000, 50000, 55000, 55000, 55000 };    //超限对照表

        #endregion

        #region 属性

        private SerialPortTools serialPortTools;

        #endregion

        #region 构造函数
        private void DataReceivedHandler(byte[] buffer)
        {

        }
        public DataCollector()
        {
            serialPortTools = new SerialPortTools();
            serialPortTools.dataReceived = DataReceivedHandler;
        }

        #endregion

        #region 外部方法
        [DllImport("WtSys_Dll.dll")]
        public static extern int WtSys_Init(int IniType);

        [DllImport("WtSys_Dll.dll")]
        public static extern int WtSys_Test();

        [DllImport("WtSys_Dll.dll")]
        public static extern bool WtSys_SetCom(int comNo, int bps);

        [DllImport("WtSys_Dll.dll")]
        public static extern bool WtSys_CloseCom();

        [DllImport("WtSys_Dll.dll")]
        public static extern bool WtSys_ClearOne();

        [DllImport("WtSys_Dll.dll")]
        public static extern int WtSys_GetVehicleCount();

        [DllImport("WtSys_Dll.dll")]
        public static extern int WtSys_GetAxisCount(int VehID);

        [DllImport("WtSys_Dll.dll")]
        public static extern int WtSys_GetAxisData(int CarN, int AxisID, int[] Type, int[] AxisWeight, ref int Speed, ref int MeterVer);

        [DllImport("WtSys_Dll.dll")]
        public static extern void WtSys_GetDataFrame([MarshalAs(UnmanagedType.LPArray)] byte[] buff, int length);

        #endregion

        #region 方法

        /// <summary>
        /// 打开远程数据采集串口.
        /// </summary>
        /// <param name="address">ip或者串口号</param>
        /// <param name="portOrBaud">端口号或者波特率</param>
        public int OpenRemote(string address, int portOrBaud)
        {
            return serialPortTools.Open(address, portOrBaud);
        }
        /// <summary>
        /// 使用缓冲区的数据将指定数量的字节写入串行端口。
        /// </summary>
        /// <param name="buffer"> 包含要写入端口的数据的字节数组</param>
        /// <param name="offset"> buffer 参数中从零开始的字节偏移量，从此处开始将字节复制到端口。</param>
        /// <param name="count"> 要写入的字节数。</param>
        /// <returns>1:成功.11:失败.</returns>
        public int WriteRemote(byte[] buffer, int offset, int count)
        {
            return serialPortTools.Write(buffer, offset, count);
        }
        /// <summary>
        /// 远程串口是否打开
        /// </summary>
        /// <returns>1:成功.11:未打开.12:打开异常.</returns>
        public int IsOpenRemote()
        {
            return serialPortTools.IsOpen();
        }
        /// <summary>
        /// 关闭远程串口
        /// </summary>
        public void CloseRemote()
        {
            serialPortTools.Close();
        }

        //获取整车数据。
        //参数：Global.VehData，并修改其值返回。
        //返回：0为无车
        //      1为正常
        //      -1为异常
        public int GetFirst(ref VehData g_sVehData)
        {
            try
            {
                LogTools.WriteDataCollectorMonitorLog("称重数据到来！");
                int VehCount = DataCollector.WtSys_GetVehicleCount(); //获取当前车辆总数
                if (VehCount <= 0)
                    return 0;

                for (int i = 1; i <= 1; i++)
                {
                    int axlenum = 0;//取超限值是使用

                    //获取该车的轴数
                    g_sVehData.Axle_Num = WtSys_GetAxisCount(i);

                    //获取该车的各轴重及总重
                    GetWeightInfo(i, ref g_sVehData.AxisWeight, ref g_sVehData.Whole_Weight);

                    //获取该车的轴型
                    if (GetAxleType(i, ref g_sVehData.Veh_Speed, ref g_sVehData.MeterVer) != -1)
                    {
                        g_sVehData.Axle_Type = GetAxleType(i, ref g_sVehData.Veh_Speed, ref g_sVehData.MeterVer);
                    }

                }
                return 1;
            }
            catch (System.Exception ex)
            {
                return -1;
            }
            finally
            {
                WtSys_ClearOne();//清除已取的车
            }
        }

        //获取缓存中第CarN辆车的轴型
        //参数：CarN
        //返回：-1，没有第CarN辆车
        //      其他，该车的轴型
        public int GetAxleType(int CarN, ref int Veh_Speed, ref int MeterVer)
        {
            if (DataCollector.WtSys_GetVehicleCount() < CarN)
                return -1;

            int[] weight = new int[1] { 0 };
            int[] AxisType = new int[1] { 0 };
            int axisgroupcount = WtSys_GetAxisCount(CarN);

            string AxisTypestr = "";
            for (int k = 0; k < axisgroupcount; k++)
            {
                WtSys_GetAxisData(CarN, k + 1, AxisType, weight, ref Veh_Speed, ref MeterVer);
                AxisTypestr = AxisTypestr + AxisType[0].ToString();
            }
            if (!AxisTypestr.Equals(""))
                return Convert.ToInt32(AxisTypestr);
            else
                return 0;
        }

        //获取缓存中第CarN辆车的各轴重及总重
        //参数：CarN，该车的各轴数组及总重
        //返回：-1，没有第CarN辆车
        //      1，正常读取到缓存中第CarN辆车的各轴重及总重
        public int GetWeightInfo(int CarN, ref int[] SingleAxisWeight_1, ref int weight_1)
        {
            if (DataCollector.WtSys_GetVehicleCount() < CarN)
                return -1;

            int[] weight = new int[1] { 0 };
            int[] type = new int[1] { 0 };
            int speed= 0;
            int veh=0;
            int[] SingleAxisWeight = new int[20] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int Axle_Num = WtSys_GetAxisCount(CarN);
            int whole_weight = 0;
            if (Axle_Num > 0 && Axle_Num <= 47)
            {
                for (int j = 0; j < Axle_Num; j++)
                {
                    //WtSys_SinAxisData(CarN, j + 1, weight);
                    WtSys_GetAxisData(CarN, j + 1, type, weight,ref speed,ref veh);
                    //WtSys_AxisData(i, j + 1, AxisType, weight);
                    if (j <= 20)
                    {
                        SingleAxisWeight[j] = weight[0];
                    }
                    whole_weight = whole_weight + weight[0];

                }
                SingleAxisWeight_1 = SingleAxisWeight;
                weight_1 = whole_weight;
            }
            return 1;
        }
        #endregion
    }
}
