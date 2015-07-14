using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TruckETCExitus.Device;
using TruckETCExitus.Etc;
using Util;

namespace TruckETCExitus.Model
{
    public abstract class ExitusState : State
    {
        protected int preStep = 0;                                                            // 预读步骤

        protected int trdStep = 0;                                                            // 交易步骤

        protected int preAntanaOBUNo = -1;                                                    // 预读中的OBU号

        protected UInt64 preUserCardNo = 0;                                                   // 预读用户卡编号                                                     

        protected int trdAntanaOBUNo = -1;                                                    // 交易中的OBU号

        protected Location preLoc;

        protected Location trdLoc;  

        protected byte[] B2Frame = new byte[Antenna.B2_PRE_LENGTH];                           // B2帧，存储预读天线B2帧数据

        protected List<byte> D1Frame = new List<byte>();                                      // D1数据帧内容        

        protected System.Timers.Timer tmrPre = new System.Timers.Timer();                     // 预读超时重置      

        protected System.Timers.Timer tmrTrd = new System.Timers.Timer();                     // 交易超时重置

        public void InitControl(Button btnUICtrl)
        {
            tmrPre.Interval = 5000;
            tmrPre.Enabled = false;
            tmrPre.Elapsed += new System.Timers.ElapsedEventHandler(tmrPreElapsed);

            tmrTrd.Interval = 10000;
            tmrTrd.Enabled = false;
            tmrTrd.Elapsed += new System.Timers.ElapsedEventHandler(tmrTrdElapsed);

            if (InitParams.UIEnabled)
            {
                btnUICtrl.Text = "关闭UI";
            }
            else
            {
                btnUICtrl.Text = "启动UI";
            }
        }

