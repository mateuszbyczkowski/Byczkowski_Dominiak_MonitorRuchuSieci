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
            IP = new_ip;
            client = new Client(new_tcp, "1");

            String value = new_ip;
            Char delimiter = '.';
            String[] substrings = new String[2];
            substrings = value.Split(delimiter);
            substrings[3] = "1";
            value = "";
            foreach (var substring in substrings)
            {
                value = value + substring + ".";
            }
            value = value.Remove(value.Length - 1);
            


           // bool pinging = false;
           // Ping isPing = new Ping();
       
           //try
           //{
           //     PingReply reply = isPing.Send(value);
           //     pinging = reply.Status == IPStatus.Success;
           //}
           //catch (PingException)
           //{

           //}

           // if (pinging == true)
           // {
           //     NewPing("Router IP: " + value + "#" + pinging.ToString());
           // }


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
    }
}
