using System;
using System.Collections.Generic;

namespace Net.DDP.Client.Test
{
    static class Program
    {
        static void Main(string[] args)
        {

            DDPClient client;
            using (client = new DDPClient(new Subscriber()))
            {
                // Events
                client.StateTracker.OnConnecting += (sender, eventArgs) => Console.WriteLine("--- CONNECTING ---");
                client.StateTracker.OnError += (sender, eventArgs) => Console.WriteLine("--- ERROR: " + eventArgs.Message + " ---");
                client.StateTracker.OnOpen += (sender, eventArgs) => Console.WriteLine("--- OPEN ---");
                client.StateTracker.OnClosed += (sender, eventArgs) => Console.WriteLine("--- CLOSED ---");

                // Testing by listing all atmosphere packages
                client.ConnectWithSsl("atmosphere.meteor.com:443");
                client.Subscribe("packages");
                Console.ReadLine();
                client.Close();
                Console.ReadLine();
            }
            Console.WriteLine("--- DISPOSED ---");
            Console.ReadLine();

        }
    }

    public class Subscriber : IDataSubscriber
    {
        // Atmosphere packages list
        private readonly List<string> _packages = new List<string>();
        public string Session { get; set; }
        public void DataReceived(dynamic data)
        {
            try
            {
                // Handling connection to server
                if (data.Type == DDPType.Connected)
                {
                    Session = data.Session;
                    Console.WriteLine("Connected! Session id: " + Session);
                }
                else if (data.Type == DDPType.Added) // Handling added event
                {
                    _packages.Add(data.Name);
                    Console.Write(data.Name + ", ");
                }
                else if (data.Type == DDPType.Changed) // Handling added event
                {
                    Console.WriteLine("Package " + data.Name + " was modified");
                }
                else if (data.Type == DDPType.Error)
                {
                    Console.WriteLine("Error: " + data.Error);
                }
                else if (data.Type == DDPType.Ready)
                {
                    Console.WriteLine("Collections " + data.RequestsIds[0] + " loaded!");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error trying to parse data");
            }
        }
    }
}
