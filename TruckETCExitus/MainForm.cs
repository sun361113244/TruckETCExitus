using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TruckETCExitus.Device;
using TruckETCExitus.Etc;
using TruckETCExitus.Model;
using Util;

namespace TruckETCExitus
{
    public partial class MainForm : Form
    {

        private int PAGENUM = 5;                                                            // 切换面板数量  
           

        private TruckETCContext truckETCContext;                           

        public MainForm()
        {
            InitializeComponent();
            
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Hide();
            Form loginForm = new LoginForm();
            ReadWritePara.ReadXMLPara();

            loginForm.ShowDialog();
            if (loginForm.DialogResult == DialogResult.OK)
            {
                this.Show();
                InitModuleHandlers();
                tmrConnStat.Enabled = true;
                tmrRshRunParams.Enabled = true;
            }
            else
            {
                this.Close();
            }
        }

        private void InitModuleHandlers()
        {
            switch (InitParams.RunMode)
            {
                case 0:
                    break;
                case 1:
                    truckETCContext = new TruckETCContext(new EntranceNonCoilState()); 
                    break;
                case 2:
                    truckETCContext = new TruckETCContext(new ExitusCoilState());               
                    break;
                case 3:
                    truckETCContext = new TruckETCContext(new ExitusNonCoilState());
                    break;
                case 4:
                    truckETCContext = new TruckETCContext(new ExitusLiZhiCoilState());
                    break;
                case 5:
                    truckETCContext = new TruckETCContext(new ExitusLiZhiC2CoilState());
                    break;
                case 6:
                    truckETCContext = new TruckETCContext(new ExitusLiZhiC2LocCoilState());
                    break;
                default:
                    throw new Exception("无此运行状态:" + InitParams.RunMode);
            }

            truckETCContext.InitControl(btnUICtrl);

            if(LocalServerParams.LocalServerMode!=0)
            {
                Global.localServer.locSrvConnHandler = LocServerConnHandler;
                Global.localServer.locSrvCloseHandler = LocServerCloseHandler;
                Global.localServer.locSrvRecvDataHandler = LocServerRecvDataHandler;
            }

            if(IOModuleParams.IOModuleAccessType != 0)
            {
                Global.coils.coilRefreshHandler = RefreshCoilStatus;
            }

            if(PreAntennaParams.PreAntennaAccessType != 0)
            {
                Global.PreAntenna.antennaConnHandler = PreAntConnHandler;
                Global.PreAntenna.antennaCloseHandler = PreAntCloseHandler;
                Global.PreAntenna.antennarecvDataHandler = PreAntRecvDataHandler;
            }

            if (TrdAntennaParams.TrdAntennaAccessType != 0)
            {
                Global.TrdAntenna.antennaConnHandler = TrdAntConnHandler;
                Global.TrdAntenna.antennaCloseHandler = TrdAntCloseHandler;
                Global.TrdAntenna.antennarecvDataHandler = TrdAntRecvDataHandler;
            }
        }

        #region 本地服务器句柄

        private void LocServerConnHandler(CSUnit csUnit)
        {
            truckETCContext.HandleLocSrvConn(csUnit, rtxtLocSvr);
        }
        private void LocServerCloseHandler(CSUnit csUnit)
        {
            truckETCContext.HandleLocSrvClose(csUnit, rtxtLocSvr);
        }
        private void LocServerRecvDataHandler(CSUnit csUnit)
        {
            truckETCContext.HandleLocSrvRecvData(csUnit, rtxtLocSvr);
        }        

        #endregion

        #region 线圈句柄

