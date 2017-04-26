using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Serwer
{

    class Server
    {
        public bool ForwardingStatus { get; private set; }
        public TcpListener Listener { get; private set; }
        public Client ClientOne { get; private set; }
        public Client ClientTwo { get; private set; }

        private static int NumberOfDevices;

        public static Dictionary<int, Users> user = new Dictionary<int, Users>();

        private static System.Timers.Timer aTimer;
        public static string table = "";

        public Server(int port, bool forwardingStatus = false)
        {
            Listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            ForwardingStatus = forwardingStatus;
            Listener.Start();
            WaitForConnection();
        }
        public void WaitForConnection()
        {
            Console.WriteLine(@"Uruchomić serwer? t/n");
            string command = Console.ReadLine();
            if (command == "t")
                ForwardingStatus = true;
            else
                ForwardingStatus = false;


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

            Task t1 = new Task(new Action(CheckForMessages));
            Task t2 = new Task(new Action(NewUser));
            //Task t3 = new Task(new Action(SendIP));


            t1.Start();
            t2.Start();
            //t3.Start();
            Task.WaitAny(t1, t2);
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
                SendIPaddr();
            }
        }

        public void SendIPaddr()
        {
            foreach (KeyValuePair<int, Users> i in user)
            {
                foreach (KeyValuePair<int, Users> j in user)
                {
                    SendCommunique(j.Value.IP, i.Value.client.Tcp);

                    Thread.Sleep(300);
                }
            }

            Console.WriteLine("Odswiezono adresy IP");
        }
        public void CheckForMessages()
        {
            int n = NumberOfDevices;
            while (true)
            {

                table = "";
                //foreach (KeyValuePair<int, Users> kvp in user)
                //{
                //    table=table+kvp.Value.IP+"$";
                //}



                for (int i = 1; i <= n; i++)
                {
                    if (user[i].client.Tcp.Available > 0)
                    {

                        string response = getCommunique(user[i].client.Tcp);
                        CheckCommunicat(response, user[i].nr);

                    }
                }
                if (n != NumberOfDevices)
                {
                    n = NumberOfDevices;
                    Thread.Sleep(200);
                }



            }
        }



        public void CheckCommunicat(string communique, int nr)
        {
            Console.WriteLine("Odebrany komunikat  od użytkownika " + nr.ToString() + " o tresci " + communique);
        }

        private string getCommunique(TcpClient tcpClient)
        {
            Thread.Sleep(200);
            BinaryReader reader = new BinaryReader(tcpClient.GetStream());
            string message = reader.ReadString();
            string data = Convert.ToString(message);
            return data;
        }

        public static void SendCommunique(string communique, TcpClient tcpClient)
        {
            try
            {
                BinaryWriter writer = new BinaryWriter(tcpClient.GetStream());

                writer.Write(Convert.ToString(communique));
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }
    }
}
