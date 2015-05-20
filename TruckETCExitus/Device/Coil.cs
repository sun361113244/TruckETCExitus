using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Util;

namespace TruckETCExitus.Device
{
    public class Coil
    {
        public enum CurStatus
        {
            NonSheltered = 1,                                   // 未遮挡
            Sheltered                                           // 遮挡
        };
        public enum CoilStatus
        {
            NonSheltered = 1,                                   // 未遮挡
            Trigger,                                            // 触发
            Sheltered,                                          // 遮挡
            End                                                 // 收尾
        };

        private CurStatus preStat = CurStatus.NonSheltered;
        private CurStatus Stat = CurStatus.NonSheltered;

        private CoilStatus coilStat = CoilStatus.NonSheltered;

        public CurStatus StatNow
        {
            get { return Stat; }
        }

        public CoilStatus CoilStat
        {
            get { return coilStat; }
        }

        public void ChangeStatus(CurStatus statNow, int i)
        {
            if (statNow != Stat)
            {
                if (statNow == CurStatus.NonSheltered)
                {
                    //LogTools.WriteCoilMonitorLog("线圈" + i + "收尾");
                    coilStat = CoilStatus.End;
                }
                else
                {
                    //LogTools.WriteCoilMonitorLog("线圈" + i + "触发");
                    coilStat = CoilStatus.Trigger;
                }
                preStat = Stat;
                Stat = statNow;
            }
            else
            {
                if (statNow == CurStatus.NonSheltered)
                    coilStat = CoilStatus.NonSheltered;
                else
                    coilStat = CoilStatus.Sheltered;
            }
        }
    }
}
