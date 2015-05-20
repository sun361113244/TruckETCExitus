using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TruckETCExitus.Controller;
using TruckETCExitus.Etc;
using Util;

namespace TruckETCExitus
{
    public partial class PreSetForm : Form
    {

        private BackgroundWorker loginWorker;                                                   // 验证登陆线程

        public PreSetForm()
        {
            InitializeComponent();

        }
        //服务器模式选择交互
        private void ChangeStatus(object sender, EventArgs e)
        {
            if (tcpRadio.Checked)
            {
                pnlTcp.Visible = true;
                pnlComm.Visible = false;
            }
            else if (commRadio.Checked)
            {
                pnlTcp.Visible = false;
                pnlComm.Visible = true;
            }
            else
            {
                pnlTcp.Visible = false;
                pnlComm.Visible = false;
            }
        }
        private void changePreAntennaStat(object sender, EventArgs e)
        {
            if (0 == cmbPreCollect.SelectedIndex)
            {
                cmbPreSerl.Enabled = false;
                cmbPreBaud.Enabled = false;
                txtPreIP.Enabled = false;
                txtPrePort.Enabled = false;
            }
            else
            {
                if (1 == cmbPreCollect.SelectedIndex)
                {
                    cmbPreSerl.Enabled = true;
                    cmbPreBaud.Enabled = true;
                    txtPreIP.Enabled = false;
                    txtPrePort.Enabled = false;
                }
                else
                {
                    cmbPreSerl.Enabled = false;
                    cmbPreBaud.Enabled = false;
                    txtPreIP.Enabled = true;
                    txtPrePort.Enabled = true;
                }
            }
        }
        private void changeExchangeAntennaStat(object sender, EventArgs e)
        {
            if (0 == cmbExchangeType.SelectedIndex)
            {
                cmbExchangeCommPort.Enabled = false;
                cmbExchangeBaud.Enabled = false;
                txtExchangeIP.Enabled = false;
                txtExchangePort.Enabled = false;
            }
            else
            {
                if (1 == cmbExchangeType.SelectedIndex)
                {
                    cmbExchangeCommPort.Enabled = true;
                    cmbExchangeBaud.Enabled = true;
                    txtExchangeIP.Enabled = false;
                    txtExchangePort.Enabled = false;
                }
                else
                {
                    cmbExchangeCommPort.Enabled = false;
                    cmbExchangeBaud.Enabled = false;
                    txtExchangeIP.Enabled = true;
                    txtExchangePort.Enabled = true;
                }
            }
        }
        private void CopyFromGlobal()
        {
            //本地数据库
            txtDBIp.Text = LocalDBParams.DBLocation;
            txtDBPort.Text = LocalDBParams.DBPort.ToString();
            txtDBName.Text = LocalDBParams.DBName;
            cmbDBType.SelectedIndex = (int)LocalDBParams.DBType;
            txtDBUserName.Text = LocalDBParams.DBUser;
            txtDBPwd.Text = LocalDBParams.DBPwd;

            if (LocalDBParams.DBType == DataBaseType.SQLServer)
                cmbDBType.Text = "SQLServer";
            else
                if (LocalDBParams.DBType == DataBaseType.SQLServerExpress)
                    cmbDBType.Text = "SQLServerExpress";

            //数据采集
            cmbLocType.SelectedIndex = DataCollectorParams.localAccessType;
            cmbLocComm.SelectedItem = DataCollectorParams.localSerialPort;
            cmbLocBaud.SelectedItem = DataCollectorParams.localBaud.ToString();
            txtLocIP.Text = DataCollectorParams.localIp;
            txtLocPort.Text = DataCollectorParams.localPort.ToString();
            cmbRmtType.SelectedIndex = DataCollectorParams.remoteAccessType;
            cmbRmtComm.SelectedItem = DataCollectorParams.remoteSerialPort;
            cmbRmtBaud.SelectedItem = DataCollectorParams.remoteBaud.ToString();
            txtRmtIP.Text = DataCollectorParams.remoteIp;
            txtRmtPort.Text = DataCollectorParams.remotePort.ToString();

            //本地服务器设置
            switch (LocalServerParams.LocalServerMode)
            {
                case 0:
                    nonUpRadio.Checked = true;
                    tcpRadio.Checked = false;
                    commRadio.Checked = false;
                    break;
                case 2:
                    nonUpRadio.Checked = false;
                    tcpRadio.Checked = true;
                    commRadio.Checked = false;
                    break;
                default:
                    break;
            }
            ChangeStatus(null, null);

            txtTCPServerIP.Text = LocalServerParams.IP;
            txtTCPServerPort.Text = LocalServerParams.Port.ToString();

            //IO设置
            cmbIOCollectType.SelectedIndex = IOModuleParams.IOModuleAccessType;
            cmbSerialPort.SelectedItem = IOModuleParams.IOModuleSerialPort;
            cmbIOBaud.SelectedItem = IOModuleParams.IOModuleBaud.ToString();
            txtIOIP.Text = IOModuleParams.IOModuleIp;
            txtIOPort.Text = IOModuleParams.IOModulePort.ToString();

            //运行状态设置
            cmbRunState.SelectedIndex = InitParams.RunMode;

            //预读天线设置
            cmbPreCollect.SelectedIndex = PreAntennaParams.PreAntennaAccessType;
            changePreAntennaStat();
            txtPreIP.Text = PreAntennaParams.PreAntennaIp;
            txtPrePort.Text = PreAntennaParams.PreAntennaPort.ToString();
            cmbPreSerl.SelectedItem = PreAntennaParams.PreAntennaSerialPort.ToString();
            cmbPreBaud.SelectedItem = PreAntennaParams.PreAntennaBaud.ToString();

            //交易天线设置
            cmbExchangeType.SelectedIndex = TrdAntennaParams.TrdAntennaAccessType;
            changeExchangeAntennaStat();
            txtExchangeIP.Text = TrdAntennaParams.TrdAntennaIp;
            txtExchangePort.Text = TrdAntennaParams.TrdAntennaPort.ToString();
            cmbExchangeCommPort.SelectedItem = TrdAntennaParams.TrdAntennaSerialPort.ToString();
            cmbExchangeBaud.SelectedItem = TrdAntennaParams.TrdAntennaBaud.ToString();

        }
        private void PreSetForm_Load(object sender, EventArgs e)
        {
            CopyFromGlobal();
        }

