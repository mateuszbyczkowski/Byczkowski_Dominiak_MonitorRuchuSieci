using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Serwer
{
    
    class Users
    {
        public int nr { get; set; }
        public string IP { get; set; }
        public Client client { get; set; }

        public Users(int new_nr, string new_ip, TcpClient new_tcp)
        {
            nr = new_nr;
            IP = new_ip;
            client = new Client(new_tcp, "1");
        } 
    }
}
