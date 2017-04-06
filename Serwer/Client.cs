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
    class Client
    {
        public TcpClient Tcp { get; private set; }
        public string Number { get; set; }
        public Client(TcpClient tcp, string number)
        {
            Tcp = tcp;
            Number = number;
        }
    }
}
