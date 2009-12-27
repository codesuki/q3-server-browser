using System;
using System.Collections.Generic;
using System.Text;

namespace AllSeeingQuick
{
    public class ServerProperties
    {
        private string m_name;
        private string m_value;

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

        public string value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }
    }
}
