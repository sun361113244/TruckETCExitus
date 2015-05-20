using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Util;

namespace TruckETCExitus.Device
{
    public class Coils
    {
        #region 属性

        private int coilCount = 0;                                          // 线圈数量

        private Coil[] coils;                                               // 线圈状态       

        private Thread thrdSendCir;                                         // 循环发送查询命令线程

        private IOControl ioControl;                                        // 控制IO卡

        private Semaphore isHandleCoilMsg;                                  // 是否正在处理线圈状态

        private byte[] sendCmd = new byte[4] { 0x40, 0x30, 0x31, 0x0d };    // 查询IO命令

        public delegate void CoilRefreshHandler();
        public CoilRefreshHandler coilRefreshHandler;                       // 收到数据时触发

        public Coil[] CoilStatus
        {
            get { return coils; }
        }

        #endregion

        #region 构造函数

        public Coils(int count)
        {
            coilCount = count;
            coils = new Coil[coilCount];
            for (int i = 0; i < coilCount; i++)
                coils[i] = new Coil();

            ioControl = new IOControl7063D();
        }

        #endregion

        #region 方法
        private void SndChkMthd()
        {
            while (true)
            {
                isHandleCoilMsg.WaitOne(2000, true);
                ioControl.Write(sendCmd, 0, 4);
            }
        }
        private void InitSrdCmd()
        {
            this.thrdSendCir = new Thread(new ThreadStart(this.SndChkMthd));

            isHandleCoilMsg = new Semaphore(1, 1);
            thrdSendCir.Start();
        }
        private void Recvfunc(int value)
        {
            try
            {
                if (value != -1)
                {
                    ChangeStatus(value);
                    if (coilRefreshHandler != null)
                        coilRefreshHandler();

                    isHandleCoilMsg.Release();
                }
                else
                {

                    isHandleCoilMsg.Release();
                }
            }
            catch (Exception ex)
            { }
        }
        public int Initialize(int accessType, string ipOrComm, int portOrBaud)
        {
            ioControl.IoRecvHandler = Recvfunc;
            int initStat = ioControl.Initialize(accessType, ipOrComm, portOrBaud);
            InitSrdCmd();

            return initStat;
        }

        public void ChangeStatus(int Status)
        {
            //LogTools.WriteCoilMonitorLog("+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            for (int i = 0; i < coilCount; i++)
            {                
                int coilNum = Status & (int)(Math.Pow(2, i));
                
                if( coilNum > 0)
                    coils[i].ChangeStatus(Coil.CurStatus.NonSheltered , i);
                else
                    coils[i].ChangeStatus(Coil.CurStatus.Sheltered , i);
            }
            //LogTools.WriteCoilMonitorLog("----------------------------------------------------------------------");
        }

        public void Close()
        {
            ioControl.Close();
            if (thrdSendCir != null)
                thrdSendCir.Abort();
        }

        public bool IsOpen()
        {
            return ioControl.IsOpen();
        }
        #endregion



    }
}
