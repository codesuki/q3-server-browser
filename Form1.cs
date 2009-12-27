using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Net;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace AllSeeingQuick
{
    public partial class Form1 : Form
    {
        public master m;
        public Hashtable h;
        public Socket s;
        public Form1()
        {
            InitializeComponent();

            h = new Hashtable();
            s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 27960);
            s.Bind(endPoint);
            
            master m1 = new master();
            progressBar1.Maximum = m1.UpdateMaster();
            progressBar1.Value = 0;

            ArrayList servers = new ArrayList();
            servers.Add(new server("127.0.0.1", 1000, ref s));
           // dataGridView1.DataSource = servers;
            serverBindingSource.DataSource = servers;
           
            foreach (string ip in m1.ips)
            {
                if (ip != null)
                {
                    string _ip = ip.Substring(0, ip.IndexOf(':'));
                    int _port = Convert.ToInt32(ip.Substring(ip.IndexOf(':') + 1));
                    server s1 = new server(_ip, _port, ref  s);
                    h.Add(ip, s1);
                    servers.Add(s1);
                }
            }
        }

        

        private void showExInfo(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = !splitContainer1.Panel2Collapsed;
        }

        private void addServer(object sender, EventArgs e)
        {
        }

        private void addServerToList()
        {
        }

        private void updateSettingsView()
        {
        }

        private void updatePlayersView()
        {
        }

        private void queryMaster(object sender, EventArgs e)
        {
        }

        private void refreshServer(object sender, EventArgs e)
        {
        }

        public delegate void updateProgressBarDelegate();
        public void updateProgressBar() 
        {
            progressBar1.Value++;
        }

        public void receive()
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)sender;
            int i = 1;
            while (true)
            {
                try
                {
                    this.Invoke(new updateProgressBarDelegate(updateProgressBar));
                    //Console.WriteLine(i++);
                    byte[] recvd = new byte[9999];
                    s.ReceiveFrom(recvd, ref senderRemote);

                    string key = ((IPEndPoint)senderRemote).Address.ToString() + ":" + ((IPEndPoint)senderRemote).Port;
                    if (((server)h[key]) != null)
                    {
                        ((server)h[key]).data = Encoding.UTF8.GetString(recvd);
                        ((server)h[key]).waiting = false;
                        ((server)h[key]).parseInfoResponse();
                    }
                }
                catch (SocketException e)
                {
                    string key = ((IPEndPoint)senderRemote).Address.ToString() + ":" + ((IPEndPoint)senderRemote).Port;
                    if (((server)h[key]) != null)
                    {
                        ((server)h[key]).retries++;
                        if (((server)h[key]).retries == 3)
                        {
                            ((server)h[key]).down = true;
                            ((server)h[key]).waiting = false;
                        }
                    }
                }
                Thread.Sleep(0);
            }
        }

        public void send()
        {
            int i = 0;
            foreach (DictionaryEntry de in h)
            {
                ((server)de.Value).UpdateServer();
                i++;
                if (i%20 == 0) Thread.Sleep(150);
            }
           // Console.WriteLine("SERVERS: " + i);
            bool missingServer = false;
            i = 0;
            Thread.Sleep(100);

            do
            {
                missingServer = false;
                foreach (DictionaryEntry de in h)
                {
                    if (((server)de.Value).waiting && !((server)de.Value).down)
                    {
                        missingServer = true;
                    }
                }

                foreach (DictionaryEntry de in h)
                {
                    Thread.Sleep(1);
                    if (((server)de.Value).waiting && !((server)de.Value).down)
                    {
                        ((server)de.Value).UpdateServer();
                    }
                }
            } while (missingServer);
        }

        private void dataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            dataGridView2.DataSource = ((server)this.serverBindingSource.Current).properties;
            dataGridView3.DataSource = ((server)this.serverBindingSource.Current).players;
        }

        private void Shown(object sender, EventArgs e)
        {
            Thread recvThread = new Thread(new ThreadStart(receive));
            recvThread.IsBackground = true;
            recvThread.Start();

            Thread sendThread = new Thread(new ThreadStart(send));
            sendThread.IsBackground = true;
            sendThread.Start();
        }

        private void init(object sender, EventArgs e)
        {
            Thread recvThread = new Thread(new ThreadStart(receive));
            recvThread.IsBackground = true;
            recvThread.Start();

            Thread sendThread = new Thread(new ThreadStart(send));
            sendThread.IsBackground = true;
            sendThread.Start();
        }
    }
}