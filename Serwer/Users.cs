using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.NetworkInformation;

namespace Serwer
{
    
    class Users
    {
        public int nr { get; set; }
        public string IP { get; set; }
        public Client client { get; set; }
        
        public Dictionary<string, bool> ping = new Dictionary<string, bool>();

        public string DefoultGateaway;


        public Users(int new_nr, string new_ip, TcpClient new_tcp)
        {
            nr = new_nr;

            IP = GetGateway(new_ip);
            client = new Client(new_tcp, "1");
        } 


        public void NewPing(string comm)
        {
            bool TorF;
            String value = comm;
            Char delimiter = '#';
            String[] substrings = new String[2];
            substrings=value.Split(delimiter);

            value= substrings[0];
            if (substrings[1] == "True")
            {
                TorF = true;
            }
            else
            {
                TorF = false;
            }


            bool temp=true;
            foreach (KeyValuePair<string, bool> i in ping)
            {
                if(i.Key==value)
                {
                    temp = false;
                }
            }
            if (temp == true)
            {
                ping.Add(value, TorF);
            }
        }

        private string GetGateway(string addr)
        {
            string IP = "";
            string gate = "";

            String value = addr;
            Char delimiter = '#';
            String[] substrings = new String[2];
            substrings = value.Split(delimiter);
            IP = substrings[0];
            gate = substrings[1];
            DefoultGateaway = gate;
            return IP;
        }
    }
}