        private void UpDateCoilAction()
        {
            truckETCContext.HandleRefreshCoilStatus();
        }
        private void UpDateCoilUI()
        {
            CheckForIllegalCrossThreadCalls = false;
            if (InitParams.UIEnabled)
            {
                if (Global.coils.CoilStatus[0].StatNow == Coil.CurStatus.NonSheltered)
                    pcbDI0.Image = TruckETCExitus.Properties.Resources.green;
                else
                    pcbDI0.Image = TruckETCExitus.Properties.Resources.red;
                switch (Global.coils.CoilStatus[0].CoilStat)
                {
                    case Coil.CoilStatus.NonSheltered:
                        lblcoilStat1.Text = "未遮挡";
                        break;
                    case Coil.CoilStatus.Trigger:
                        lblcoilStat1.Text = "触发";
                        break;
                    case Coil.CoilStatus.End:
                        lblcoilStat1.Text = "收尾";
                        break;
                    case Coil.CoilStatus.Sheltered:
                        lblcoilStat1.Text = "遮挡";
                        break;
                    default:
                        lblcoilStat1.Text = "未知";
                        break;

                }

                if (Global.coils.CoilStatus[1].StatNow == Coil.CurStatus.NonSheltered)
                    pcbDI1.Image = TruckETCExitus.Properties.Resources.green;
                else
                    pcbDI1.Image = TruckETCExitus.Properties.Resources.red;
                switch (Global.coils.CoilStatus[1].CoilStat)
                {
                    case Coil.CoilStatus.NonSheltered:
                        lblcoilStat2.Text = "未遮挡";
                        break;
                    case Coil.CoilStatus.Trigger:
                        lblcoilStat2.Text = "触发";
                        break;
                    case Coil.CoilStatus.End:
                        lblcoilStat2.Text = "收尾";
                        break;
                    case Coil.CoilStatus.Sheltered:
                        lblcoilStat2.Text = "遮挡";
                        break;
                    default:
                        lblcoilStat2.Text = "未知";
                        break;

                }

                if (Global.coils.CoilStatus[2].StatNow == Coil.CurStatus.NonSheltered)
                    pcbDI2.Image = TruckETCExitus.Properties.Resources.green;
                else
                    pcbDI2.Image = TruckETCExitus.Properties.Resources.red;
                switch (Global.coils.CoilStatus[2].CoilStat)
                {
                    case Coil.CoilStatus.NonSheltered:
                        lblcoilStat3.Text = "未遮挡";
                        break;
                    case Coil.CoilStatus.Trigger:
                        lblcoilStat3.Text = "触发";
                        break;
                    case Coil.CoilStatus.End:
                        lblcoilStat3.Text = "收尾";
                        break;
                    case Coil.CoilStatus.Sheltered:
                        lblcoilStat3.Text = "遮挡";
                        break;
                    default:
                        lblcoilStat3.Text = "未知";
                        break;

                }

                if (Global.coils.CoilStatus[3].StatNow == Coil.CurStatus.NonSheltered)
                    pcbDI3.Image = TruckETCExitus.Properties.Resources.green;
                else
                    pcbDI3.Image = TruckETCExitus.Properties.Resources.red;
                switch (Global.coils.CoilStatus[3].CoilStat)
                {
                    case Coil.CoilStatus.NonSheltered:
                        lblcoilStat4.Text = "未遮挡";
                        break;
                    case Coil.CoilStatus.Trigger:
                        lblcoilStat4.Text = "触发";
                        break;
                    case Coil.CoilStatus.End:
                        lblcoilStat4.Text = "收尾";
                        break;
                    case Coil.CoilStatus.Sheltered:
                        lblcoilStat4.Text = "遮挡";
                        break;
                    default:
                        lblcoilStat4.Text = "未知";
                        break;

                }

                if (Global.coils.CoilStatus[4].StatNow == Coil.CurStatus.NonSheltered)
                    pcbDI4.Image = TruckETCExitus.Properties.Resources.green;
                else
                    pcbDI4.Image = TruckETCExitus.Properties.Resources.red;
                switch (Global.coils.CoilStatus[4].CoilStat)
                {
                    case Coil.CoilStatus.NonSheltered:
                        lblcoilStat5.Text = "未遮挡";
                        break;
                    case Coil.CoilStatus.Trigger:
                        lblcoilStat5.Text = "触发";
                        break;
                    case Coil.CoilStatus.End:
                        lblcoilStat5.Text = "收尾";
                        break;
                    case Coil.CoilStatus.Sheltered:
                        lblcoilStat5.Text = "遮挡";
                        break;
                    default:
                        lblcoilStat5.Text = "未知";
                        break;

                }

                if (Global.coils.CoilStatus[5].StatNow == Coil.CurStatus.NonSheltered)
                    pcbDI5.Image = TruckETCExitus.Properties.Resources.green;
                else
                    pcbDI5.Image = TruckETCExitus.Properties.Resources.red;
                switch (Global.coils.CoilStatus[5].CoilStat)
                {
                    case Coil.CoilStatus.NonSheltered:
                        lblcoilStat6.Text = "未遮挡";
                        break;
                    case Coil.CoilStatus.Trigger:
                        lblcoilStat6.Text = "触发";
                        break;
                    case Coil.CoilStatus.End:
                        lblcoilStat6.Text = "收尾";
                        break;
                    case Coil.CoilStatus.Sheltered:
                        lblcoilStat6.Text = "遮挡";
                        break;
                    default:
                        lblcoilStat6.Text = "未知";
                        break;

                }

                if (Global.coils.CoilStatus[6].StatNow == Coil.CurStatus.NonSheltered)
                    pcbDI6.Image = TruckETCExitus.Properties.Resources.green;
                else
                    pcbDI6.Image = TruckETCExitus.Properties.Resources.red;
                switch (Global.coils.CoilStatus[6].CoilStat)
                {
                    case Coil.CoilStatus.NonSheltered:
                        lblcoilStat7.Text = "未遮挡";
                        break;
                    case Coil.CoilStatus.Trigger:
                        lblcoilStat7.Text = "触发";
                        break;
                    case Coil.CoilStatus.End:
                        lblcoilStat7.Text = "收尾";
                        break;
                    case Coil.CoilStatus.Sheltered:
                        lblcoilStat7.Text = "遮挡";
                        break;
                    default:
                        lblcoilStat7.Text = "未知";
                        break;

                }

                if (Global.coils.CoilStatus[7].StatNow == Coil.CurStatus.NonSheltered)
                    pcbDI7.Image = TruckETCExitus.Properties.Resources.green;
                else
                    pcbDI7.Image = TruckETCExitus.Properties.Resources.red;
                switch (Global.coils.CoilStatus[7].CoilStat)
                {
                    case Coil.CoilStatus.NonSheltered:
                        lblcoilStat8.Text = "未遮挡";
                        break;
                    case Coil.CoilStatus.Trigger:
                        lblcoilStat8.Text = "触发";
                        break;
                    case Coil.CoilStatus.End:
                        lblcoilStat8.Text = "收尾";
                        break;
                    case Coil.CoilStatus.Sheltered:
                        lblcoilStat8.Text = "遮挡";
                        break;
                    default:
                        lblcoilStat8.Text = "未知";
                        break;

                }
            }
        }
        private void RefreshCoilStatus()
        {
            //执行线圈逻辑
            UpDateCoilAction();

            //更新UI状态
            UpDateCoilUI();
        }

