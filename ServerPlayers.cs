using System;
using System.Collections.Generic;
using System.Text;

namespace AllSeeingQuick
{
    public class ServerPlayers
    {
        private string m_name;
        private string m_ping;
        private string m_score;

        public string name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        public string ping
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

        public string score
        {
            get
            {
                return m_score;
            }
            set
            {
                m_score = value;
            }
        }
    }
}
