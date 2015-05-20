using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TruckETCExitus.Controller;
using TruckETCExitus.Device;
using TruckETCExitus.Etc;
using Util;

namespace TruckETCExitus
{
    public partial class LoginForm : Form
    {
        private int CurModuleNo;                                                            // 控制进度条

        private BackgroundWorker backGWLogin;                                               // 登录进程

        public LoginForm()
        {
            InitializeComponent();

            // 窗体圆角
            SetWindowRegion();
            // 按钮圆角
            SetButtonRegion();
        }
        #region 窗体圆角
        public void SetWindowRegion()
        {
            GraphicsPath FormPath;
            Rectangle rect;
            FormPath = new System.Drawing.Drawing2D.GraphicsPath();
            if (this.WindowState == FormWindowState.Normal)
                rect = new Rectangle(-1, -1, this.Width + 1, this.Height);
            else if (this.WindowState == FormWindowState.Maximized)
                rect = new Rectangle(-1, -1, Screen.PrimaryScreen.Bounds.Width + 1, Screen.PrimaryScreen.Bounds.Height);
            else
                rect = new Rectangle(-1, -1, this.Width + 1, this.Height);
            FormPath = GetRoundedRectPath(rect, 10);
            this.Region = new Region(FormPath);
        }

        //窗体圆角
        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            int diameter = radius;
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
            GraphicsPath path = new GraphicsPath();
            // 左上角
            path.AddArc(arcRect, 185, 90);
            // 右上角
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 275, 90);
            // 右下角
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 356, 90);
            // 左下角
            arcRect.X = rect.Left;
            arcRect.Width += 2;
            arcRect.Height += 2;
            path.AddArc(arcRect, 90, 90);
            path.CloseFigure();
            return path;
        }
        #endregion

        #region 按钮圆角
        private void SetButtonRegion(Button btn, int radius)
        {
            GraphicsPath BtnPath;
            BtnPath = new System.Drawing.Drawing2D.GraphicsPath();
            Rectangle rect = new Rectangle(-1, -1, btn.Width + 1, btn.Height);
            BtnPath = GetRoundedRectPath(rect, radius);
            btn.Region = new Region(BtnPath);
        }
        private void SetButtonRegion()
        {
            SetButtonRegion(btn_login, 10);
            SetButtonRegion(buttonConfig, 10);
        }
        #endregion

        private void btn_close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonConfig_Click(object sender, EventArgs e)
        {
            tmrLogin.Enabled = false;
            PreSetForm pSetting = new PreSetForm();
            pSetting.ShowDialog();
            textBox_User.Focus();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            LoginWindowClear();
        }
        // 清理登录窗口
        private void LoginWindowClear()
        {
            toolStripStatusLabel1.Text = "";
            toolStripProgressBar1.Visible = false;
            textBox_User.Focus();
        }

        #region 登陆部分
        private void btn_login_Click(object sender, EventArgs e)
        {
            tmrLogin.Enabled = false;

            if (btn_login.Text.Equals("登录"))
            {
                buttonConfig.Enabled = false;
                toolStripProgressBar1.Visible = true;
                CurModuleNo = 1;
                btn_login.Text = "停止";

                backGWLogin = new BackgroundWorker();
                backGWLogin.WorkerReportsProgress = true;
                backGWLogin.WorkerSupportsCancellation = true;
                backGWLogin.DoWork += backGWLogin_DoWork;
                backGWLogin.ProgressChanged += backGWLogin_ProgressChanged;
                backGWLogin.RunWorkerCompleted += backGWLogin_RunWorkerCompleted;
                backGWLogin.RunWorkerAsync();
            }
            else
            {
                backGWLogin.CancelAsync();
                ResetStatus();
            }
        }

        private void backGWLogin_DoWork(object sender, DoWorkEventArgs e)
        {
            string msg = "";

            if (backGWLogin.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            else
            {
                if (DataCollectorParams.localAccessType != 0)
                {
                    if (!InitDataCollecter())
                    {
                        backGWLogin.ReportProgress((CurModuleNo) * 100 / InitParams.InitModuleCount, "初始化数据采集器失败");
                        e.Cancel = true;
                        return;
                    }
                    else
                        backGWLogin.ReportProgress((CurModuleNo) * 100 / InitParams.InitModuleCount, "初始化数据采集器成功");
                }
            }
            Thread.Sleep(500);

            if (backGWLogin.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            else
            {
                if (0 != LocalServerParams.LocalServerMode)
                {
                    if (!InitLocSvr())
                    {
                        backGWLogin.ReportProgress((CurModuleNo) * 100 / InitParams.InitModuleCount, "初始化本地服务器失败");
                        e.Cancel = true;
                        return;
                    }
                    else
                        backGWLogin.ReportProgress((CurModuleNo) * 100 / InitParams.InitModuleCount, "初始化本地服务器成功");
                }
            }
            Thread.Sleep(500);

            if (backGWLogin.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            else
            {
                if (0 != IOModuleParams.IOModuleAccessType)
                {
                    if (!InitIOCoils())
                    {
                        backGWLogin.ReportProgress((CurModuleNo) * 100 / InitParams.InitModuleCount, "初始化线圈失败");
                        e.Cancel = true;
                        return;
                    }
                    else
                        backGWLogin.ReportProgress((CurModuleNo) * 100 / InitParams.InitModuleCount, "初始化线圈成功");
                }
            }
            Thread.Sleep(500);

            if (backGWLogin.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            else
            {
                if (0 != PreAntennaParams.PreAntennaAccessType)
                {
                    if (!InitPreAntenna())
                    {
                        backGWLogin.ReportProgress((CurModuleNo) * 100 / InitParams.InitModuleCount, "初始化预读天线失败");
                        e.Cancel = true;
                        return;
                    }
                    else
                        backGWLogin.ReportProgress((CurModuleNo) * 100 / InitParams.InitModuleCount, "初始化预读天线成功");
                }
            }
            Thread.Sleep(500);

            if (backGWLogin.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            else
            {
                if (0 != TrdAntennaParams.TrdAntennaAccessType)
                {
                    if (!InitTrdAntenna())
                    {
                        backGWLogin.ReportProgress((CurModuleNo) * 100 / InitParams.InitModuleCount, "初始化交易天线失败");
                        e.Cancel = true;
                        return;
                    }
                    else
                        backGWLogin.ReportProgress((CurModuleNo) * 100 / InitParams.InitModuleCount, "初始化交易天线成功");
                }
            }
            Thread.Sleep(500);

            if (backGWLogin.CancellationPending)
            {
                e.Cancel = true;
                return;
            }
            else
            {
                Global.SQLDBConn = LoginController.GetSQLServerConnection();
                if (Global.SQLDBConn.State != ConnectionState.Open)
                {
                    backGWLogin.ReportProgress((CurModuleNo) * 100 / InitParams.InitModuleCount, "连接数据库失败");
                    e.Cancel = true;
                    return;
                }
                else
                    backGWLogin.ReportProgress((CurModuleNo) * 100 / InitParams.InitModuleCount, "连接数据库");
            }
            Thread.Sleep(500);
        }
        
        #region 初始化设备

        private bool InitTrdAntenna()
        {
            try
            {
                Global.TrdAntenna = new Antenna(TrdAntennaParams.TrdAntennaAccessType);
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
                if (Global.TrdAntenna.IsConnect())
                    return true;
                else
                    return false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        private bool InitPreAntenna()
        {
            try
            {
                Global.PreAntenna = new Antenna(PreAntennaParams.PreAntennaAccessType);
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
                if (Global.PreAntenna.IsConnect())
                {
                    //Global.PreAntenna.Send(Antenna.)
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool InitIOCoils()
        {
            try
            {
                Global.coils = new Coils(Global.COIL_COUNT);
                Global.coils.Initialize(IOModuleParams.IOModuleAccessType,
                    IOModuleParams.IOModuleSerialPort, IOModuleParams.IOModuleBaud);
                if (Global.coils.IsOpen())
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool InitLocSvr()
        {            
            try
            {
                Global.localServer = new LocalServer(LocalServerParams.LocalServerMode, LocalServerParams.IP, LocalServerParams.Port);
                Global.localServer.Connect();
                if (Global.localServer.IsConnect())
                    return true;
                else
                    return false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        private bool InitDataCollecter()
        {
            try
            {
                int portNo = Convert.ToInt32(DataCollectorParams.localSerialPort.Substring(3, 1));

                Global.dataCollector = new DataCollector();
                if (!DataCollector.WtSys_SetCom(portNo, DataCollectorParams.localBaud))
                {                    
                    LogTools.WriteSystemErrorLog(MethodBase.GetCurrentMethod().DeclaringType, MethodBase.GetCurrentMethod().Name, "数据接收串口打开失败");
                    return false;
                }

                Global.raster = new Raster();
                LogTools.WriteDataCollectorMonitorLog(string.Format("打开本地数据采集端口:{0}", DataCollectorParams.localSerialPort));
                //int status = DataCollector.WtSys_Test();
                //if (status != 0)
                //{
                //    //if (status == 16)
                //    //    MessageBox.Show("数据采集器同步失败，请查看使用说明书'常见问题及解决方法'！");
                //    //else
                //    //    MessageBox.Show("数据采集器有故障，请到系统维护模块查询设备状态！");
                //}

                if (DataCollectorParams.remoteAccessType != 0)
                {
                    int res = Global.dataCollector.OpenRemote(DataCollectorParams.remoteSerialPort, DataCollectorParams.remoteBaud);
                    if (res != 1)
                        return false;
                    LogTools.WriteDataCollectorMonitorLog(string.Format("打开远程数据采集端口:{0}", DataCollectorParams.remoteSerialPort));
                }

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        #endregion
        private void backGWLogin_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                DisposeDevice();
                ResetStatus();   
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void DisposeDevice()
        {
            // 释放数据采集器
            if(Global.dataCollector!= null)
            {
                DataCollector.WtSys_CloseCom();
                Global.dataCollector.CloseRemote();
            }

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
        }

        private void ResetStatus()
        {
            buttonConfig.Enabled = true;
            btn_login.Text = "登录";

            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Visible = false;
        }
        // 显示设备初始化等登陆信息
        private void ShowLoginInfo(string msg, int processNum)
        {
            toolStripStatusLabel1.Text = msg;
            if ((processNum >= toolStripProgressBar1.Minimum) && (processNum <= toolStripProgressBar1.Maximum))
                toolStripProgressBar1.Value = processNum;
            if (processNum > toolStripProgressBar1.Maximum)
                toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
            CurModuleNo += 1;
        }
        private void backGWLogin_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ShowLoginInfo(Convert.ToString(e.UserState), (toolStripProgressBar1.Maximum / (InitParams.InitModuleCount > 0 ? InitParams.InitModuleCount : 8)) * CurModuleNo);
        }
        #endregion

        private void tmrLogin_Tick(object sender, EventArgs e)
        {
            tmrLogin.Enabled = false;
            btn_login_Click(sender, e);
        }
    }
}
