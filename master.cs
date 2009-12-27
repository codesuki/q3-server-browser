using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace AllSeeingQuick
{
    public class master
    {
        public string[] ips;
        public int UpdateMaster()
        {
            IPEndPoint ipe = new IPEndPoint(Dns.GetHostEntry("monster.idsoftware.com").AddressList[0], 27950);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)sender;

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.ReceiveTimeout = 1000;
            s.Bind(endPoint);

            System.IO.MemoryStream bytes = new System.IO.MemoryStream(30);
            bytes.Write(new byte[] { 0xff, 0xff, 0xff, 0xff }, 0, 4);
            bytes.Write(Encoding.UTF8.GetBytes("getservers 68 empty full"), 0, 24);
            s.SendTo(bytes.ToArray(), ipe);

            List<byte[]> packetlist = new List<byte[]>();
            try
            {
                while (true)
                {
                    byte[] recvd = new byte[5000];
                    s.ReceiveFrom(recvd, ref senderRemote);
                    packetlist.Add(recvd);
                }
            }
            catch
            {
                s.Close();
            }

            ips = new string[5000];
            int i = 0;

            foreach (byte[] packet in packetlist)
            {
                int start = 22;
                int end = Encoding.Default.GetString(packet).IndexOf("\\EOT");
                while (start < end)
                {
                    ips[i++] = packet[start + 1] + "." + packet[start + 2] + "." + packet[start + 3] + "." + packet[start + 4] + ":" + ((packet[start + 5] * 256) + packet[start + 6]);
                    start += 7;
                }
            }
            Console.WriteLine("packets received: " + packetlist.Count);
            Console.WriteLine("servers received: " + i);

            return i;
        }
    }
}
