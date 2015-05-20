using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Util
{
    class RichTextBoxUtil
    {
        public static void UpdateRTxtUI(RichTextBox rtb, string msg, Color fontColor)
        {
            rtb.Invoke(new EventHandler(delegate
            {
                if (rtb.Lines.Length % 30 == 0)
                {
                    rtb.Text = "";
                }
                rtb.SelectionStart = rtb.Text.Length;
                rtb.SelectionColor = fontColor;
                rtb.AppendText(msg);
                rtb.ScrollToCaret();
            }));
        }

        public static void SetRTxtUI(RichTextBox rtb, string msg, Color fontColor)
        {
            rtb.Invoke(new EventHandler(delegate
            {
                rtb.SelectionStart = rtb.Text.Length;
                rtb.SelectionColor = fontColor;
                rtb.Text = msg;
                rtb.ScrollToCaret();
            }));
        }
    }
}
