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
        public List<string> MAC { get; set; }
        public Client client { get; set; }

        public Dictionary<string, bool> ping = new Dictionary<string, bool>();

        public string DefoultGateaway;
        public int Number_of_connection=0;

        public static List<string> connection = new List<string>() ;


        public Users(int new_nr, string new_ip, TcpClient new_tcp)
        {
            nr = new_nr;
            MAC = new List<string>();
            GetGateway(new_ip);
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

            string tempMAC = "";
            String value = addr;
            Char delimiter = '#';
            String[] substrings = new String[2];
            substrings = value.Split(delimiter);
            IP = substrings[0];
            tempMAC = substrings[1];
            GetMAC(tempMAC);
            DefoultGateaway = substrings[2];
            return IP;
        }

        private void GetMAC(string tempMAC)
        {

      
            String value = tempMAC;
            Char delimiter = '@';
            String[] substrings = new String[10];
            substrings = value.Split(delimiter);
            foreach(String s in substrings)
            {
                if(s!=""&s!=null)
                {
                    MAC.Add(s);
                }

            }
    
        }

        public void NewConnection(string addr)
        {
            if(connection.Contains(addr)==false)
            {
                connection.Add(addr);
                Number_of_connection++;
                //Console.WriteLine(addr);
            }
        }

        public string getConnection(int i)
        {
            return connection[i];
        }
    }
}
