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
    public abstract class EntranceState : State
    {
        protected int preStep = 0;                                                            // 预读步骤

        protected int preAntanaOBUNo = -1;                                                    // 预读中的OBU号

        protected System.Timers.Timer tmrPre = new System.Timers.Timer();                     // 预读超时重置   

        public void InitControl(Button btnUICtrl)
        {
            tmrPre.Interval = 2000;
            tmrPre.Enabled = false;
            tmrPre.Elapsed += new System.Timers.ElapsedEventHandler(tmrPreElapsed);

            if (InitParams.UIEnabled)
            {
                btnUICtrl.Text = "关闭UI";
            }
            else
            {
                btnUICtrl.Text = "启动UI";
            }
        }

        private void tmrPreElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ResetPreStep();
        }
        protected void ResetPreStep()
        {
            tmrPre.Enabled = false;
            preAntanaOBUNo = -1;
            preStep = 0;
        }

        #region 预读天线

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

        public void HandlePreAntRecvData(CSUnit csUnit, RichTextBox rtb)
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
                    case 0xB3:
                        HandlePreB3Frame(csUnit, rtb);
                        break;
                    case 0xD4:
                        HandleD4Frame(csUnit, rtb);
                        break;
                    case 0xB5:
                        HandleB5Frame(csUnit, rtb);
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
        /// 预读B2
        /// </summary>
        /// <param name="csUnit">远程单元</param>
        protected void HandlePreB2Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (csUnit.Buffer[Antenna.HEARTBEAT_LOC] == Antenna.HEARTBEAT_CONTENT)
            {
                // 心跳
                if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                    Global.localServer.Send(csUnit.Buffer);
            }
            else
            {
                if (preStep == 0)
                {
                    tmrPre.Enabled = true;

                    preAntanaOBUNo = csUnit.Buffer[5] * 16777216 + csUnit.Buffer[6] * 65536
                        + csUnit.Buffer[7] * 256 + csUnit.Buffer[8];
                    preStep = 1;

                    if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                        Global.localServer.Send(csUnit.Buffer);
                    UpdatePreAntannaMsg(csUnit, "B2帧到来", Color.CornflowerBlue, rtb);
                }
                else
                {
                    UpdatePreAntannaMsg(csUnit, "B2帧到来顺序异常:" + preStep, Color.Red, rtb);
                    ResetPreStep();
                }
            }
        }


        /// <summary>
        /// 预读B3
        /// </summary>
        /// <param name="csUnit">远程单元</param>
        protected void HandlePreB3Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (preStep == 1)
            {
                int b3OBUNo = csUnit.Buffer[4] * 16777216 + csUnit.Buffer[5] * 65536
                    + csUnit.Buffer[6] * 256 + csUnit.Buffer[7];
                if (b3OBUNo == preAntanaOBUNo)
                {
                    preStep = 2;
                    if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                        Global.localServer.Send(csUnit.Buffer);
                    UpdatePreAntannaMsg(csUnit, "B3帧到来", Color.CornflowerBlue, rtb);
                }
                else
                {
                    UpdatePreAntannaMsg(csUnit, "B3帧到来,obu号!=preAntanaOBUNo", Color.Red, rtb);
                    ResetPreStep();
                }
            }
            else
            {
                UpdatePreAntannaMsg(csUnit, "B3帧到来顺序异常:" + preStep, Color.Red, rtb);
                ResetPreStep();
            }
        }

        /// <summary>
        /// 预读D4
        /// </summary>
        /// <param name="csUnit">远程单元</param>
        /// <param name="rtb"></param>
        protected void HandleD4Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (preStep == 2)
            {
                int d4OBUNo = csUnit.Buffer[4] * 16777216 + csUnit.Buffer[5] * 65536
                    + csUnit.Buffer[6] * 256 + csUnit.Buffer[7];
                if (d4OBUNo == preAntanaOBUNo)
                {
                    preStep = 3;

                    List<byte> d4frame = ByteFilter.deFilter(csUnit.Buffer);
                    byte[] b4Frame = createB4Frame(d4frame);

                    if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                        Global.localServer.Send(ByteFilter.enFilter(b4Frame));

                    UpdatePreAntannaMsg(csUnit, "D4帧到来", Color.CornflowerBlue, rtb);
                }
                else
                {
                    UpdatePreAntannaMsg(csUnit, "D4帧到来,obu号!=preAntanaOBUNo", Color.Red, rtb);
                    ResetPreStep();
                }
            }
            else
            {
                UpdatePreAntannaMsg(csUnit, "D4帧到来顺序异常:" + preStep, Color.Red, rtb);
                ResetPreStep();
            }

        }

        protected byte[] createB4Frame(List<byte> d4frame)
        {
            byte[] b4Frame = new byte[133];
            b4Frame[2] = 0x08;
            b4Frame[3] = 0xB4;

            Array.ConstrainedCopy(d4frame.ToArray(), 4, b4Frame, 4, 4);
            b4Frame[8] = 0x00;
            b4Frame[9] = d4frame[9];
            Array.ConstrainedCopy(d4frame.ToArray(), 10, b4Frame, 10, 4);
            Array.ConstrainedCopy(d4frame.ToArray(), 14, b4Frame, 14, 46);
            Array.ConstrainedCopy(d4frame.ToArray(), 60, b4Frame, 60, 43);
            Array.ConstrainedCopy(d4frame.ToArray(), 103, b4Frame, 103, 24);

            VehData vehData = null;
            OBUData obuData = null;
            if (Global.exchangeQueue.vehQueue.Count > 0)
                vehData = Global.exchangeQueue.vehQueue.Dequeue();
            else
                vehData = new VehData();
            if (Global.exchangeQueue.obuQueue.Count > 0)
                obuData = Global.exchangeQueue.obuQueue.Dequeue();
            else
                obuData = new OBUData(0);


            b4Frame[127] = (byte)(vehData.Axle_Type / 255);
            b4Frame[128] = (byte)(vehData.Axle_Type % 255);
            b4Frame[129] = ((byte)((vehData.Whole_Weight / 10) / 255));
            b4Frame[130] = ((byte)((vehData.Whole_Weight / 10) % 255));
            b4Frame[131] = SystemUnit.Get_CheckXor(b4Frame, b4Frame.Length);
            b4Frame[132] = (0xFF);
            return b4Frame;
        }

        /// <summary>
        /// 预读B5
        /// </summary>
        /// <param name="csUnit">远程单元</param>
        /// <param name="rtb"></param>
        protected void HandleB5Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (preStep == 3)
            {
                if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                    Global.localServer.Send(csUnit.Buffer);

                preStep = 4;
                UpdatePreAntannaMsg(csUnit, "B5到来", Color.BlueViolet, rtb);
            }
            else
            {
                UpdatePreAntannaMsg(csUnit, "B5帧到来顺序异常:" + preStep, Color.Red, rtb);
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

        public void HandleTrdAntennaConn(CSUnit csUnit, RichTextBox rtb)
        {
            throw new NotImplementedException();
        }

        public void HandleTrdAntennaClose(CSUnit csUnit, RichTextBox rtb)
        {
            throw new NotImplementedException();
        }

        public void HandleTrdAntRecvData(CSUnit csUnit, RichTextBox rtb)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 本地服务器

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

        public void HandleLocSrvConn(CSUnit csUnit, RichTextBox rtb)
        {
            UpdateLocSrvMsg(csUnit, "连接", Color.Blue, rtb);
        }

        public void HandleLocSrvClose(CSUnit csUnit, RichTextBox rtb)
        {
            UpdateLocSrvMsg(csUnit, "断开", Color.Green, rtb);
        }

        public void HandleLocSrvRecvData(CSUnit csUnit, RichTextBox rtb)
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
                    case 0xCD:
                        HandleCDFrame(csUnit, rtb);
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

        protected void Handle4CFrame(CSUnit csUnit, RichTextBox rtb)
        {
            if (Global.PreAntenna != null && Global.PreAntenna.IsConnect())
                Global.PreAntenna.Send(csUnit.Buffer);

            UpdateLocSrvMsg(csUnit, "关闭天线", Color.OrangeRed, rtb);
        }

        protected void HandleC0Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (Global.PreAntenna != null && Global.PreAntenna.IsConnect())
                Global.PreAntenna.Send(csUnit.Buffer);

            UpdateLocSrvMsg(csUnit, "初始化帧", Color.Yellow, rtb);
        }
        protected void HandleC1Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (csUnit.Buffer.Length == LocalServer.C1_LENGTH && csUnit.Buffer[0] == LocalServer.bof[0]
                && csUnit.Buffer[1] == LocalServer.bof[1] && csUnit.Buffer[csUnit.Buffer.Length - 1] == LocalServer.eof[0]
                && csUnit.Buffer[LocalServer.CMD_LOC] == 0xC1)
            {
                if (preStep == 0 )
                {
                    if (Global.PreAntenna != null && Global.PreAntenna.IsConnect())
                        Global.PreAntenna.Send(csUnit.Buffer);

                    ResetPreStep();
                    UpdateLocSrvMsg(csUnit, "初始化确认帧", Color.Green, rtb);
                    return;
                }

                int c1OBUNo = csUnit.Buffer[4] * 16777216 + csUnit.Buffer[5] * 65536
                    + csUnit.Buffer[6] * 256 + csUnit.Buffer[7];

                if (preStep == 1)
                {
                    if (c1OBUNo == preAntanaOBUNo)
                    {
                        if (Global.PreAntenna != null && Global.PreAntenna.IsConnect())
                            Global.PreAntenna.Send(csUnit.Buffer);

                        UpdateLocSrvMsg(csUnit, "收到交易B2后C1", Color.LightSeaGreen, rtb);
                        return;
                    }
                    else
                    {
                        ResetPreStep();
                        UpdateLocSrvMsg(csUnit, "收到交易B2后C1(OBU号码和B2不同)", Color.DarkRed, rtb);
                        return;
                    }
                }

                if (preStep == 2)
                {
                    if (c1OBUNo == preAntanaOBUNo)
                    {
                        if (Global.PreAntenna != null && Global.PreAntenna.IsConnect())
                            Global.PreAntenna.Send(csUnit.Buffer);

                        UpdateLocSrvMsg(csUnit, "收到交易B3后C1", Color.LightSeaGreen, rtb);
                        return;
                    }
                    else
                    {
                        ResetPreStep();
                        UpdateLocSrvMsg(csUnit, "收到交易B3后C1(OBU号码和B3不同)", Color.DarkRed, rtb);
                        return;
                    }
                }

                if (preStep == 4)
                {
                    if (c1OBUNo == preAntanaOBUNo)
                    {
                        if (Global.PreAntenna != null && Global.PreAntenna.IsConnect())
                            Global.PreAntenna.Send(csUnit.Buffer);

                        ResetPreStep();
                        UpdateLocSrvMsg(csUnit, "收到交易B5后C1", Color.LightSeaGreen, rtb);
                        return;
                    }
                    else
                    {
                        ResetPreStep();
                        UpdateLocSrvMsg(csUnit, "收到交易B5后C1(OBU号码和B5不同)", Color.DarkRed, rtb);
                        return;
                    }
                }
            }


            if (Global.PreAntenna != null && Global.PreAntenna.IsConnect())
                Global.PreAntenna.Send(csUnit.Buffer);

            UpdateLocSrvMsg(csUnit, "C1帧到来", Color.OrangeRed, rtb);
        }

        protected void HandleC3Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (Global.PreAntenna != null && Global.PreAntenna.IsConnect())
                Global.PreAntenna.Send(csUnit.Buffer);

            UpdateLocSrvMsg(csUnit, "C3帧到来", Color.Yellow, rtb);
        }
        protected void HandleCDFrame(CSUnit csUnit, RichTextBox rtb)
        {
            if ((csUnit.Buffer[8] & 1) > 0)
            {
                ;
            }
        }
 
        #endregion

        #region 线圈

        public void HandleRefreshCoilStatus()
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

        public void HandleVehComeMsg(int tmp, VehData vehData, RichTextBox rtb)
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

        public void HandleVehRasterComeMsg(RichTextBox rtb)
        {
            UpdateMeterMsgUI("光栅信号到来", Color.Orange, rtb);
        }

        public void HandleRasterComeMsg(RichTextBox rtb)
        {
            UpdateMeterMsgUI("车辆到位帧到来", Color.OrangeRed, rtb);
        }

        #endregion

        #region 运行参数
        public void HandleShowRunParams(RichTextBox rtb)
        {

        }

        public void HandleReSetParamsCmd()
        {

        }

        #endregion
    }
}
