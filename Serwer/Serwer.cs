using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
        private static int NumberOfDevices;
        public static Dictionary<int, Users> user = new Dictionary<int, Users>();
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

           // string a = GetDefaultGateway().ToString();
           //Console.WriteLine();



            Console.WriteLine("Oczekiwanie na połączenie użytkownikow");
            TcpClient client;
            string communique;

            for (int i = 1; i <= NumberOfDevices; i++)
            {

                client = Listener.AcceptTcpClient();
                communique = getCommunique(client);
                Console.WriteLine( i + " użytkownik połączony");
                user.Add(i, new Users(i, communique, client));
                

            }

            Task t1 = new Task(new Action(CheckForMessages));
            Task t2 = new Task(new Action(NewUser));
            Task t3 = new Task(new Action(WritePingTable));


            t1.Start();
            t2.Start();
            t3.Start();
            Task.WaitAny(t1, t2,t3);
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
                Console.WriteLine("Użytkownik " + NumberOfDevices + " połączony");
                user.Add(NumberOfDevices, new Users(NumberOfDevices, communique, client));
                Thread.Sleep(200);
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
              
                SendCommunique("#####", i.Value.client.Tcp);
            }

            Console.WriteLine("Odswieżono adresy IP");
        }
        public void CheckForMessages()
        {
            int n = NumberOfDevices;
            while (true)
            {

               table = "";

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
           
            user[nr].NewPing(communique);
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


        public static void WritePingTable()
        {
            while (true)
            {
                //Console.WriteLine("Podaj komunikat");
                string communique = Console.ReadLine();

                if(communique=="help")
                {
                    Console.Clear();
                    Console.WriteLine("Dostępne opcje komunikatów: ");
                    Console.WriteLine("    ping - wyświetlenie tablicy pingów połączonych urządzeń");
                    Console.WriteLine("   clear - wyczyszczenie konsoli");
                    Console.WriteLine("  router - wyświetlenie bram domyślnych połączonych urządzeń");
                    Console.WriteLine("    exit - wyłączenie programu");
                }
                else if (communique == "ping")
                {
                    foreach (KeyValuePair<int, Users> i in user)
                    {

                        Console.WriteLine("Wynik pingowania do urządzeń z klienta nr " + i.Value.nr + ":");
                        foreach (KeyValuePair<string, bool> j in user[i.Value.nr].ping)
                        {
                            Console.WriteLine(j.Key + ": " + j.Value);

                            //Thread.Sleep(100);
                        }
                    }
                }
                else if(communique == "clear")
                {
                    Console.Clear();
                }
                else if(communique == "router")
                {
                    Console.WriteLine("ADRES BRAMY"  + " - " + "ADRES URZĄDZENIA");
                    foreach (KeyValuePair<int, Users> i in user)
                    {
                        Console.WriteLine( i.Value.DefoultGateaway + " - " + i.Value.IP );
                    }
                }
                else if (communique == "exit")
                {
                    foreach (KeyValuePair<int, Users> i in user)
                    {
                         SendCommunique("EXIT", i.Value.client.Tcp);
                    }
                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                }
                else
                {
                    Console.WriteLine("----NIEZNANE POLECENIE----");
                    Console.WriteLine("Jeśli chcesz zobaczyć możliwe polecenia wpisz help");
                }
                

            }
        }

        public static IPAddress GetDefaultGateway()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .SelectMany(n => n.GetIPProperties()?.GatewayAddresses)
                .Select(g => g?.Address)
                .FirstOrDefault(a => a != null);
        }

      
    }
}