        private void cmbLocType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbLocType.SelectedIndex)
            {
                case 0:
                    cmbLocComm.Enabled = false;
                    cmbLocBaud.Enabled = false;
                    txtLocIP.Enabled = false;
                    txtLocPort.Enabled = false;

                    cmbRmtType.SelectedIndex = 0;
                    cmbRmtType.Enabled = false;
                    break;
                case 1:
                    cmbLocComm.Enabled = true;
                    cmbLocBaud.Enabled = true;
                    txtLocIP.Enabled = false;
                    txtLocPort.Enabled = false;
                    cmbRmtType.Enabled = true;
                    break;
                case 2:
                    cmbLocComm.Enabled = false;
                    cmbLocBaud.Enabled = false;
                    txtLocIP.Enabled = true;
                    txtLocPort.Enabled = true;
                    cmbRmtType.Enabled = true;
                    break;
                default:
                    cmbLocComm.Enabled = false;
                    cmbLocBaud.Enabled = false;
                    txtLocIP.Enabled = false;
                    txtLocPort.Enabled = false;

                    cmbRmtType.SelectedIndex = 0;
                    cmbRmtType.Enabled = false;
                    throw new Exception("无此数据接收方式选项.");
            }
        }

        private void cmbRmtType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbRmtType.SelectedIndex)
            {
                case 0:
                    cmbRmtComm.Enabled = false;
                    cmbRmtBaud.Enabled = false;
                    txtRmtIP.Enabled = false;
                    txtRmtPort.Enabled = false;
                    break;
                case 1:
                    cmbRmtComm.Enabled = true;
                    cmbRmtBaud.Enabled = true;
                    txtRmtIP.Enabled = false;
                    txtRmtPort.Enabled = false;
                    break;
                case 2:
                    cmbRmtComm.Enabled = false;
                    cmbRmtBaud.Enabled = false;
                    txtRmtIP.Enabled = true;
                    txtRmtPort.Enabled = true;
                    break;
                default:
                    cmbRmtComm.Enabled = false;
                    cmbRmtBaud.Enabled = false;
                    txtRmtIP.Enabled = false;
                    txtRmtPort.Enabled = false;
                    throw new Exception("无此数据接收方式选项.");
            }
        }

        private void cmbIOCollectType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (0 == cmbIOCollectType.SelectedIndex)
            {
                cmbSerialPort.Enabled = false;
                cmbIOBaud.Enabled = false;
                txtIOIP.Enabled = false;
                txtIOPort.Enabled = false;
            }
            else
            {
                if (1 == cmbIOCollectType.SelectedIndex)
                {
                    cmbSerialPort.Enabled = true;
                    cmbIOBaud.Enabled = true;
                    txtIOIP.Enabled = false;
                    txtIOPort.Enabled = false;
                }
                else
                {
                    cmbSerialPort.Enabled = false;
                    cmbIOBaud.Enabled = false;
                    txtIOIP.Enabled = true;
                    txtIOPort.Enabled = true;
                }
            }
        }

        private void changePreAntennaStat()
        {

        }

        private void changeExchangeAntennaStat()
        {

        }

        #region 验证连接
        private void BtnDBTest_Click(object sender, EventArgs e)
        {
            if (CheckInput())
            {
                try
                {
                    CopyDBToGlobal();
                }
                catch (Exception)
                {
                    MessageBox.Show("保存失败，请检查输入！");
                }
            }

            loginWorker = new BackgroundWorker();
            loginWorker.DoWork += backGWLogin_DoWork;
            loginWorker.RunWorkerCompleted += backGWLogin_RunWorkerCompleted;
            loginWorker.RunWorkerAsync();
        }

        private void CopyDBToGlobal()
        {
            //本地数据库
            LocalDBParams.DBLocation = txtDBIp.Text;
            LocalDBParams.DBPort = Convert.ToInt32(txtDBPort.Text);
            LocalDBParams.DBName = txtDBName.Text;
            LocalDBParams.DBType = (DataBaseType)cmbDBType.SelectedIndex;
            LocalDBParams.DBUser = txtDBUserName.Text;
            LocalDBParams.DBPwd = txtDBPwd.Text;
        }
        private bool CheckInput()
        {
            if (txtDBIp.Text.Trim().Equals(""))
            {
                MessageBox.Show("IP不能为空");
                txtDBIp.Focus();
                return false;
            }
            if (txtDBPort.Text.Trim().Equals(""))
            {
                MessageBox.Show("端口号不能为空");
                txtDBPort.Focus();
                return false;
            }
            if (!RegexMatch.CheckNum(txtDBPort.Text.Trim()))
            {
                MessageBox.Show("端口号只能为数字");
                txtDBPort.Focus();
                return false;
            }
            if (txtDBName.Text.Trim().Equals(""))
            {
                MessageBox.Show("数据库名称不能为空");
                txtDBName.Focus();
                return false;
            }
            if (txtDBUserName.Text.Trim().Equals(""))
            {
                MessageBox.Show("用户名不能为空");
                txtDBUserName.Focus();
                return false;
            }
            if (txtDBPwd.Text.Trim().Equals(""))
            {
                MessageBox.Show("密码不能为空");
                txtDBPwd.Focus();
                return false;
            }
            if (cmbDBType.Text.Trim().Equals(""))
            {
                MessageBox.Show("数据库类型不能为空");
                cmbDBType.Focus();
                return false;
            }

            return true;
        }

        private void backGWLogin_DoWork(object sender, DoWorkEventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false; 

            okBtn.Enabled = false;
            BtnDBTest.Enabled = false;
            lblTestRes.Text = "连接中...";

            Global.SQLDBConn = LoginController.GetSQLServerConnection();
        }
        private void backGWLogin_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (Global.SQLDBConn.State != ConnectionState.Open)
            {
                lblTestRes.ForeColor = Color.Red;
                lblTestRes.Text = "连接失败";
            }
            else
            {
                lblTestRes.ForeColor = Color.Green;
                lblTestRes.Text = "连接成功";
            }
            BtnDBTest.Enabled = true;
            okBtn.Enabled = true;
        }
        #endregion

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void okBtn_Click(object sender, EventArgs e)
        {
            if (CheckInput())
            {
                try
                {
                    CopyToGlobal();
                    ReadWritePara.WriteXMLPara();
                    this.Close();
                    MessageBox.Show("保存成功！");
                    this.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存失败，请检查输入！");
                }
            }
        }

        private void CopyToGlobal()
        {

            //本地数据库
            LocalDBParams.DBLocation = txtDBIp.Text;
            LocalDBParams.DBPort = Convert.ToInt32(txtDBPort.Text);
            LocalDBParams.DBName = txtDBName.Text;
            LocalDBParams.DBType = (DataBaseType)cmbDBType.SelectedIndex;
            LocalDBParams.DBUser = txtDBUserName.Text;
            LocalDBParams.DBPwd = txtDBPwd.Text;

            //数据采集
            DataCollectorParams.localAccessType = cmbLocType.SelectedIndex;
            DataCollectorParams.localSerialPort = cmbLocComm.SelectedItem.ToString();
            DataCollectorParams.localBaud = Convert.ToInt32(cmbLocBaud.SelectedItem);
            DataCollectorParams.localIp = txtLocIP.Text;
            DataCollectorParams.localPort = Convert.ToInt32(txtLocPort.Text);
            DataCollectorParams.remoteAccessType = cmbRmtType.SelectedIndex;
            DataCollectorParams.remoteSerialPort = cmbRmtComm.SelectedItem.ToString();
            DataCollectorParams.remoteBaud = Convert.ToInt32(cmbRmtBaud.SelectedItem);
            DataCollectorParams.remoteIp = txtRmtIP.Text;
            DataCollectorParams.remotePort = Convert.ToInt32(txtRmtPort.Text);

            //本地服务器设置
            if (nonUpRadio.Checked)
            {
                LocalServerParams.LocalServerMode = 0;
            }
            if (tcpRadio.Checked)
            {
                LocalServerParams.LocalServerMode = 2;
                LocalServerParams.IP = txtTCPServerIP.Text;
                LocalServerParams.Port = Convert.ToInt32(txtTCPServerPort.Text);
            }

            //IO
            IOModuleParams.IOModuleAccessType = cmbIOCollectType.SelectedIndex;
            IOModuleParams.IOModuleIp = txtIOIP.Text;
            IOModuleParams.IOModulePort = Convert.ToInt32(txtIOPort.Text);
            IOModuleParams.IOModuleSerialPort = cmbSerialPort.Text;
            IOModuleParams.IOModuleBaud = Convert.ToInt32(cmbIOBaud.Text);

            //运行状态
            InitParams.RunMode = cmbRunState.SelectedIndex;

            //预读天线
            PreAntennaParams.PreAntennaAccessType = cmbPreCollect.SelectedIndex;
            PreAntennaParams.PreAntennaIp = txtPreIP.Text;
            PreAntennaParams.PreAntennaPort = Convert.ToInt32(txtPrePort.Text);
            PreAntennaParams.PreAntennaSerialPort = cmbPreSerl.Text;
            PreAntennaParams.PreAntennaBaud = Convert.ToInt32(cmbPreBaud.Text);

            //交易天线
            TrdAntennaParams.TrdAntennaAccessType = cmbExchangeType.SelectedIndex;
            TrdAntennaParams.TrdAntennaIp = txtExchangeIP.Text;
            TrdAntennaParams.TrdAntennaPort = Convert.ToInt32(txtExchangePort.Text);
            TrdAntennaParams.TrdAntennaSerialPort = cmbExchangeCommPort.Text;
            TrdAntennaParams.TrdAntennaBaud = Convert.ToInt32(cmbExchangeBaud.Text);

            // 设置启动模块数量
            InitParams.SetInitModuleCount();
        }
    }
}