        #endregion

        #region 预读天线句柄

        private void PreAntConnHandler(CSUnit csUnit)
        {
            truckETCContext.HandlePreAntConn(csUnit, rtxtPreAnt);
        }
        private void PreAntCloseHandler(CSUnit csUnit)
        {
            truckETCContext.HandlePreAntClose(csUnit, rtxtPreAnt);
        }
        private void PreAntRecvDataHandler(CSUnit csUnit)
        {
            truckETCContext.HandlePreAntRecvData(csUnit, rtxtPreAnt);
        }       


        #endregion

        #region 交易天线句柄

        private void TrdAntConnHandler(CSUnit csUnit)
        {
            truckETCContext.HandleTrdAntConn(csUnit, rtxtTrdAnt);
        }
        private void TrdAntCloseHandler(CSUnit csUnit)
        {
            truckETCContext.HandleTrdAntClose(csUnit, rtxtTrdAnt);
        }
        private void TrdAntRecvDataHandler(CSUnit csUnit)
        {
            truckETCContext.HandleTrdAntRecvData(csUnit, rtxtTrdAnt);
        }

        #endregion

        protected override void DefWndProc(ref Message m)
        {
            //**************************处理缓存数据到来消息**************************
            if (m.Msg == Convert.ToInt32(Global.WM_DATAFRAMECOME))
            {
                HandleDataFrameComeMsg(ref m);
                return;
            }
            //**************************缓存消息处理完毕******************************

            //**************************处理车辆数据到来消息**************************
            if (m.Msg == Convert.ToInt32(Global.WM_DATA))
            {
                HandleVehComeMsg(ref m);
                return;
            }
            //**************************来数消息处理完毕**************************

            //**************************处理车辆到位消息**************************
            if (m.Msg == Convert.ToInt32(Global.WM_VEH_RASTER_COME))
            {
                HandleVehRasterComeMsg(ref m);
                return;
            }
            //**************************车辆到位消息处理完毕**************************

            //**************************处理光栅数据到来消息**************************
            if (m.Msg == Convert.ToInt32(Global.WM_RASTER_COME))
            {
                HandleRasterComeMsg(ref m);
                return;
            }
            //**************************光栅消息处理完毕**************************

           

            //若以上都不是，则执行系统默认消息处理函数
            base.DefWndProc(ref m);

        }
        #region 仪表消息处理函数

