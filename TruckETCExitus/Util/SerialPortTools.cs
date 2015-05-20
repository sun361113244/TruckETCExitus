using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Util
{
    /// <summary>
    /// 版本1.2
    /// 更改：1.增加获取串口号，波特率
    /// </summary>
    public class SerialPortTools
    {
        #region 事件

        /// <summary>
        /// 串口接收事件
        /// </summary>
        public delegate void RecvFunc(byte[] buffer);

        #endregion

        #region 属性

        /// <summary>
        /// 控制串口
        /// </summary>
        private SerialPort serialPort;

        /// <summary>
        /// 串口缓存
        /// </summary>
        private byte[] srlPortBuffer;

        /// <summary>
        /// 接收数据事件
        /// </summary>
        public RecvFunc dataReceived;

        #endregion

        #region 构造函数

        public SerialPortTools()
        {
            serialPort = new SerialPort();
            serialPort.DataReceived += new SerialDataReceivedEventHandler(SrlPort_DataReceiver);
        }

        #endregion

        /// <summary>
        /// 串口接收事件函数
        /// </summary>
        private void SrlPort_DataReceiver(object sender, SerialDataReceivedEventArgs e)
        {
            int readLength = serialPort.BytesToRead;
            byte[] buffer = new byte[readLength];

            serialPort.Read(buffer, 0, readLength);

            if (dataReceived != null)
            {
                dataReceived(buffer);                
            }
        }
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="portName"> 串口号</param>
        /// <param name="baud"> 波特率</param>
        /// <returns>1:成功.11:失败.</returns>
        public int Open(string portName, int baud)
        {
            serialPort.BaudRate = baud;
            serialPort.PortName = portName;
            serialPort.DataBits = 8;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;

            serialPort.Open();            

            return 1;
        }

        /// <summary>
        /// 获取打开串口号
        /// </summary>
        /// <returns>串口号</returns>
        public string GetPortNo()
        {
            return serialPort.PortName;
        }

        /// <summary>
        /// 获取打开串口的波特率
        /// </summary>
        /// <returns>打开串口的波特率</returns>
        public int GetBaudRate()
        {
            return serialPort.BaudRate;
        }

        /// <summary>
        /// 串口是否打开
        /// </summary>
        /// <returns>1:成功.11:未打开.12:打开异常.</returns>
        public int IsOpen()
        {
            if (serialPort.IsOpen)
                return 1;
            else
                return 11;
        }
        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <returns>1:成功.11:关闭异常.</returns>
        public int Close()
        {
            serialPort.Close();
            
            return 1;
        }
        /// <summary>
        /// 使用缓冲区的数据将指定数量的字节写入串行端口。
        /// </summary>
        /// <param name="buffer"> 包含要写入端口的数据的字节数组</param>
        /// <param name="offset"> buffer 参数中从零开始的字节偏移量，从此处开始将字节复制到端口。</param>
        /// <param name="count"> 要写入的字节数。</param>
        /// <returns>1:成功.11:失败.</returns>
        public int Write(byte[] buffer, int offset, int count)
        {
            if (serialPort.IsOpen)
            {
                serialPort.Write(buffer, offset, count);                
            }
            return 1;
        }
    }
}
