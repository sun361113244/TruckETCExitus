using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Util;

namespace TruckETCExitus.Model
{
    interface State
    {
        void InitControl(Button btnUICtrl);
        void HandlePreAntennaConn(CSUnit csUnit, RichTextBox rtb);

        void HandlePreAntennaClose(CSUnit csUnit, RichTextBox rtb);

        void HandlePreAntRecvData(CSUnit csUnit, RichTextBox rtb);

        void HandleTrdAntennaConn(CSUnit csUnit, RichTextBox rtb);

        void HandleTrdAntennaClose(CSUnit csUnit, RichTextBox rtb);

        void HandleTrdAntRecvData(CSUnit csUnit, RichTextBox rtb);

        void HandleLocSrvConn(CSUnit csUnit, RichTextBox rtb);

        void HandleLocSrvClose(CSUnit csUnit, RichTextBox rtb);

        void HandleLocSrvRecvData(CSUnit csUnit, RichTextBox rtb);

        void HandleRefreshCoilStatus();

        void HandleDataFrameComeMsg(byte[] buffer, RichTextBox rtb);

        void HandleVehComeMsg(int tmp,VehData vehData, RichTextBox rtb);

        void HandleVehRasterComeMsg( RichTextBox rtb);

        void HandleRasterComeMsg( RichTextBox rtb);

        void HandleShowRunParams(RichTextBox rtb);


        void HandleReSetParamsCmd();
    }
}