        protected void tmrPreElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ResetPreStep();
        }
        protected void tmrTrdElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ResetTrdStep();
        }

        #region 预读天线

        protected void ResetPreStep()
        {
            tmrPre.Enabled = false;
            preAntanaOBUNo = -1;
            preStep = 0;
        }

        protected void UpdatePreAntannaMsg(CSUnit csUnit, string msg, Color color, RichTextBox rtb)
        {
            if (InitParams.UIEnabled)
            {
                string info = string.Format("{0}---预读天线(IP:{1},Port:{2})=>>CRTC---{3}\r\n",
                            System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss:fff"), csUnit.IpOrCommPort,
                            csUnit.PortOrBaud, msg);

                RichTextBoxUtil.UpdateRTxtUI(rtb, info, color);
            }
        }
        public void HandlePreAntennaConn(CSUnit csUnit, RichTextBox rtb)
        {
            UpdatePreAntannaMsg(csUnit, "连接", Color.Blue, rtb);

            ResetPreStep();
        }

        public void HandlePreAntennaClose(CSUnit csUnit, RichTextBox rtb)
        {
            UpdatePreAntannaMsg(csUnit, "断开", Color.Green, rtb);

            ResetPreStep();
        }

        public virtual void HandlePreAntRecvData(CSUnit csUnit, RichTextBox rtb)
        {
            if (csUnit.Buffer[0] == Antenna.bof[0] && csUnit.Buffer[1] == Antenna.bof[1]
                && csUnit.Buffer[csUnit.Buffer.Length - 1] == Antenna.eof[0] && csUnit.Buffer.Length >= 6)
            {
                switch (csUnit.Buffer[Antenna.CMD_LOC])
                {
                    case 0xB0:
                        HandlePreB0Frame(csUnit, rtb);
                        break;
                    case 0xB2:
                        HandlePreB2Frame(csUnit, rtb);
                        break;
                    case 0xD0:
                        HandlePreD0Frame(csUnit, rtb);
                        break;
                    case 0xD1:                        
                        HandleD1Frame(csUnit, rtb);
                        break;
                    case 0xD2:
                        HandleD2Frame(csUnit, rtb);
                        break;
                    default:
                        HandlePreAntDefaultFrame(csUnit, rtb);
                        break;
                }
            }
            else
            {
                UpdatePreAntannaMsg(csUnit, "异常帧", Color.Red, rtb);
            }
        }
        

        /// <summary>
        /// 收到B0直接转发给PC
        /// </summary>
        /// <param name="csUnit">远程单元</param>
        protected void HandlePreB0Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                Global.localServer.Send(csUnit.Buffer);

            UpdatePreAntannaMsg(csUnit, "预读B0到来", Color.Black, rtb);
            ResetPreStep();
        }

        /// <summary>
        /// 预读B2，只可能为心跳
        /// </summary>
        /// <param name="csUnit">远程单元</param>
        protected void HandlePreB2Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                Global.localServer.Send(csUnit.Buffer);
        }

        /// <summary>
        /// D0预读定位帧
        /// </summary>
        /// <param name="csUnit">远程单元</param>
        protected void HandlePreD0Frame(CSUnit csUnit, RichTextBox rtb)
        {
            List<byte> D0Frame = ByteFilter.deFilter(csUnit.Buffer);

            if (D0Frame.Count == Antenna.D0_LENGTH)
            {
                int d0OBUNo = D0Frame[4] * 16777216 + D0Frame[5] * 65536
                    + D0Frame[6] * 256 + D0Frame[7];
                int SendFrameType = D0Frame[8];
                int POstate = D0Frame[9];
                int XOBUCoord = GetCoordValue(D0Frame[10], D0Frame[11], D0Frame[12], D0Frame[13]);
                int YOBUCoord = GetCoordValue(D0Frame[14], D0Frame[15], D0Frame[16], D0Frame[17]);

                preLoc = new Location(new OBUData(d0OBUNo), SendFrameType, POstate, XOBUCoord, YOBUCoord);

                //string msg = string.Format("d0OBUNo={0},SendFrameType={1},POstate={2},XOBUCoord={3},YOBUCoord={4}"
                //    , preLoc.ObuData.ObuNum, preLoc.SendFrameType, preLoc.POstate, preLoc.XOBUCoord, preLoc.YOBUCoord);
                //UpdatePreAntannaMsg(csUnit, "收到D0,msg=" + msg, Color.CadetBlue, rtb);
            }
            else
            {
                UpdatePreAntannaMsg(csUnit, "D0长度错误:" + D0Frame.Count, Color.Red, rtb);
                ResetPreStep();
            }

        }

        private int GetCoordValue(byte p1, byte p2, byte p3, byte p4)
        {
            if(p1 == 0xff)
            {
                return (-1) * (16777215 - (p2 * 65536 + p3 * 256 + p4) + 1);
            }
            else
            {                
                return p1 * 16777216 + p2 * 65536 + p3 * 256 + p4;
            }
        }

        /// <summary>
        /// 只有预读步骤为0时执行(即第一次收到D1,tmrPre后重置).
        /// 收到后记录OBU号,回复C1.
        /// 根据D1、D2内容生成预读B2.
        /// </summary>
        /// <param name="csUnit">远程单元</param>
        protected virtual void HandleD1Frame(CSUnit csUnit, RichTextBox rtb)
        {
            tmrPre.Enabled = false;
            tmrPre.Enabled = true;

            string info = "";

            D1Frame.Clear();
            D1Frame = ByteFilter.deFilter(csUnit.Buffer);

            if (D1Frame.Count == Antenna.D1_LENGTH)
            {
                preAntanaOBUNo = csUnit.Buffer[4] * 16777216 + csUnit.Buffer[5] * 65536
                + csUnit.Buffer[6] * 256 + csUnit.Buffer[7];

                byte[] C1Frame = Antenna.createC1Frame(csUnit.Buffer[4], csUnit.Buffer[5], csUnit.Buffer[6], csUnit.Buffer[7]);

                if (Global.PreAntenna.IsConnect())
                    Global.PreAntenna.Send(C1Frame);

                preStep = 1;
                UpdatePreAntannaMsg(csUnit, "D1到来", Color.BlueViolet, rtb);
            }
            else
            {
                UpdatePreAntannaMsg(csUnit, "D1长度错误", Color.CadetBlue, rtb);
                ResetPreStep();
            }
        }

        /// <summary>
        /// 只有预读步骤为1时执行(即收到D1后的D2才接收,tmrPre后重置).
        /// 收到后对比D1 obu号,正确则D1、D2组合B2 发送PC机.
        /// </summary>
        /// <param name="csUnit">远程单元</param>
        protected void HandleD2Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (preStep == 1)
            {
                List<byte> D2Frame = ByteFilter.deFilter(csUnit.Buffer);

                if (D2Frame.Count == 128)
                {
                    preUserCardNo = D2Frame[21] * ((UInt64)Math.Pow(2, 56)) + D2Frame[22] * ((UInt64)Math.Pow(2, 48))
                        + D2Frame[23] * ((UInt64)Math.Pow(2, 40)) + D2Frame[24] * ((UInt64)Math.Pow(2, 32))
                        + D2Frame[25] * ((UInt64)Math.Pow(2, 24)) + D2Frame[26] * ((UInt64)Math.Pow(2, 16))
                        + D2Frame[27] * ((UInt64)Math.Pow(2, 8)) + D2Frame[28];

                    int d1OBUNo = csUnit.Buffer[4] * 16777216 + csUnit.Buffer[5] * 65536
                    + csUnit.Buffer[6] * 256 + csUnit.Buffer[7];
                    if (d1OBUNo == preAntanaOBUNo)
                    {
                        B2Frame = Antenna.CreateB2Frame(D1Frame, D2Frame);

                        if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                            Global.localServer.Send(ByteFilter.enFilter(B2Frame));

                        preStep = 2;
                        UpdatePreAntannaMsg(csUnit, "D2到来", Color.Black, rtb);
                    }
                    else
                    {
                        UpdatePreAntannaMsg(csUnit, "D2 obu号和D1不同", Color.Red, rtb);
                        ResetPreStep();
                    }
                }
                else
                {
                    UpdatePreAntannaMsg(csUnit, "D2长度错误", Color.Red, rtb);
                    ResetPreStep();
                }
            }
            else
            {
                UpdatePreAntannaMsg(csUnit, "D2到来顺序错误", Color.Red, rtb);
                ResetPreStep();
            }
        }

        protected void HandlePreAntDefaultFrame(CSUnit csUnit, RichTextBox rtb)
        {
            UpdatePreAntannaMsg(csUnit, "无此帧信息:" + csUnit.Buffer[Antenna.CMD_LOC].ToString("X2"), Color.Red, rtb);
            ResetPreStep();
        }

        #endregion

        #region 交易天线

        protected void ResetTrdStep()
        {
            trdAntanaOBUNo = -1;
            trdStep = 0;
            tmrTrd.Enabled = false;
        }

        protected void UpdateTrdAntannaMsg(CSUnit csUnit, string msg, Color color, RichTextBox rtb)
        {
            if (InitParams.UIEnabled)
            {
                string info = string.Format("{0}---交易天线(IP:{1},Port:{2})=>>CRTC---{3}\r\n",
                            System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss:fff"), csUnit.IpOrCommPort,
                            csUnit.PortOrBaud, msg);

                RichTextBoxUtil.UpdateRTxtUI(rtb, info, color);
            }
        }

        public void HandleTrdAntennaConn(CSUnit csUnit, RichTextBox rtb)
        {
            UpdateTrdAntannaMsg(csUnit, "连接", Color.Blue, rtb);

            ResetTrdStep();
        }

        public void HandleTrdAntennaClose(CSUnit csUnit, RichTextBox rtb)
        {
            UpdateTrdAntannaMsg(csUnit, "断开", Color.Green, rtb);

            ResetTrdStep();
        }

        public virtual void HandleTrdAntRecvData(CSUnit csUnit, RichTextBox rtb)
        {
            if (csUnit.Buffer[0] == Antenna.bof[0] && csUnit.Buffer[1] == Antenna.bof[1]
                && csUnit.Buffer[csUnit.Buffer.Length - 1] == Antenna.eof[0] && csUnit.Buffer.Length >= 6)
            {
                switch (csUnit.Buffer[Antenna.CMD_LOC])
                {
                    case 0xB0:
                        HandleTrdB0Frame(csUnit, rtb);
                        break;
                    case 0xB2:
                        HandleB2Frame(csUnit, rtb);
                        break;
                    case 0xB3:
                        HandleB3Frame(csUnit, rtb);
                        break;
                    case 0xB5:
                        HandleB5Frame(csUnit, rtb);
                        break;
                    case 0xD0:
                        HandletrdD0Frame(csUnit, rtb);
                        break;
                    case 0xE4:
                        HandleE4Frame(csUnit, rtb);
                        break;
                    default:
                        HandleTrdAntDefaultFrame(csUnit, rtb);
                        break;
                }
            }
            else
            {
                UpdateTrdAntannaMsg(csUnit, "异常帧", Color.Red, rtb);
            }
        }
        
        
        /// <summary>
        /// 收到B0直接转发给PC
        /// </summary>
        /// <param name="csUnit">远程单元</param>
        protected void HandleTrdB0Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                Global.localServer.Send(csUnit.Buffer);

            UpdateTrdAntannaMsg(csUnit, "收到交易B0帧", Color.GreenYellow, rtb);
            ResetTrdStep();
        }

        protected virtual void HandleB2Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (csUnit.Buffer[Antenna.HEARTBEAT_LOC] == Antenna.HEARTBEAT_CONTENT)
            {
                //心跳包
                if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                    Global.localServer.Send(csUnit.Buffer);
            }
            else
            {
                tmrTrd.Enabled = false;
                tmrTrd.Enabled = true;

                List<byte> B2frame = ByteFilter.deFilter(csUnit.Buffer);
                if (B2frame.Count == Antenna.B2_TRD_LENGTH)
                {
                    trdAntanaOBUNo = csUnit.Buffer[5] * 16777216 + csUnit.Buffer[6] * 65536
                    + csUnit.Buffer[7] * 256 + csUnit.Buffer[8];
                    trdStep = 1;

                    if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                        Global.localServer.Send(csUnit.Buffer);
                    UpdateTrdAntannaMsg(csUnit, "B2帧到来", Color.CornflowerBlue, rtb);
                }
                else
                {
                    UpdateTrdAntannaMsg(csUnit, "B2帧长度异常", Color.Red, rtb);
                    ResetTrdStep();
                }
            }
        }

        protected void HandleB3Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (trdStep == 1)
            {
                if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                    Global.localServer.Send(csUnit.Buffer);
                trdStep = 2;
                UpdateTrdAntannaMsg(csUnit, "B3到来", Color.Blue, rtb);
            }
            else
            {
                UpdateTrdAntannaMsg(csUnit, "B3帧到来顺序异常:" + trdStep, Color.Red, rtb);
                ResetTrdStep();
            }
        }

        protected void HandleB5Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (trdStep == 3)
            {
                if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                    Global.localServer.Send(csUnit.Buffer);

                trdStep = 4;
                UpdateTrdAntannaMsg(csUnit, "B5到来", Color.BlueViolet, rtb);
            }
            else
            {
                UpdateTrdAntannaMsg(csUnit, "B5帧到来顺序异常:" + trdStep, Color.Red, rtb);
                ResetTrdStep();
            }
        }
        /// <summary>
        /// D0预读定位帧
        /// </summary>
        /// <param name="csUnit">远程单元</param>
        protected void HandletrdD0Frame(CSUnit csUnit, RichTextBox rtb)
        {
            //List<byte> D0Frame = ByteFilter.deFilter(csUnit.Buffer);

            //if (D0Frame.Count == Antenna.D0_LENGTH)
            //{
            //    int d0OBUNo = D0Frame[4] * 16777216 + D0Frame[5] * 65536
            //        + D0Frame[6] * 256 + D0Frame[7];
            //    int SendFrameType = D0Frame[8];
            //    int POstate = D0Frame[9];
            //    int XOBUCoord = GetCoordValue(D0Frame[10], D0Frame[11], D0Frame[12], D0Frame[13]);
            //    int YOBUCoord = GetCoordValue(D0Frame[14], D0Frame[15], D0Frame[16], D0Frame[17]);

            //    trdLoc = new Location(new OBUData(d0OBUNo), SendFrameType, POstate, XOBUCoord, YOBUCoord);

            //    string msg = string.Format("d0OBUNo={0},SendFrameType={1},POstate={2},XOBUCoord={3},YOBUCoord={4}"
            //        , trdLoc.ObuData.ObuNum, trdLoc.SendFrameType, trdLoc.POstate, trdLoc.XOBUCoord, trdLoc.YOBUCoord);
            //    UpdateTrdAntannaMsg(csUnit, "收到D0,msg=" + msg, Color.CadetBlue, rtb);
            //}
            //else
            //{
            //    UpdateTrdAntannaMsg(csUnit, "D0长度错误:" + D0Frame.Count, Color.Red, rtb);
            //    ResetPreStep();
            //}

        }
        protected void HandleTrdAntDefaultFrame(CSUnit csUnit, RichTextBox rtb)
        {
            UpdateTrdAntannaMsg(csUnit, "无此帧信息:" + csUnit.Buffer[Antenna.CMD_LOC].ToString("X2"), Color.Red, rtb);
            ResetTrdStep();
        }

        protected void HandleE4Frame(CSUnit csUnit, RichTextBox rtb)
        {
            List<byte> E4Frame = ByteFilter.deFilter(csUnit.Buffer);

            if(E4Frame.Count == Antenna.E4_LENGTH)
            {
                int e4OBUNo = csUnit.Buffer[4] * 16777216 + csUnit.Buffer[5] * 65536
                    + csUnit.Buffer[6] * 256 + csUnit.Buffer[7];

                VehData vehData = null;
                OBUData obuData = null;
                if (Global.exchangeQueue.vehQueue.Count > 0)
                    vehData = Global.exchangeQueue.vehQueue.Peek();
                else
                    vehData = new VehData();
                if (Global.exchangeQueue.obuQueue.Count > 0)
                    obuData = Global.exchangeQueue.obuQueue.Peek();
                else
                    obuData = new OBUData(0);

                if (obuData.ObuNum == e4OBUNo)
                {
                    byte[] B4Frame = Antenna.CreateB4Frame(e4OBUNo, vehData.Axle_Type, vehData.Whole_Weight, B2Frame, E4Frame.ToArray());
                    if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                        Global.localServer.Send(ByteFilter.enFilter(B4Frame));                    

                    trdStep = 3;
                    UpdateTrdAntannaMsg(csUnit, "收到交易E4发送B4", Color.MediumPurple, rtb);
                    return;
                }
                else
                {
                    ResetTrdStep();
                    UpdateTrdAntannaMsg(csUnit, "收到交易E4后C1(C1 OBU号和交易队列OBU号不同)", Color.DarkRed, rtb);
                    return;
                }

            }
            else
            {
                ResetTrdStep();
                UpdateLocSrvMsg(csUnit, "收到交易E4长度错误", Color.DarkRed, rtb);
                return;
            }            
        }

        #endregion

        #region 本地服务器

        public void HandleLocSrvConn(CSUnit csUnit, RichTextBox rtb)
        {
            UpdateLocSrvMsg(csUnit, "连接", Color.Blue, rtb);
        }

        public void HandleLocSrvClose(CSUnit csUnit, RichTextBox rtb)
        {
            UpdateLocSrvMsg(csUnit, "断开", Color.Green, rtb);
        }

        public virtual void HandleLocSrvRecvData(CSUnit csUnit, RichTextBox rtb)
        {
            if (csUnit.Buffer[0] == LocalServer.bof[0] && csUnit.Buffer[1] == LocalServer.bof[1] && csUnit.Buffer[csUnit.Buffer.Length - 1] == LocalServer.eof[0] &&
                csUnit.Buffer.Length >= 6)
            {
                switch (csUnit.Buffer[LocalServer.CMD_LOC])
                {
                    case 0x4C:
                        Handle4CFrame(csUnit, rtb);
                        break;
                    case 0xC0:
                        HandleC0Frame(csUnit, rtb);
                        break;
                    case 0xC1:
                        HandleC1Frame(csUnit, rtb);
                        break;
                    case 0xC3:
                        HandleC3Frame(csUnit, rtb);
                        break;
                    case 0xC6:
                        HandleC6Frame(csUnit, rtb);
                        break;
                    case 0xCD:
                        HandleCDFrame(csUnit, rtb);
                        break;
                    case 0xCE:
                        HandleCEFrame(csUnit, rtb);
                        break;
                    default:
                        UpdateLocSrvMsg(csUnit, "无此帧", Color.OrangeRed, rtb);
                        break;
                }
            }
            else
            {
                UpdateLocSrvMsg(csUnit, "异常帧", Color.Purple, rtb);
            }
        }
       

        protected void UpdateLocSrvMsg(CSUnit csUnit, string msg, Color color, RichTextBox rtb)
        {
            if (InitParams.UIEnabled)
            {
                string info = string.Format("{0}---PC(IP:{1},Port:{2})=>>CRTC---{3}\r\n",
                            System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss:fff"), csUnit.IpOrCommPort,
                            csUnit.PortOrBaud, msg);

                RichTextBoxUtil.UpdateRTxtUI(rtb, info, color);
            }
        }
        protected void Handle4CFrame(CSUnit csUnit, RichTextBox rtb)
        {
            if (Global.PreAntenna != null && Global.PreAntenna.IsConnect())
                Global.PreAntenna.Send(csUnit.Buffer);
            if (Global.TrdAntenna != null && Global.TrdAntenna.IsConnect())
                Global.TrdAntenna.Send(csUnit.Buffer);

            UpdateLocSrvMsg(csUnit, "关闭天线", Color.OrangeRed, rtb);
        }

        protected void HandleC0Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (Global.PreAntenna != null && Global.PreAntenna.IsConnect())
                Global.PreAntenna.Send(csUnit.Buffer);
            if (Global.TrdAntenna != null && Global.TrdAntenna.IsConnect())
                Global.TrdAntenna.Send(csUnit.Buffer);

            UpdateLocSrvMsg(csUnit, "初始化帧", Color.Yellow, rtb);

        }
        protected virtual void HandleC1Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (csUnit.Buffer.Length == LocalServer.C1_LENGTH && csUnit.Buffer[0] == LocalServer.bof[0]
                && csUnit.Buffer[1] == LocalServer.bof[1] && csUnit.Buffer[csUnit.Buffer.Length - 1] == LocalServer.eof[0]
                && csUnit.Buffer[LocalServer.CMD_LOC] == 0xC1)
            {
                if (preStep == 0 && trdStep == 0)
                {
                    if (Global.PreAntenna != null && Global.PreAntenna.IsConnect())
                        Global.PreAntenna.Send(csUnit.Buffer);
                    if (Global.TrdAntenna != null && Global.TrdAntenna.IsConnect())
                        Global.TrdAntenna.Send(csUnit.Buffer);

                    ResetPreStep();
                    ResetTrdStep();
                    UpdateLocSrvMsg(csUnit, "初始化确认帧", Color.Green, rtb);
                    return;
                }
                int c1OBUNo = csUnit.Buffer[4] * 16777216 + csUnit.Buffer[5] * 65536
                    + csUnit.Buffer[6] * 256 + csUnit.Buffer[7];

                if (c1OBUNo == preAntanaOBUNo)
                {
                    if (preStep == 2)
                    {
                        if (Global.PreAntenna.IsConnect())
                            Global.PreAntenna.Send(csUnit.Buffer);

                        PreProcessSucess(c1OBUNo);

                        ResetPreStep();
                        UpdateLocSrvMsg(csUnit, "收到预读B2后C1", Color.LightGreen, rtb);
                        return;
                    }
                }
                else
                {
                    if (c1OBUNo == trdAntanaOBUNo)
                    {
                        if (trdStep == 1)
                        {
                            if (Global.TrdAntenna.IsConnect())
                                Global.TrdAntenna.Send(csUnit.Buffer);

                            UpdateLocSrvMsg(csUnit, "收到交易B2后C1", Color.LightSeaGreen, rtb);
                            return;
                        }
                        if (trdStep == 2)
                        {
                            if (Global.TrdAntenna.IsConnect())
                                Global.TrdAntenna.Send(csUnit.Buffer);

                            UpdateLocSrvMsg(csUnit, "收到交易B3后C1", Color.LightSeaGreen, rtb);
                            return;

                        }
                        if (trdStep == 4)
                        {
                            if (Global.TrdAntenna.IsConnect())
                                Global.TrdAntenna.Send(csUnit.Buffer);

                            Global.exchangeQueue.vehQueue.Dequeue();
                            Global.exchangeQueue.obuQueue.Dequeue();

                            ResetTrdStep();
                            UpdateLocSrvMsg(csUnit, "收到交易B5后C1,交易成功", Color.Purple, rtb);
                            return;
                        }
                    }
                    else
                    {
                        ResetPreStep();
                        ResetTrdStep();

                        UpdateLocSrvMsg(csUnit, "收到预读或交易后C1号码不存在", Color.DarkRed, rtb);
                    }
                }

            }
        }

        protected virtual void PreProcessSucess(int c1OBUNo)
        {
            Global.exchangeQueue.obuQueue.Enqueue(new OBUData(c1OBUNo));
        }

        protected void HandleC3Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (trdStep == 3)
            {
                if (Global.TrdAntenna.IsConnect())
                {
                    Global.TrdAntenna.Send(csUnit.Buffer);
                }

                UpdateLocSrvMsg(csUnit, "收到C3", Color.DarkGreen, rtb);
            }
            else
            {
                UpdateLocSrvMsg(csUnit, "收到C3到来顺序异常", Color.DarkRed, rtb);
            }
        }

        protected void HandleC6Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (trdStep == 3)
            {
                if (Global.TrdAntenna != null && Global.TrdAntenna.IsConnect())
                {
                    Global.TrdAntenna.Send(csUnit.Buffer);
                    UpdateLocSrvMsg(csUnit, "收到C6", Color.DarkGreen, rtb);
                }
                else
                {
                    UpdateLocSrvMsg(csUnit, "收到C6时连接断开", Color.DarkRed, rtb);
                }
                
            }
            else
            {
                UpdateLocSrvMsg(csUnit, "收到C6到来顺序异常", Color.DarkRed, rtb);
            }

        }
        protected virtual void HandleCDFrame(CSUnit csUnit, RichTextBox rtb)
        {
            if ((csUnit.Buffer[8] & 1) > 0)
            {
                ;
            }            
        }

        protected void HandleCEFrame(CSUnit csUnit, RichTextBox rtb)
        {
            List<byte> CEFrame = ByteFilter.deFilter(csUnit.Buffer);
            if(CEFrame.Count == Antenna.CE_LENGTH)
            {
                UInt64 CEUserCardNo = CEFrame[4] * ((UInt64)Math.Pow(2, 56)) + CEFrame[5] * ((UInt64)Math.Pow(2, 48))
                        + CEFrame[6] * ((UInt64)Math.Pow(2, 40)) + CEFrame[7] * ((UInt64)Math.Pow(2, 32))
                        + CEFrame[8] * ((UInt64)Math.Pow(2, 24)) + CEFrame[9] * ((UInt64)Math.Pow(2, 16))
                        + CEFrame[10] * ((UInt64)Math.Pow(2, 8)) + CEFrame[11];
                if ((CEFrame[12] & 1) == 0)
                {
                    int weightData = 0;
                    int axleData = 0;
                    if (Global.exchangeQueue.ContainsUserCardNo(CEUserCardNo))
                    {
                        while (Global.exchangeQueue.obuQueue.Count > 0)
                        {
                            OBUData obuData = Global.exchangeQueue.obuQueue.Peek();
                            if (obuData.UserCardNo == CEUserCardNo)
                            {
                                weightData = Global.exchangeQueue.vehQueue.Peek().Whole_Weight;
                                axleData = Global.exchangeQueue.vehQueue.Peek().Axle_Type;

                                break;
                            }
                            else
                            {
                                Global.exchangeQueue.obuQueue.Dequeue();
                                Global.exchangeQueue.vehQueue.Dequeue();
                            }
                        }
                    }
                    byte[] BEFrame = Antenna.createBEFrame(CEUserCardNo, axleData, weightData);
                    //byte[] BEFrame = Antenna.createBEFrame(11, 11, 12000);
                    Global.localServer.Send(ByteFilter.enFilter(BEFrame));
                }
                else
                {
                    if((CEFrame[12] & 1) == 1)
                    {
                        if (Global.exchangeQueue.obuQueue.Count > 0 && Global.exchangeQueue.vehQueue.Count > 0)
                        {
                            Global.exchangeQueue.obuQueue.Dequeue();
                            Global.exchangeQueue.vehQueue.Dequeue();
                        }
                    }
                    else
                    {
                        UpdateLocSrvMsg(csUnit, "CE STATE异常:" + CEFrame[12], Color.DarkRed, rtb);
                    }
                }
            }
            else
            {
                UpdateLocSrvMsg(csUnit, "CE长度异常:" + CEFrame.Count, Color.DarkRed, rtb);
            }
        }

        #endregion

        #region 线圈

        public virtual void HandleRefreshCoilStatus()
        {

        }

        #endregion

        #region 仪表
        protected void UpdateMeterMsgUI(string msg, Color color, RichTextBox rtb)
        {
            if (InitParams.UIEnabled)
            {
                string info = string.Format("{0}---仪表=>>CRTC---{1}\r\n",
                            System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss:fff"), msg);

                RichTextBoxUtil.UpdateRTxtUI(rtb, info, color);
            }
        }

        public void HandleDataFrameComeMsg(byte[] buffer, RichTextBox rtb)
        {
            if (DataCollectorParams.remoteAccessType != 0)
            {
                DataCollector.WtSys_GetDataFrame(buffer, buffer.Length);
                Global.dataCollector.WriteRemote(buffer, 0, buffer.Length);
            }

            UpdateMeterMsgUI("数据帧到来", Color.Black, rtb);
        }

        public virtual void HandleVehComeMsg(int tmp, VehData vehData, RichTextBox rtb)
        {
            if (tmp == 1)
            {
                WeightSucess(vehData);

                UpdateMeterMsgUI("称重数据到来", Color.Blue, rtb);
            }
            else
            {
                UpdateMeterMsgUI("称重数据到来错误!", Color.Red, rtb);
            }
        }
        protected void WeightSucess(VehData g_sVehData)
        {
            Global.exchangeQueue.vehQueue.Enqueue(g_sVehData);
        }

        public virtual void HandleVehRasterComeMsg(RichTextBox rtb)
        {
            UpdateMeterMsgUI("光栅信号到来", Color.Orange, rtb);
        }

        public virtual void HandleRasterComeMsg(RichTextBox rtb)
        {
            UpdateMeterMsgUI("车辆到位帧到来", Color.OrangeRed, rtb);
        }

        #endregion

        #region 运行参数

        public virtual void HandleShowRunParams(RichTextBox rtb)
        {

        }


        public virtual void HandleReSetParamsCmd()
        {

        }

        #endregion
    }
}
