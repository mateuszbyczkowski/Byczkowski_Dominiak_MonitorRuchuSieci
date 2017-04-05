using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;


namespace Byczkowski_Dominiak_MonitorRuchuSieci
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : Page
    {
        public bool ForwardingStatus { get; private set; }
        public TcpListener Listener { get; private set; }

        private int NumberOfDevices;

        private Dictionary<int, Users> user = new Dictionary<int, Users>();

        public Menu()
        {
            InitializeComponent();
        
            Listener = new TcpListener(new IPEndPoint(IPAddress.Any, 1000));
            ForwardingStatus = false;
            Listener.Start();

        }

      

        private void Starts_button_Click(object sender, RoutedEventArgs e)
        {

            ForwardingStatus = true;
            NumberOfDevices = 1;
            messages.Text= "Oczekiwanie na połączenie użytkownikow";
            
            waiting();

        }
            
        private void waiting()
        {
            TcpClient client;
            string communique;

            for (int i = 1; i <= NumberOfDevices; i++)
            {

                client = Listener.AcceptTcpClient();
                communique = getCommunique(client);
                messages.Text = messages.Text + "\n" + "Użytkownik " + i + " o adresie " + communique + " połączony";
               
                user.Add(i, new Users(i, communique, client));

            }

            Task t1 = new Task(new Action(CheckForMessages));
            Task t2 = new Task(new Action(NewUser));

            t1.Start();
            t2.Start();
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
                messages.Text = messages.Text +"\n" + "Użytkownik " + NumberOfDevices + " o adresie " + communique + " połączony";
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
        public void CheckCommunicat(string communique, int user_nr)
        {
            messages.Text = messages.Text + "\n" + "Odebrany komunikat  od użytkownika " + user_nr + " o tresci " + communique;
        }

        private string getCommunique(TcpClient tcpClient)
        {
            Thread.Sleep(200);
            BinaryReader reader = new BinaryReader(tcpClient.GetStream());
            string message = reader.ReadString();
            string data = Convert.ToString(message);
            return data;
        }





    }
}
