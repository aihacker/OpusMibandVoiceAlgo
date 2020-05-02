using System;
using OpusDotNet;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;

namespace Opus
{
    static class Program
    {
        public static byte[] HexStringToBytes(this string hexStr, char spliteChar = ' ') => (hexStr.IndexOf(spliteChar) == -1 ? Regex.Split(hexStr, @"(?<=\G.{2})(?!$)") : hexStr.Split(spliteChar)).Select(b => Convert.ToByte(b, 16)).ToArray();
        static void Main(string[] args)
        {
            OpusDecoder decoder = new OpusDecoder(16000, 1);
            //Console.WriteLine("Input the capture file path:");
            string capture = File.ReadAllText("capvoice.txt");
            List<byte> ret = new List<byte>();
            List<byte> output = new List<byte>();
            foreach (var item in capture.Split("\n"))
            {
                if (item.StartsWith("[Read]20"))//the voice sensor data
                {
                    var bytes = new List<byte>(item.Replace("[Read]20 ", "").Trim().HexStringToBytes());
                    //raw[0] = index of packet
                    bytes.RemoveAt(0);
                    ret.AddRange(bytes);
                }
            }
            int index = 0;
            while (index < ret.Count)
            {
                int encodeLength = ret[index];
                byte[] encodeData = new byte[encodeLength];
                byte[] decodeData = new byte[640];//maybe more than 640
                ret.CopyTo(index + 1, encodeData, 0, encodeLength);//cut the length
                int retlen = decoder.Decode(encodeData, encodeLength, decodeData, decodeData.Length);
                Console.WriteLine(retlen);
                output.AddRange(decodeData);
                //Console.WriteLine(length);
                ret.RemoveAt(index);
                index += encodeLength;
            }
            File.WriteAllBytes("output.pcm", output.ToArray());
            Console.WriteLine("Successcully,See output.pcm");
            Console.WriteLine("[Notice]16bit,16000Hz,1 channel");
        }
    }
}
