using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TruckETCExitus.Model
{
    public class ExchangeQueue
    {
        public Queue<VehData> vehQueue;
        public Queue<OBUData> obuQueue;

        private Object toStringlock = new Object();

        public ExchangeQueue()
        {
            vehQueue = new Queue<VehData>();
            obuQueue = new Queue<OBUData>();
        }

        public string GetVehQueueString()
        {
            lock (toStringlock)
            {
                StringBuilder sb = new StringBuilder("");
                foreach (VehData elem in vehQueue)
                {
                    sb.Append(string.Format("{0}轴车,轴型:{1},重量:{2},检测时间:{3}\r\n",
                        elem.Axle_Num, elem.Axle_Type, elem.Whole_Weight, elem.Check_DT.ToString()));
                }

                return sb.ToString();
            }
        }
        public string GetOBUQueueString()
        {
            lock (toStringlock)
            {
                StringBuilder sb = new StringBuilder("");
                foreach (OBUData elem in obuQueue)
                {
                    sb.Append(string.Format("OBU号:{0}\r\n", elem.ObuNum));
                }

                return sb.ToString();
            }
        }
        public string GetExchangeQueueInfo()
        {
            StringBuilder sb = new StringBuilder("");
            int count = obuQueue.Count > vehQueue.Count ? obuQueue.Count : vehQueue.Count;
            
            return sb.ToString();
        }

        public void Clear()
        {
            obuQueue.Clear();
            vehQueue.Clear();
        }
    }
}
