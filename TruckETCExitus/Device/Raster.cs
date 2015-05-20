using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Util;

namespace TruckETCExitus.Device
{
    public class Raster
    {
        public enum CurStat
        {
            NonSheltered = 1,                                   // 未遮挡
            Sheltered                                           // 遮挡
        };
        public enum RasterStat
        {
            NonSheltered = 1,                                   // 未遮挡
            Trigger,                                            // 触发
            Sheltered,                                          // 遮挡
            End                                                 // 收尾
        };

        #region 属性

        private int vehStatWord;

        private CurStat preStatus = CurStat.NonSheltered;       // 光栅之前状态。1:未遮挡，2:遮挡  

        private CurStat curStatus = CurStat.NonSheltered;       // 光栅当前状态。1:未遮挡，2:遮挡        

        private int vehCount;                                   // 待称重车辆数

        private RasterStat rasterStat = RasterStat.NonSheltered;// 光栅状态        

        public int VehStatWord
        {
            get { return vehStatWord; }
        }
        public CurStat CurStatus
        {
            get { return curStatus; }
        }
        public int VehCount
        {
            get { return vehCount; }
        }
        public RasterStat RasterStatus
        {
            get { return rasterStat; }
        }

        #endregion
        public void UpdateRasterStat(int value)
        {
            LogTools.WriteDataCollectorMonitorLog("光栅数据帧到来！value = " + value);
            this.vehStatWord = value;
            this.vehCount = (value & 8) * 2 + (value & 4);

            if ((value & 2) > 0)
                curStatus = CurStat.Sheltered;
            else
                curStatus = CurStat.NonSheltered;

            if (curStatus != preStatus)
            {
                if (curStatus == CurStat.Sheltered)
                    rasterStat = RasterStat.Trigger;
                else
                    rasterStat = RasterStat.End;
                preStatus = curStatus;
            }
            else
            {
                if (curStatus == CurStat.Sheltered)
                    rasterStat = RasterStat.Sheltered;
                else
                    rasterStat = RasterStat.NonSheltered;
            }

        }

    }
}
