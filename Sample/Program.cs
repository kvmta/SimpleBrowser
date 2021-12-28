// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace Sample
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.NetworkInformation;
    using System.Threading;
    using SimpleBrowser;

    internal class Program
    {
        private static void Main(string[] args)
        {
            NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(
                (sender, e) => AvailabilityChangedCallback(sender, e));

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void AutoConnectNetwork()
        {
            Browser browser = new Browser();
            try
            {
                // log the browser request/response data to files so we can interrogate them in case of an issue with our scraping
                browser.RequestLogged += OnBrowserRequestLogged;
                browser.MessageLogged += new Action<Browser, string>(OnBrowserMessageLogged);

                // we'll fake the user agent for websites that alter their content for unrecognised browsers
                browser.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36 Edg/96.0.1054.62";

                // browse to link
                browser.Navigate("http://192.168.40.1:8090/httpclient.html");
                if (LastRequestFailed(browser)) return;

                // auto fill input
                browser.Log("Fill ussername / password.");
                browser.Find(id: "username").Value = "khaivq";
                browser.Find(id: "password").Value = "Savis@A123";
                Console.WriteLine("Fill ussername / password.");

                // click the login link and click it
                browser.Log("Click login button.");
                browser.Find(id: "loginbutton").Click();
                Console.WriteLine($"Click login button with result: {LastRequestFailed(browser)}");

            }
            catch (Exception ex)
            {
                browser.Log(ex.Message, LogMessageType.Error);
                browser.Log(ex.StackTrace, LogMessageType.StackTrace);
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }

        static void AvailabilityChangedCallback(object sender, NetworkAvailabilityEventArgs e)
        {
            while (!e.IsAvailable)
            {
                AutoConnectNetwork();
                Thread.Sleep(30000);
            }
        }

        private static bool LastRequestFailed(Browser browser)
        {
            if (browser.LastWebException != null)
            {
                browser.Log("There was an error loading the page: " + browser.LastWebException.Message);
                Console.WriteLine("There was an error loading the page: " + browser.LastWebException.Message);
                return true;
            }
            return false;
        }

        private static void OnBrowserMessageLogged(Browser browser, string log)
        {
            Console.WriteLine(log);
        }

        private static void OnBrowserRequestLogged(Browser req, HttpRequestLog log)
        {
            Console.WriteLine(" -> " + log.Method + " request to " + log.Url);
            Console.WriteLine(" <- Response status code: " + log.ResponseCode);
        }

        private static string WriteFile(string filename, string text)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"));
            if (!dir.Exists)
            {
                dir.Create();
            }

            string path = Path.Combine(dir.FullName, filename);
            File.WriteAllText(path, text);
            return path;
        }
    }
}