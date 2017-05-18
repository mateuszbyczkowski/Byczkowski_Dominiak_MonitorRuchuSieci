using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static bool draw = false;
        public static List<Packets> packet= new List<Packets>();
        


        public Server(int port, bool forwardingStatus = false)
        {
            Listener = new TcpListener(new IPEndPoint(IPAddress.Any, port));
            ForwardingStatus = forwardingStatus;
            Listener.Start();
            WaitForConnection();
        }
        public void WaitForConnection()
        {
          
            //Process.Start("D:\\GIT\\Client\\Client\\bin\\Debug\\Client.exe");
            //Console.WriteLine(@"Uruchomić serwer? t/n");
            //string command = Console.ReadLine();
            //if (command == "t")
               
            //else
            //    ForwardingStatus = false;

            ForwardingStatus = true;
            NumberOfDevices = 1;
            Console.WriteLine("Uruchomiono serwer");


            Console.WriteLine("Oczekiwanie na połączenie użytkownikow.");
            TcpClient client;
            string communique;

            for (int i = 1; i <= NumberOfDevices; i++)
            {

                client = Listener.AcceptTcpClient();
                communique = getCommunique(client);
                Console.WriteLine("Użytkownik "+i+" połączony.");
                user.Add(i, new Users(i, communique, client));


            }

            Task t1 = new Task(new Action(CheckForMessages));
            Task t2 = new Task(new Action(NewUser));
            Task t3 = new Task(new Action(WritePingTable));
            Task t4 = new Task(new Action(DrawGraph));

            t1.Start();
            t2.Start();
            t3.Start();
            t4.Start();
            Task.WaitAny(t1, t2, t3, t4);
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
                Console.WriteLine("Użytkownik " + NumberOfDevices + " połączony.");
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

            Console.WriteLine("Odswieżono adresy IP.");
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
            if (communique[0] == '^')
            {
                communique=communique.Trim(new Char[] {'^'});
                user[nr].NewConnection(communique);
            }
            else if(communique[0]=='*')
            {
                communique = communique.Trim(new Char[] { '*' });
                
                packet.Add(new Packets(communique, packet.Count));
            }
            else if(communique=="@@@@@")
            {
                Console.WriteLine("Klient o adresie " + user[nr].IP + " przesłał przechwycone pakiety");
            }
            else
            {
                user[nr].NewPing(communique);
            }
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

                if (communique == "help")
                {
                    Console.Clear();
                    Console.WriteLine("Dostępne opcje komunikatów: ");
                    Console.WriteLine("    ping - wyświetlenie tablicy pingów połączonych urządzeń");
                    Console.WriteLine("   clear - wyczyszczenie konsoli");
                    Console.WriteLine("  router - wyświetlenie bram domyślnych połączonych urządzeń");
                    Console.WriteLine("   graph - rysowanie grafu według aktualnie posiadanych danych");
                    Console.WriteLine("    exit - wyłączenie programu");
                    Console.WriteLine("    dump - uruchomienie procesu pozyskiwania przechwyconych pakietów przez klientów");
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
                else if (communique == "clear")
                {
                    Console.Clear();
                }
                else if (communique == "router")
                {
                    Console.WriteLine("ADRES BRAMY" + " - " + "ADRES URZĄDZENIA");
                    foreach (KeyValuePair<int, Users> i in user)
                    {
                        Console.WriteLine(i.Value.DefoultGateaway + " - " + i.Value.IP);
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
                else if (communique == "graph")
                {
                    draw = true;
                }
                else if(communique == "dump")
                {
                    foreach (KeyValuePair<int, Users> i in user)
                    {
                        SendCommunique("DUMP", i.Value.client.Tcp);
                    }
                }
                else if (communique == "packets")
                {
                   // List<int> temp = new List<int>();
                    foreach (Packets p in packet)
                    {
                           
                            Console.Write(p.display_information());

                    }
                  
                        
                }
                else if(communique == "filtr")
                {
                    filtr();
                    //TODO filtrowanie z tych samych pakietów

                }
                else
                {
                    Console.WriteLine("----NIEZNANE POLECENIE----");
                    Console.WriteLine("Jeśli chcesz zobaczyć możliwe polecenia wpisz help");
                }


            }
        }

        private static void DrawGraph()
        {
            while (true)
            {
                if (draw)
                {
                    List<string> ToFile = new List<String>();
                    ToFile.Add("graph { ");
                    //ToFile.Add("a--b");
                    foreach (var us in user)
                    {
                        ToFile.Add("\"" + us.Value.DefoultGateaway + "\" " + "[shape = box];");
                        ToFile.Add("\"" + us.Value.IP + "\"" + "--" +"\"" + us.Value.DefoultGateaway+"\"" + ";" + "\n" );
                        for(int i=0;i<us.Value.Number_of_connection;i++)
                        {
                            ToFile.Add("\"" + us.Value.IP + "\"" + "--" + "\"" + us.Value.getConnection(i) + "\"" + ";" + "\n");
                        }

                    }
                    
                    


                    ToFile.Add("}");
                    System.IO.File.WriteAllLines(@"testgraph.dt", ToFile);
                    //const string strCmdText = "/C dot.exe –c&dot.exe –Tjpg –O testgraph.dt";
                    const string strCmdText = "/C dot.exe –c&dot -Tjpg  testgraph.dt -o image.jpg";

                    
                    Process.Start("CMD.exe", strCmdText);
                    Thread.Sleep(1000);
                    Process.Start(@"image.jpg");
                    
                    draw = false;
                }


            }
        }

        private static void filtr()
        {
            List<Packets> p2 = new List <Packets>();
            var unique = packet.Distinct(new MyComparer());
            //Console.WriteLine("filtr");
            
            foreach(var a in unique)
            {
                //Console.WriteLine(a.display_information());
            }
            unique = unique.Distinct(new MyComparer2());
            foreach (var a in unique)
            {
                //Console.WriteLine(a.display_information());
                p2.Add(new Packets(a.Line, p2.Count));
            }
            packet.Clear();
            foreach(Packets p in p2)
            {
                packet.Add(p);
            }
        }

        class MyComparer : IEqualityComparer<Packets>
        {
            public bool Equals(Packets x, Packets y)
            {
                return x.CheckSum.Equals(y.CheckSum);

            }

            public int GetHashCode(Packets obj)
            {
                return obj.CheckSum.GetHashCode();
            }
        }

        class MyComparer2 : IEqualityComparer<Packets>
        {
            public bool Equals(Packets x, Packets y)
            {
                return x.Identification.Equals(y.Identification);

            }

            public int GetHashCode(Packets obj)
            {
                return obj.Identification.GetHashCode();
            }
        }



    }
}
