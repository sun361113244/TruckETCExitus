using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Util
{
    /// <summary>
    /// 数据报文分析器,通过分析接收到的原始数据,得到完整的数据报文.
    /// 继承该类可以实现自己的报文解析方法.
    /// 通常的报文识别方法包括:固定长度,长度标记,标记符等方法
    /// 本类的现实的是标记符的方法,你可以在继承类中实现其他的方法
    /// </summary>
    public class DatagramResolver
    {
        /// <summary>
        /// 报文结束标记
        /// </summary>
        private byte endTag;

        /// <summary>
        /// 返回结束标记
        /// </summary>
        byte EndTag
        {
            get
            {
                return endTag;
            }
        }

        /// <summary>
        /// 受保护的默认构造函数,提供给继承类使用
        /// </summary>
        protected DatagramResolver()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="endTag">报文结束标记</param>
        public DatagramResolver(byte endTag)
        {
            if (endTag == null)
            {
                throw (new ArgumentNullException("结束标记不能为null"));
            }

            this.endTag = endTag;
        }

        /// <summary>
        /// 解析报文
        /// </summary>
        /// <param name="rawDatagram">原始数据,返回未使用的报文片断,
        /// 该片断会保存在Session的Datagram对象中</param>
        /// <returns>报文数组,原始数据可能包含多个报文</returns>
        public virtual List<byte>[] Resolve(ref List<byte> rawDatagram)
        {
            ArrayList datagrams = new ArrayList();

            //末尾标记位置索引
            int tagIndex = -1;

            while (true)
            {
                if (rawDatagram.Count > 2)
                    tagIndex = rawDatagram.IndexOf(endTag, 2);
                else
                    tagIndex = -1;

                if (tagIndex == -1)
                {
                    break;
                }
                else
                {
                    //按照末尾标记把字符串分为左右两个部分
                    List<byte> newDatagram = new List<byte>();
                    for (int i = 0; i <= tagIndex; i++)
                        newDatagram.Add(rawDatagram[i]);

                    datagrams.Add(newDatagram);

                    rawDatagram.RemoveRange(0, tagIndex + 1);

                    //从开始位置开始查找
                    tagIndex = 0;
                }
            }

            List<byte>[] results = new List<byte>[datagrams.Count];

            datagrams.CopyTo(results);

            return results;
        }

    }

}

