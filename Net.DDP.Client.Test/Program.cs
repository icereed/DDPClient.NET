﻿using System;
using System.Collections.Generic;

namespace Net.DDP.Client.Test
{
    static class Program
    {
        static void Main(string[] args)
        {
            IDdpConnector connector = new DdpConnector();

            connector.OnConnecting += (sender, eventArgs) => Console.WriteLine("--- CONNECTING ---");
            connector.OnError += (sender, eventArgs) => Console.WriteLine("--- ERROR: "+eventArgs.Message+" ---");
            connector.OnOpen += (sender, eventArgs) => Console.WriteLine("--- OPEN ---");
            connector.OnClosed += (sender, eventArgs) => Console.WriteLine("--- CLOSED ---");

            // Testing by listing all atmosphere packages
            DDPClient client = new DDPClient(new Subscriber(),connector);
            client.Connect("atmosphere.meteor.com:443");
            client.Subscribe("packages");
            Console.ReadLine();
            connector.Close();
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
