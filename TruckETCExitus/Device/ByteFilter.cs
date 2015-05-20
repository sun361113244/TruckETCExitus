using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TruckETCExitus.Device
{
    public class ByteFilter
    {
        public static List<byte> deFilter(byte[] buffer)
        {
            List<byte> frame = new List<byte>();
            for (int i = 0; i < buffer.Length - 1; i++)
            {
                if (buffer[i] == 0xFE && buffer[i + 1] == 0x01)
                {
                    frame.Add(0xFF);
                    i++;
                    continue;
                }
                if (buffer[i] == 0xFE && buffer[i + 1] == 0x00)
                {
                    frame.Add(0xFE);
                    i++;
                    continue;
                }
                frame.Add(buffer[i]);
            }
            frame.Add(buffer[buffer.Length - 1]);
            return frame;
        }

        public static List<byte> enFilter(List<byte> frame)
        {
            List<byte> list = new List<byte>();
            list.Add(0xFF);
            list.Add(0xFF);
            for (int i = 2; i < frame.Count - 1; i++)
            {
                if (frame[i] == 0xFF)
                {
                    list.Add(0xFE);
                    list.Add(0x01);
                    continue;
                }
                if (frame[i] == 0xFE)
                {
                    list.Add(0xFE);
                    list.Add(0x00);
                    continue;
                }
                list.Add(frame[i]);
            }
            list.Add(0xFF);
            return list;
        }

        public static byte[] enFilter(byte[] B2Frame)
        {
            List<byte> list = new List<byte>();
            list.AddRange(B2Frame);
            return enFilter(list).ToArray();
        }
    }
}