        private void HandleDataFrameComeMsg(ref Message m)
        {
            byte[] buffer = new byte[(int)m.WParam + 1];
            if(truckETCContext != null)
                truckETCContext.HandleDataFrameComeMsg(buffer, rtxtMeter);
        }        

        private void HandleVehComeMsg(ref Message m)
        {
            VehData g_sVehData = new VehData();
            //填充车辆信息
            int tmp = Global.dataCollector.GetFirst(ref g_sVehData);

            if (truckETCContext != null)
                truckETCContext.HandleVehComeMsg(tmp, g_sVehData, rtxtMeter);

        }

        private void HandleVehRasterComeMsg(ref Message m)
        {
            Global.raster.UpdateRasterStat((int)m.WParam);

            if (truckETCContext != null)
                truckETCContext.HandleVehRasterComeMsg(rtxtMeter);
        }

        private void HandleRasterComeMsg(ref Message m)
        {
            Global.raster.UpdateRasterStat((int)m.WParam);
            if (truckETCContext != null)
                truckETCContext.HandleRasterComeMsg(rtxtMeter);
        }
        #endregion
        

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ConfirmQuit();            
        }
        private void ConfirmQuit()
        {
            DialogResult result = MessageBox.Show("您确认要退出软件吗？", "请确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (System.Windows.Forms.DialogResult.Yes == result)
                QuitProcess();
            else
                return;
        }
        private void QuitProcess()
        {
            // 释放本地服务器
            if (Global.localServer != null)
            {
                Global.localServer.Close();
            }

            // 释放IO模块
            if (Global.coils != null)
            {
                Global.coils.Close();
            }

            // 释放预读天线
            if (Global.PreAntenna != null)
            {
                Global.PreAntenna.Close();               
            }

            // 释放交易天线
            if (Global.TrdAntenna != null)
            {
                Global.TrdAntenna.Close();
            }

            Process.GetCurrentProcess().Kill(); 
        }

        private void tmrConnStat_Tick(object sender, EventArgs e)
        {
            // 更新时钟
            lblCurTmr.Text = DateTime.Now.ToString();

            // 更新远端DLL连接状态
            if(Global.dataCollector != null)
            {
                if (Global.dataCollector.IsOpenRemote() == 1)
                    lblRmtDLLConnStat.Text = "远端DLL状态:连接";
                else
                    lblRmtDLLConnStat.Text = "远端DLL状态:断开";
            }
            else
                lblRmtDLLConnStat.Text = "远端DLL状态:断开";

            // 本地服务器状态
            if (Global.localServer != null)
            {
                lblLocSvrConnStat.Text = string.Format("本地服务器连接:{0}",Global.localServer.GetConnectionCount());
            }
            else
                lblLocSvrConnStat.Text = "本地服务器:未启动";

            // IO模块状态
            if(Global.coils != null)
            {
                if (Global.coils.IsOpen())
                    lblIOModuleConnStat.Text = "IO模块:连接";
                else
                    lblIOModuleConnStat.Text = "IO模块:断开";

            }
            else
            {
                lblIOModuleConnStat.Text = "IO模块:未启动";
            }

            // 预读天线
            if (Global.PreAntenna != null)
            {
                if (Global.PreAntenna.IsConnect())
                    lblPreAntConnStat.Text = "预读天线:连接";
                else
                {
                    lblPreAntConnStat.Text = "预读天线:断开";
                    try
                    {
                        switch (PreAntennaParams.PreAntennaAccessType)
                        {
                            case 1:
                                Global.PreAntenna.Connect(PreAntennaParams.PreAntennaSerialPort, PreAntennaParams.PreAntennaBaud);
                                break;
                            case 2:
                                Global.PreAntenna.Connect(PreAntennaParams.PreAntennaIp, PreAntennaParams.PreAntennaPort);
                                break;
                            default:

                                break;
                        }
                    }
                    catch(Exception ex)
                    {

                    }
                }
            }
            else
            {
                lblPreAntConnStat.Text = "预读天线:未启动";
            }

            // 交易天线
            if (Global.TrdAntenna != null)
            {
                if (Global.TrdAntenna.IsConnect())
                    lblTrdAntConnStat.Text = "交易天线:连接";
                else
                {
                    lblTrdAntConnStat.Text = "交易天线:断开";
                    try
                    {
                        switch (TrdAntennaParams.TrdAntennaAccessType)
                        {
                            case 1:
                                Global.TrdAntenna.Connect(TrdAntennaParams.TrdAntennaSerialPort, TrdAntennaParams.TrdAntennaBaud);
                                break;
                            case 2:
                                Global.TrdAntenna.Connect(TrdAntennaParams.TrdAntennaIp, TrdAntennaParams.TrdAntennaPort);
                                break;
                            default:

                                break;
                        }
                    }
                    catch(Exception ex)
                    { }
                }

            }
            else
            {
                lblTrdAntConnStat.Text = "交易天线:未启动";
            }
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            ConfirmQuit(); 
        }

         // 切换面板
        private void btn_Click(object sender, EventArgs e)
        {
            String btn = ((Button)sender).Name;

            for (int i = 0; i < PAGENUM; i++)
            {
                Control control = Controls.Find("btn" + i.ToString(), true)[0];
                if (btn.Contains(i.ToString()))
                {
                    tabControl1.SelectedIndex = i;
                    ((Button)control).BackColor = Color.LightBlue;
                }
                else
                {
                    ((Button)control).BackColor = Color.Transparent;
                }
            }
        }

        private void tmrRshRunParams_Tick(object sender, EventArgs e)
        {
            if (InitParams.UIEnabled)
            {
                switch (Global.prdAlartStat1)
                {
                    case Global.AlartStat.NonAlarmed:
                        pctprdAlartStat.Image = TruckETCExitus.Properties.Resources.green;
                        break;
                    case Global.AlartStat.Alarmed:
                        pctprdAlartStat.Image = TruckETCExitus.Properties.Resources.red;
                        break;
                    default:
                        pctprdAlartStat.Image = null;
                        break;
                }

                switch (Global.trdAlartStat1)
                {
                    case Global.AlartStat.NonAlarmed:
                        pctTrdAlartStat.Image = TruckETCExitus.Properties.Resources.green;
                        break;
                    case Global.AlartStat.Alarmed:
                        pctTrdAlartStat.Image = TruckETCExitus.Properties.Resources.red;
                        break;
                    default:
                        pctTrdAlartStat.Image = null;
                        break;
                }
                txtPreQueue.Text = Global.preQueue.GetOBUQueueString() + Global.preQueue.GetVehQueueString();
                txtTrdQueue.Text = Global.exchangeQueue.GetOBUQueueString() + Global.exchangeQueue.GetVehQueueString();
                textBox2.Text = showObuData(Global.preOBUQueue);

                truckETCContext.HandleShowRunParams(rtxtParams);
            }

        }

        private string showObuData(Queue<OBUData> obuQueue)
        {
            Object toStringLock = new Object();
            lock (toStringLock)
            {
                StringBuilder sb = new StringBuilder("");
                foreach (OBUData elem in obuQueue)
                {
                    sb.Append(string.Format("OBU号:{0}\r\n", elem.ObuNum));
                }

                return sb.ToString();
            }
        }
        #region 运行设置
        private void btnUICtrl_Click(object sender, EventArgs e)
        {
            if(InitParams.UIEnabled)
            {
                InitParams.UIEnabled = false;
            }
            else
            {
                InitParams.UIEnabled = true;
            }
            truckETCContext.InitControl(btnUICtrl);
        }

        #endregion

        #region 测试
        private void btnInitRunParams_Click(object sender, EventArgs e)
        {
            truckETCContext.HandleReSetParamsCmd();         
        }


        private void btnInitTrdAlarm_Click(object sender, EventArgs e)
        {

        }

        #endregion

        private void tmrMonitor_Tick(object sender, EventArgs e)
        {
            Process CurrentProcess = Process.GetCurrentProcess();

            string memery = (CurrentProcess.WorkingSet64 / 1024 / 1024).ToString() + "M (" + (CurrentProcess.WorkingSet64 / 1024).ToString() + "KB)";//占用内存
            string cpuVol = CurrentProcess.ProcessorAffinity.ToString();
            string threadcount = CurrentProcess.Threads.Count.ToString();//线程

            string res = string.Format("cpu使用量:{0}::内存使用量:{1}::线程数:{2}", cpuVol, memery, threadcount);
            LogTools.WriteSystemMonitorLog(res);
        }   

    }
}
