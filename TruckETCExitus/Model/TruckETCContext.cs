using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Util;

namespace TruckETCExitus.Model
{
    class TruckETCContext
    {
        private State run_State;

        public TruckETCContext(State curState)
        {
            run_State = curState;
        }

        public void InitControl(Button btnUICtrl)
        {
            run_State.InitControl(btnUICtrl);
        }

        public void HandleLocSrvConn(CSUnit csUnit, RichTextBox rtb)
        {
            run_State.HandleLocSrvConn(csUnit, rtb);
        }

        public void HandleLocSrvClose(CSUnit csUnit, RichTextBox rtb)
        {
            run_State.HandleLocSrvClose(csUnit, rtb);
        }

        public void HandleLocSrvRecvData(CSUnit csUnit, RichTextBox rtb)
        {
            run_State.HandleLocSrvRecvData(csUnit, rtb);
        }

        public void HandlePreAntConn(CSUnit csUnit, RichTextBox rtb)
        {
            run_State.HandlePreAntennaConn(csUnit, rtb);
        }

        public void HandlePreAntClose(CSUnit csUnit, RichTextBox rtb)
        {
            run_State.HandlePreAntennaClose(csUnit, rtb);
        }

        public void HandlePreAntRecvData(CSUnit csUnit, RichTextBox rtb)
        {
            run_State.HandlePreAntRecvData(csUnit, rtb);
        }

        public void HandleTrdAntConn(CSUnit csUnit, RichTextBox rtb)
        {
            run_State.HandleTrdAntennaConn(csUnit, rtb);
        }

        public void HandleTrdAntClose(CSUnit csUnit, RichTextBox rtb)
        {
            run_State.HandleTrdAntennaClose(csUnit, rtb);
        }

        public void HandleTrdAntRecvData(CSUnit csUnit, RichTextBox rtb)
        {
            run_State.HandleTrdAntRecvData(csUnit, rtb);
        }

        public void HandleRefreshCoilStatus()
        {
            run_State.HandleRefreshCoilStatus();
        }

        public void HandleDataFrameComeMsg(byte[] buffer, RichTextBox rtb)
        {
            run_State.HandleDataFrameComeMsg(buffer, rtb);
        }

        public void HandleVehComeMsg(int tmp ,VehData vehData, RichTextBox rtb)
        {
            run_State.HandleVehComeMsg(tmp, vehData, rtb);
        }

        public void HandleVehRasterComeMsg(RichTextBox rtb)
        {
            run_State.HandleVehRasterComeMsg(rtb);
        }

        public void HandleRasterComeMsg(RichTextBox rtb)
        {
            run_State.HandleRasterComeMsg(rtb);
        }

        public void HandleShowRunParams(RichTextBox rtb)
        {
            run_State.HandleShowRunParams(rtb);
        }

        public void HandleReSetParamsCmd()
        {
            run_State.HandleReSetParamsCmd();
        }
    }
}
