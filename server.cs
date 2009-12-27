using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace AllSeeingQuick
{
    public class server
    {
        public IPEndPoint endPoint;
        public Socket s;
        private string m_data;
        private string m_players;
        private string m_properties;
        public bool down = false;
        public bool waiting = false;
        public int retries = 0;
        public byte[] getinfo;
        public byte[] getstatus;
        private string m_hostname;
        private string m_game;
        private DateTime qryStart;
        private int m_ping;
        private string m_maxplayers;
        private string m_map;
        private int m_playercount;

        public ArrayList properties;
        public ArrayList players;

        public bool isWaiting 
        {
            get
            {
                return waiting;
            }
            set
            {
                waiting = value;
            }
        }

        public bool isDown
        {
            get
            {
                return down;
            }
            set 
            {
                down = value;
            }
        }

        public string data
        {
            get
            {
                return m_data;
            }
            set 
            {
                m_data = value;
            }
        }

        public string rawPlayers
        {
            get
            {
                return m_players;
            }
            set
            {
                m_players = value;
            }
        }

        public string rawProperties
        {
            get
            {
                return m_properties;
            }
            set
            {
                m_properties = value;
            }
        }

        public string hostname
        {
            get
            {
                return m_hostname;
            }
            set 
            {
                m_hostname = value;
            }
        }

        public string ip 
        {
            get 
            {
                return endPoint.Address.ToString() + ":" + endPoint.Port.ToString();
            }
        }
      
        public string game
        {
            get
            {
                return m_game;
            }
            set
            {
                m_game = value;
            }
        }

        public string map
        {
            get
            {
                return m_map;
            }
            set
            {
                m_map = value;
            }
        }

        public int ping 
        {
            get
            {
                return m_ping;
            }
            set
            {
                m_ping = value;
            }
        }

        public string maxplayers
        {
            get 
            {
                return m_playercount + "/" + m_maxplayers;
            }
            set
            {
                m_maxplayers = value;
            }
        }

        public int playercount
        {
            get
            {
                return m_playercount;
            }
            set
            {
                m_playercount = value;
            }
        }

        public void parseInfoResponse() {


            DateTime now = DateTime.Now;
            TimeSpan png = now - qryStart;
            m_ping = (int)png.TotalMilliseconds;
            data = data.Remove(0, 20);
            rawProperties = data.Remove(data.IndexOf((char)0x0A));
            string[] split = rawProperties.Split(new Char[] { '\\' });

            int i = 1;
            ServerProperties prop = new ServerProperties();
            foreach (string s in split)
            {
                if (i == 2)
                {
                    if (prop.name.Equals("sv_hostname")) hostname = s;
                    if (prop.name.Equals("sv_maxclients")) maxplayers = s;
                    if (prop.name.Equals("mapname")) map = s;
                    if (prop.name.Equals("gamename")) game = s;
                    prop.value = s;
                    properties.Add(prop);
                    i = 1;
                }
                else
                {
                    prop = new ServerProperties();
                    prop.name = s;
                    i++;
                }
            }

            rawPlayers = data.Remove(0, data.IndexOf((char)0x0A)+1);
            if (rawPlayers[0] != '\0') {
                // players
                split = rawPlayers.Split(new Char[] { (char)0x0A });
                foreach (string s in split)
                {
                    if (s[0] != '\0')
                    {
                        string[] split2 = s.Split(new Char[] { ' ' });
                        ServerPlayers player = new ServerPlayers();
                        player.score = split2[0];
                        player.ping = split2[1];
                        player.name = split2[2].TrimStart(new Char[] { '"' }).TrimEnd(new Char[] { '"' });
                        players.Add(player);
                        playercount++;
                    }
                }
            }
         } 

        public server(string ip, int port, ref Socket _s)
        {
            properties = new ArrayList();
            players = new ArrayList();

            s = _s;
            endPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            System.IO.MemoryStream bytes = new System.IO.MemoryStream(30);
            bytes.Write(new byte[] { 0xff, 0xff, 0xff, 0xff }, 0, 4);
            bytes.Write(Encoding.UTF8.GetBytes("getinfo"), 0, 7);
            bytes.Flush();
            getinfo = bytes.ToArray();
            bytes.Close();

            bytes = new System.IO.MemoryStream(30);
            bytes.Write(new byte[] { 0xff, 0xff, 0xff, 0xff }, 0, 4);
            bytes.Write(Encoding.UTF8.GetBytes("getstatus"), 0, 9);
            bytes.Flush();
            getstatus = bytes.ToArray();
            bytes.Close();
        }

        public void UpdateServer()
        {
            retries++;
            if (retries > 3) {
                down = true;
                return;
            }

            try
            {
                //s.SendTo(getinfo, endPoint);
                s.SendTo(getstatus, endPoint);
                isWaiting = true;
                qryStart = DateTime.Now;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
