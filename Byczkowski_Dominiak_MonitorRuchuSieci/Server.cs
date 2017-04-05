﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Byczkowski_Dominiak_MonitorRuchuSieci
{
    class Server
    {
        public bool ForwardingStatus { get; private set; }
        public TcpListener Listener { get; private set; }

       private int NumberOfDevices;

       private Dictionary<int, Users> user = new Dictionary<int, Users>();

        public Server(int port, bool forwardingStatus = false)
        {
            Listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            ForwardingStatus = forwardingStatus;
            Listener.Start();
            // WaitForConnection();
        }
        public void WaitForConnection()
        {
            //  Console.WriteLine(@"Uruchomić serwer? t/n");
            //  string command = Console.ReadLine();
            //  if (command == "t")
            //       ForwardingStatus = true;
            //   else
            //       ForwardingStatus = false;


            NumberOfDevices = 1;

            Console.WriteLine("Oczekiwanie na połączenie użytkownikow");
            TcpClient client;
            string communique;

            for (int i = 1; i <= NumberOfDevices; i++)
            {

                client = Listener.AcceptTcpClient();
                communique = getCommunique(client);
                Console.WriteLine("Użytkownik " + i + " o adresie " + communique + " połączony");
                user.Add(i, new Users(i, communique, client));

            }

            /*
            client = Listener.AcceptTcpClient();
            communique = getCommunique(client);
            Console.WriteLine("Użytkownik o adresie "+communique + " połączony");
            //string number = getNumerFromBinary(communique);
            ClientOne = new Client(client, "1");
            //Console.WriteLine("Uzytkownik o numerze {0} podlaczyl sie", number);



            //TODO tablica użytkownikow i ich IP


            Console.WriteLine("Oczekiwanie na drugiego  uzytkownika.");
            client = Listener.AcceptTcpClient();
            communique = getCommunique(client);
            // number = getNumerFromBinary(communique);
            ClientTwo = new Client(client, "2");
            Console.WriteLine("Uzytkownik o numerze 2 podlaczyl sie");
            */

            Task t1 = new Task(new Action(CheckForMessages));
            Task t2 = new Task(new Action(NewUser));

            t1.Start();
            t2.Start();
            Task.WaitAny(t1, t2);
            //t1.Wait();        
        }
        public void NewUser()
        {
            while (true)
            {
                TcpClient client;
                string communique;

                client = Listener.AcceptTcpClient();
                communique = getCommunique(client);
                NumberOfDevices++;
                Console.WriteLine("Użytkownik " + NumberOfDevices + " o adresie " + communique + " połączony");
                user.Add(NumberOfDevices, new Users(NumberOfDevices, communique, client));
            }
        }
        public void CheckForMessages()
        {
            int n = NumberOfDevices;
            while (true)
            {



                for (int i = 1; i <= n; i++)
                {
                    if (user[i].client.Tcp.Available > 0)
                    {
                        //Console.WriteLine("Odbieram dane od uzytkownika {0}", i);
                        string response = getCommunique(user[i].client.Tcp);
                        CheckCommunicat(response, user[i].client);

                    }
                }
                if (n != NumberOfDevices)
                {
                    n = NumberOfDevices;
                    Thread.Sleep(200);
                }

            }
        }
        public void CheckCommunicat(string communique, Client Client)
        {
            Console.WriteLine("Odebrany komunikat  od użytkownika " + Client.Number + " o tresci " + communique);
        }

        private string getCommunique(TcpClient tcpClient)
        {
            Thread.Sleep(200);
            BinaryReader reader = new BinaryReader(tcpClient.GetStream());
            string message = reader.ReadString();
            string data = Convert.ToString(message);
            // data = data.PadLeft(8, '0');
            return data;
        }
    }
}
