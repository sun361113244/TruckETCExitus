using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Util
{
    public class SystemUnit
    {
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(
            int hWnd,        // handle to destination window
            int Msg,            // message
            int wParam,         // first message parameter
            int lParam          // second message parameter
            );
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern int PostMessage(
            int hWnd,        // 信息发往的窗口的句柄  
            int Msg,            // 消息ID  
            int wParam,         // 参数1  
            byte[] lParam //参数2  
        );
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(int hWnd, int msg, int wParam, uint lParam);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(int hWnd, int msg, int wParam, byte[] lParam);
        [DllImport("user32.dll")]
        public static extern uint RegisterWindowMessage(string lpString);

        public const int HWND_BROADCAST = 0xFFFF;

        /// <summary>
        /// 计算BCC校验值
        /// </summary>
        /// <param name="temp">输入缓存</param>
        /// <param name="len">缓存长度</param>
        /// <returns>BCC校验值</returns>
        public static byte Get_CheckXor(byte[] temp, int len)
        {
            byte A = 0;
            for (int i = 2; i < len - 2; i++)
            {
                A ^= temp[i];
            }
            return A;
        }

        /// <summary>
        /// 计算BCC校验值
        /// </summary>
        /// <param name="frame">输入缓存</param>
        /// <param name="len">缓存长度</param>
        /// <returns>BCC校验值</returns>
        public static byte Get_CheckXor(List<byte> frame, int len)
        {
            byte A = 0;
            for (int i = 2; i < len - 2; i++)
            {
                A ^= frame[i];
            }
            return A;
        }
        /// <summary>
        /// 字符流转为字符串
        /// </summary>
        /// <param name="bytes">字符流</param>
        /// <returns>转换为字符串的字符流</returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2") + " ";
                }
            }
            return returnStr;
        }
        /// <summary>
        /// 字符流转为字符串
        /// </summary>
        /// <param name="list">字符流</param>
        /// <returns>转换为字符串的字符流</returns>
        public static string byteToHexStr(List<byte> list)
        {
            string returnStr = "";
            if (list != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    returnStr += list[i].ToString("X2") + " ";
                }
            }
            return returnStr;
        }
    }
}
