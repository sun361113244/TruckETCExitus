using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TruckETCExitus.Device;
using TruckETCExitus.Etc;
using Util;

namespace TruckETCExitus.Model
{
    class ExitusLiZhiC2CoilState : ExitusState
    {

        private int g_u8waitPrdCarNum = 0;

        private int g_u8PrdCarNum = 0;

        private int g_u8TrdCarNum = 0;

        private int g_u8PassStatus = 0;

        private int g_u32PrdFailOBU = 0;


        private enum AlartStat
        {
            NonAlarmed = 1,
            Alarmed,
            HalfAlarmed
        };
        private AlartStat prdAlartStat = AlartStat.NonAlarmed;

        private AlartStat trdAlartStat = AlartStat.NonAlarmed;

        #region 预读天线

        public override void HandlePreAntRecvData(CSUnit csUnit, RichTextBox rtb)
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
                        tmrPre.Enabled = true;
                        if (g_u8waitPrdCarNum > 0 && prdAlartStat == AlartStat.NonAlarmed)
                        {
                            HandleD1Frame(csUnit, rtb);
                        }
                        else
                        {
                            Global.PreAntenna.Send(Antenna.C2Frame);
                            UpdatePreAntannaMsg(csUnit, "D1到来发送C2", Color.Red, rtb);
                            ResetPreStep();
                        }
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
        /// 只有预读步骤为0时执行(即第一次收到D1,tmrPre后重置).
        /// 收到后记录OBU号,回复C1.
        /// 根据D1、D2内容生成预读B2.
        /// </summary>
        /// <param name="csUnit">远程单元</param>
        protected virtual void HandleD1Frame(CSUnit csUnit, RichTextBox rtb)
        {
            string info = "";
            if (preStep == 0)
            {
                D1Frame.Clear();
                D1Frame = ByteFilter.deFilter(csUnit.Buffer);

                preAntanaOBUNo = csUnit.Buffer[4] * 16777216 + csUnit.Buffer[5] * 65536
                    + csUnit.Buffer[6] * 256 + csUnit.Buffer[7];

                if (Global.preOBUQueue.Contains(new OBUData(preAntanaOBUNo)) ||
                    (g_u32PrdFailOBU != preAntanaOBUNo && g_u32PrdFailOBU != 0))
                {
                    Global.PreAntenna.Send(Antenna.C2Frame);
                    UpdatePreAntannaMsg(csUnit, "D1到来发送C2 122", Color.Red, rtb);
                    return;
                }

                if (D1Frame.Count == Antenna.D1_LENGTH)
                {
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
            else
            {
                UpdatePreAntannaMsg(csUnit, "D1到来顺序错误", Color.Red, rtb);
                ResetPreStep();
            }
        }


        #endregion

        #region 交易天线

        public override void HandleTrdAntRecvData(CSUnit csUnit, RichTextBox rtb)
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
                        if (trdAlartStat == AlartStat.NonAlarmed)
                        {
                            tmrTrd.Enabled = true;
                            HandleB2Frame(csUnit, rtb);
                        }
                        else
                        {
                            Global.TrdAntenna.Send(Antenna.C2Frame);
                            ResetTrdStep();
                        }
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


        protected override void HandleB2Frame(CSUnit csUnit, RichTextBox rtb)
        {
            if (csUnit.Buffer[Antenna.HEARTBEAT_LOC] == Antenna.HEARTBEAT_CONTENT)
            {
                //心跳包
                if (Global.localServer != null && Global.localServer.GetConnectionCount() > 0)
                    Global.localServer.Send(csUnit.Buffer);
            }
            else
            {
                if (trdStep == 0)
                {
                    List<byte> B2frame = ByteFilter.deFilter(csUnit.Buffer);
                    if (B2frame.Count == Antenna.B2_TRD_LENGTH)
                    {
                        trdAntanaOBUNo = csUnit.Buffer[5] * 16777216 + csUnit.Buffer[6] * 65536
                        + csUnit.Buffer[7] * 256 + csUnit.Buffer[8];
                        /////////////////////////////////////////////////////////////////////////////////////
                        OBUData trdObu;
                        if (Global.exchangeQueue.obuQueue.Count > 0)
                            trdObu = Global.exchangeQueue.obuQueue.Peek();
                        else
                            trdObu = new OBUData(0);
                        if (trdObu.ObuNum != trdAntanaOBUNo)
                        {
                            Global.TrdAntenna.Send(Antenna.C2Frame);
                            UpdateTrdAntannaMsg(csUnit, "B2帧到来", Color.CornflowerBlue, rtb);
                            ResetTrdStep();
                            return;
                        }
                        /////////////////////////////////////////////////////////////////////////////////////
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
                else
                {
                    UpdateTrdAntannaMsg(csUnit, "B2帧到来顺序异常:" + trdStep, Color.Red, rtb);
                    ResetTrdStep();
                }

            }
        }

        #endregion

        #region 本地服务器

        public override void HandleLocSrvRecvData(CSUnit csUnit, RichTextBox rtb)
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
                    case 0xC2:
                        HandleC2Frame(csUnit, rtb);
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

        protected override void HandleC1Frame(CSUnit csUnit, RichTextBox rtb)
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

                        // B2后收到C1表示PC已经同意放行，所以此时不管预读有无报警都应进队列
                        PreProcessSucess(c1OBUNo);
                        if (g_u8waitPrdCarNum > 0)
                        {
                            g_u8waitPrdCarNum -= 1;
                        }
                        g_u8PrdCarNum += 1;
                        if (g_u32PrdFailOBU != 0)
                        {
                            g_u32PrdFailOBU = 0;
                        }

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
                            g_u8TrdCarNum += 1;
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
        private void HandleC2Frame(CSUnit csUnit, RichTextBox rtb)
        {
            int c2OBUNo = csUnit.Buffer[4] * 16777216 + csUnit.Buffer[5] * 65536
                   + csUnit.Buffer[6] * 256 + csUnit.Buffer[7];

            if (c2OBUNo == preAntanaOBUNo)
            {
                if (prdAlartStat == AlartStat.NonAlarmed)
                {
                    g_u32PrdFailOBU = c2OBUNo;
                }
                Global.PreAntenna.Send(Antenna.C2Frame);
                ResetPreStep();
            }
            else
            {
                if (c2OBUNo == trdAntanaOBUNo)
                {
                    Global.TrdAntenna.Send(Antenna.C2Frame);
                    ResetTrdStep();
                }
                else
                {
                    UpdateLocSrvMsg(csUnit, "收到异常C2", Color.LightGreen, rtb);
                    return;
                }
            }
        }
        protected override void PreProcessSucess(int c1OBUNo)
        {
            Global.preOBUQueue.Enqueue(new OBUData(c1OBUNo));
        }

        protected override void HandleCDFrame(CSUnit csUnit, RichTextBox rtb)
        {
            g_u8TrdCarNum = 0;
            if (Global.coils.CoilStatus[6].StatNow == Coil.CurStatus.Sheltered)
            {
                trdAlartStat = AlartStat.HalfAlarmed;

                Global.trdAlartStat1 = Global.AlartStat.HalfAlarmed;
            }
            else//交易链路释放
            {
                trdAlartStat = AlartStat.NonAlarmed;

                Global.trdAlartStat1 = Global.AlartStat.NonAlarmed;
                byte[] b9frame = Antenna.createB9Frame(0x0d, 0, 0, 0, 1);
                //                Global.localServer.Send(b9frame);
            }

            int CDobuNo = (int)(csUnit.Buffer[4] * Math.Pow(2, 24) + csUnit.Buffer[5] * Math.Pow(2, 16) +
                csUnit.Buffer[6] * Math.Pow(2, 8) + csUnit.Buffer[7] * Math.Pow(2, 0));
            OBUData cdOBU = new OBUData(CDobuNo);

            try
            {
                if (Global.exchangeQueue.obuQueue.Contains(cdOBU))
                {
                    while (Global.exchangeQueue.obuQueue.Count > 0)
                    {
                        OBUData obuData = Global.exchangeQueue.obuQueue.Peek();
                        if (obuData.Equals(cdOBU))
                        {
                            Global.exchangeQueue.obuQueue.Dequeue();
                            Global.exchangeQueue.vehQueue.Dequeue();
                            break;
                        }
                        else
                        {
                            Global.exchangeQueue.obuQueue.Dequeue();
                            Global.exchangeQueue.vehQueue.Dequeue();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw new Exception("CD帧 obu数据出队列报错");
            }
        }
        #endregion

        #region 线圈

        private void preOBUDataToPreWeightQueue()
        {
            if (Global.preOBUQueue.Count > 0)
            {
                Global.preQueue.obuQueue.Enqueue(Global.preOBUQueue.Dequeue());
            }
            else
                throw new Exception("预读obu队列进入称重队列时：obu队列数量为0");
        }
        public override void HandleRefreshCoilStatus()
        {
            // #1触发
            if (Global.coils.CoilStatus[0].CoilStat == Coil.CoilStatus.Trigger)
            {
                //跟车干扰判断
                if (g_u8waitPrdCarNum == 0)
                {
                    g_u8waitPrdCarNum += 1;
                }
                else
                {
                    if (prdAlartStat == AlartStat.NonAlarmed)//预读链路锁定
                    {
                        prdAlartStat = AlartStat.Alarmed;

                        Global.prdAlartStat1 = Global.AlartStat.Alarmed;
                        byte[] b9frame = Antenna.createB9Frame(0x0d, 0, 0, 2, 0);
                        //                        Global.localServer.Send(b9frame);
                    }
                }
                // 处于报警状态，#1触发，#2未遮挡，#3未遮挡，还原报警状态
                if (trdAlartStat == AlartStat.NonAlarmed && Global.coils.CoilStatus[1].StatNow == Coil.CurStatus.NonSheltered
                    && Global.coils.CoilStatus[1].StatNow == Coil.CurStatus.NonSheltered)
                {
                    if (prdAlartStat == AlartStat.Alarmed)//预读链路释放
                    {
                        prdAlartStat = AlartStat.NonAlarmed;
                        if (g_u32PrdFailOBU != 0)
                        {
                            g_u32PrdFailOBU = 0;
                        }

                        Global.prdAlartStat1 = Global.AlartStat.NonAlarmed;
                        byte[] b9frame = Antenna.createB9Frame(0x0d, 0, 0, 1, 0);
                        //                        Global.localServer.Send(b9frame);
                    }
                }
            }
            // #2收尾
            if (Global.coils.CoilStatus[1].CoilStat == Coil.CoilStatus.End)
            {
                //线圈3不遮挡时，还原报警状态
                if (Global.coils.CoilStatus[2].StatNow == Coil.CurStatus.NonSheltered)
                {
                    if (Global.preOBUQueue.Count == 0)
                    {
                        g_u8PrdCarNum = 0;
                    }
                    if (prdAlartStat == AlartStat.Alarmed)//预读链路释放
                    {
                        if (g_u8waitPrdCarNum != 1)
                        {
                            g_u8waitPrdCarNum = 1;
                        }
                        if (trdAlartStat == AlartStat.NonAlarmed)
                        {
                            prdAlartStat = AlartStat.NonAlarmed;
                            if (g_u32PrdFailOBU != 0)
                            {
                                g_u32PrdFailOBU = 0;
                            }

                            Global.prdAlartStat1 = Global.AlartStat.NonAlarmed;
                            byte[] b9frame = Antenna.createB9Frame(0x0d, 0, 0, 1, 0);
                            //                        Global.localServer.Send(b9frame);
                        }
                    }
                }
            }
            // #3触发，处于未报警状态，无obu数据，则进入报警状态
            if (Global.coils.CoilStatus[2].CoilStat == Coil.CoilStatus.Trigger)
            {
                if (g_u8PrdCarNum > 0)
                {
                    g_u8PrdCarNum = g_u8PrdCarNum - 1;
                }
                else
                {
                    if (prdAlartStat == AlartStat.NonAlarmed)//预读链路锁定
                    {
                        prdAlartStat = AlartStat.Alarmed;

                        Global.prdAlartStat1 = Global.AlartStat.Alarmed;
                        byte[] b9frame = Antenna.createB9Frame(0x0d, 0, 0, 2, 0);
                        //                        Global.localServer.Send(b9frame);
                    }
                }
            }
            // #4触发
            if (Global.coils.CoilStatus[3].CoilStat == Coil.CoilStatus.Trigger)
            {
                g_u8PassStatus = 0;
            }
            // #4收尾，若光栅已遮挡，表示车辆已进入称重区域，因此不管预读有无报警，预读队列obu数据进入待称重队列
            if (Global.coils.CoilStatus[3].CoilStat == Coil.CoilStatus.End)
            {
                if (g_u8PassStatus == 1)
                {
                    preOBUDataToPreWeightQueue();
                }
            }
            //预读区域变量初始化
            if (Global.coils.CoilStatus[0].StatNow == Coil.CurStatus.NonSheltered
                && Global.coils.CoilStatus[1].StatNow == Coil.CurStatus.NonSheltered &&
                Global.coils.CoilStatus[2].StatNow == Coil.CurStatus.NonSheltered)
            {
                g_u8PrdCarNum = 0;
                g_u8waitPrdCarNum = 0;
                g_u32PrdFailOBU = 0;
                if (Global.coils.CoilStatus[3].StatNow == Coil.CurStatus.NonSheltered)
                {
                    //预读队列全部清除
                    if (Global.preOBUQueue.Count > 0)//允许放行的车辆数为0
                    {
                        byte[] b9frame = Antenna.createB9Frame(0x0d, 0, 2, 0, 0);
                        //                        Global.localServer.Send(b9frame);
                    }
                    while (Global.preOBUQueue.Count > 0)
                    {                        
                        Global.preOBUQueue.Dequeue();
                    }
                }
                else
                {
                    //预读队列只保留头队列，其余数据全部清除
                    if (Global.preOBUQueue.Count > 0)
                    {
                        if (Global.preOBUQueue.Count > 1)//允许放行的车辆数为1
                        {

                            byte[] b9frame = Antenna.createB9Frame(0x0d, 0, 1, 0, 0);
                            //                        Global.localServer.Send(b9frame);
                        }
                        OBUData firstOub = Global.preOBUQueue.Dequeue();
                        while (Global.preOBUQueue.Count > 0)
                        {
                            Global.preOBUQueue.Dequeue();
                        }
                        Global.preOBUQueue.Enqueue(firstOub);
                    }
                }
            }
            //线圈7触发
            if (Global.coils.CoilStatus[6].CoilStat == Coil.CoilStatus.Trigger)
            {
                if (g_u8TrdCarNum > 0)
                {
                    g_u8TrdCarNum = g_u8TrdCarNum - 1;
                }
                else
                {
                    if (trdAlartStat != AlartStat.Alarmed)//交易链路锁定
                    {
                        trdAlartStat = AlartStat.Alarmed;

                        Global.trdAlartStat1 = Global.AlartStat.Alarmed;

                        byte[] b9frame = Antenna.createB9Frame(0x0d, 0, 0, 0, 2);
                        if (prdAlartStat == AlartStat.NonAlarmed)//预读链路锁定
                        {
                            prdAlartStat = AlartStat.Alarmed;

                            Global.prdAlartStat1 = Global.AlartStat.Alarmed;
                            b9frame = Antenna.createB9Frame(0x0d, 0, 0, 2, 2);
                        }
                        //                       Global.localServer.Send(b9frame);
                    }
                }
            }
            //线圈7收尾
            if (Global.coils.CoilStatus[6].CoilStat == Coil.CoilStatus.End)
            {
                if (trdAlartStat != AlartStat.NonAlarmed)
                {
                    trdAlartStat = AlartStat.NonAlarmed;
                    Global.trdAlartStat1 = Global.AlartStat.NonAlarmed;
                }
            }
            //线圈8收尾
            if (Global.coils.CoilStatus[7].CoilStat == Coil.CoilStatus.End)
            {
                if (trdAlartStat == AlartStat.HalfAlarmed)
                {
                    if (Global.coils.CoilStatus[6].StatNow == Coil.CurStatus.Sheltered)//交易链路锁定
                    {
                        trdAlartStat = AlartStat.Alarmed;

                        Global.trdAlartStat1 = Global.AlartStat.Alarmed;
                        byte[] b9frame = Antenna.createB9Frame(0x0d, 0, 0, 0, 2);
                        if (prdAlartStat == AlartStat.NonAlarmed)//预读链路锁定
                        {
                            prdAlartStat = AlartStat.Alarmed;

                            Global.prdAlartStat1 = Global.AlartStat.Alarmed;
                            b9frame = Antenna.createB9Frame(0x0d, 0, 0, 2, 2);
                        }
                        //                       Global.localServer.Send(b9frame);
                    }
                    else//交易链路释放
                    {
                        trdAlartStat = AlartStat.NonAlarmed;

                        Global.trdAlartStat1 = Global.AlartStat.NonAlarmed;
                        byte[] b9frame = Antenna.createB9Frame(0x0d, 0, 0, 0, 1);
                        //                       Global.localServer.Send(b9frame);
                    }
                }
            }
        }

        #endregion

        #region 仪表

        private void preDataToExchangeQueue()
        {
            VehData vehData = Global.preQueue.vehQueue.Dequeue();
            switch (Global.preQueue.obuQueue.Count)
            {
                case 0:
                    Global.exchangeQueue.obuQueue.Enqueue(new OBUData(0));
                    Global.exchangeQueue.vehQueue.Enqueue(vehData.Clone());
                    break;
                case 1:
                    Global.exchangeQueue.obuQueue.Enqueue(Global.preQueue.obuQueue.Dequeue());
                    Global.exchangeQueue.vehQueue.Enqueue(vehData.Clone());
                    break;
                default:
                    for (int i = 0; i < Global.preQueue.obuQueue.Count - 1; i++)
                    {
                        Global.exchangeQueue.obuQueue.Enqueue(Global.preQueue.obuQueue.Dequeue());
                        Global.exchangeQueue.vehQueue.Enqueue(new VehData());
                    }
                    Global.exchangeQueue.obuQueue.Enqueue(Global.preQueue.obuQueue.Dequeue());
                    Global.exchangeQueue.vehQueue.Enqueue(vehData.Clone());
                    break;
            }
        }

        public override void HandleVehComeMsg(int tmp, VehData vehData, RichTextBox rtb)
        {
            if (tmp == 1)
            {
                Global.preQueue.vehQueue.Enqueue(vehData);
                preDataToExchangeQueue();

                UpdateMeterMsgUI("称重数据到来", Color.Blue, rtb);
            }
            else
            {
                UpdateMeterMsgUI("称重数据到来错误!", Color.Red, rtb);
            }
        }

        public override void HandleVehRasterComeMsg(RichTextBox rtb)
        {
            //光栅触发
            if (Global.raster.RasterStatus == Raster.RasterStat.Trigger)
            {
                //线圈4遮挡
                if (Global.coils.CoilStatus[3].StatNow == Coil.CurStatus.Sheltered)
                {
                    g_u8PassStatus = 1;
                }
                if ((Global.raster.VehStatWord & 1) == 0 && Global.preQueue.obuQueue.Count > 0)
                {
                    if (Global.raster.VehCount == 0)
                    {
                        while (Global.preQueue.obuQueue.Count > 0)
                        {
                            VehData vehData = new VehData();
                            OBUData obuData = Global.preQueue.obuQueue.Dequeue();

                            Global.exchangeQueue.obuQueue.Enqueue(obuData);
                            Global.exchangeQueue.vehQueue.Enqueue(vehData);

                        }
                    }
                    else if (Global.preQueue.obuQueue.Count > Global.raster.VehCount && Global.raster.VehCount > 0)
                    {
                        while (Global.preQueue.obuQueue.Count > Global.raster.VehCount)
                        {
                            VehData vehData = new VehData();
                            OBUData obuData = Global.preQueue.obuQueue.Dequeue();

                            Global.exchangeQueue.obuQueue.Enqueue(obuData);
                            Global.exchangeQueue.vehQueue.Enqueue(vehData);

                        }
                    }
                }
            }

            UpdateMeterMsgUI("车辆到位帧到来--光栅触发", Color.Orange, rtb);
        }

        public override void HandleRasterComeMsg(RichTextBox rtb)
        {
            //光栅触发
            if (Global.raster.RasterStatus == Raster.RasterStat.Trigger)
            {
                //线圈4遮挡
                if (Global.coils.CoilStatus[3].StatNow == Coil.CurStatus.Sheltered)
                {
                    g_u8PassStatus = 1;
                }
            }
            if ((Global.raster.VehStatWord & 1) == 0 && Global.preQueue.obuQueue.Count > 0)
            {
                if (Global.raster.VehCount == 0)
                {
                    while (Global.preQueue.obuQueue.Count > 0)
                    {
                        VehData vehData = new VehData();
                        OBUData obuData = Global.preQueue.obuQueue.Dequeue();

                        Global.exchangeQueue.obuQueue.Enqueue(obuData);
                        Global.exchangeQueue.vehQueue.Enqueue(vehData);

                    }
                }
                else if (Global.preQueue.obuQueue.Count > Global.raster.VehCount && Global.raster.VehCount > 0)
                {
                    while (Global.preQueue.obuQueue.Count > Global.raster.VehCount)
                    {
                        VehData vehData = new VehData();
                        OBUData obuData = Global.preQueue.obuQueue.Dequeue();

                        Global.exchangeQueue.obuQueue.Enqueue(obuData);
                        Global.exchangeQueue.vehQueue.Enqueue(vehData);

                    }
                }
            }
            if (Global.raster.RasterStatus == Raster.RasterStat.Trigger)
            {
                UpdateMeterMsgUI("车辆状态帧到来--光栅触发", Color.OrangeRed, rtb);
            }
            else
            {
                UpdateMeterMsgUI("车辆状态帧到来--光栅收尾", Color.OrangeRed, rtb);
            }
        }

        #endregion

        #region 运行参数

        private void UpdateRunParamsMsg(string msg, Color color, RichTextBox rtb)
        {
            if (InitParams.UIEnabled)
            {
                RichTextBoxUtil.SetRTxtUI(rtb, msg, color);
            }
        }

        public override void HandleShowRunParams(RichTextBox rtb)
        {
            string msg = string.Format("g_u8waitPrdCarNum = {0},g_u8PrdCarNum = {1},g_u8TrdCarNum = {2},g_u8PassStatus = {3},g_u32PrdFailOBU = {4}\r\n" +
                "preStep = {5} , trdStep={6} ,preAntanaOBUNo={7},trdAntanaOBUNo={8}",
                g_u8waitPrdCarNum, g_u8PrdCarNum, g_u8TrdCarNum, g_u8PassStatus, g_u32PrdFailOBU,
                preStep, trdStep, preAntanaOBUNo, trdAntanaOBUNo);
            UpdateRunParamsMsg(msg, Color.Blue, rtb);
        }

        public override void HandleReSetParamsCmd()
        {
            g_u8waitPrdCarNum = 0;

            g_u8PrdCarNum = 0;

            g_u8TrdCarNum = 0;

            g_u8PassStatus = 0;

            prdAlartStat = AlartStat.NonAlarmed;

            trdAlartStat = AlartStat.NonAlarmed;

            g_u32PrdFailOBU = 0;

            Global.prdAlartStat1 = Global.AlartStat.NonAlarmed;

            Global.trdAlartStat1 = Global.AlartStat.NonAlarmed;

            Global.preQueue.Clear();
            Global.preOBUQueue.Clear();
            Global.exchangeQueue.Clear();
        }

        #endregion
    }
}
