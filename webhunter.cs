# Thanks to rvrsh3ll for the idea https://github.com/rvrsh3ll/Misc-Powershell-Scripts/blob/master/Find-Fruit.ps1

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace WebHunter
{
    class Program
    {
        static bool verbose = false;
        static string[] RangeToIPs(string range)
        {
            string[] info = range.Split('/');
            long ip = ToInt(info[0]);
            int mask = ~((1 << (32 - Int32.Parse(info[1]))) - 1);

            List<string> ips = new List<string>();

            long current = ip & mask;
            while(current <= ((ip & mask) | ~mask))
            {
                ips.Add(ToAddr(current));
                current++;
            }

            return ips.ToArray();
        }

        static long ToInt(string addr)
        {
            return (long)(uint)IPAddress.NetworkToHostOrder((int)IPAddress.Parse(addr).Address);
        }

        static string ToAddr(long address)
        {
            return IPAddress.Parse(address.ToString()).ToString();
        }

        static void ScanHost(string host, string[] ports)
        {
            foreach(string port in ports)
            {
                foreach(string url in GetUrls())
                {
                    string http = String.Format("http://{0}:{1}/{2}", host, port, url);
                    string https = String.Format("https://{0}:{1}/{2}", host, port, url);
                    if(verbose)
                    {
                        Console.WriteLine("Querying {0}", http);
                    }
                    if(SendRequest(http) == 200)
                    {
                        Console.WriteLine("{0} returned 200", http);
                    }
                    if (verbose)
                    {
                        Console.WriteLine("Querying {0}", https);
                    }
                    if (SendRequest(https) == 200)
                    {
                        Console.WriteLine("{0} returned 200", http);
                    }
                }
            }
        }

        static int SendRequest(string url)
        {
            int responseCode = 404;
            try
            {
                Uri uri = new Uri(url);
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(uri);

                wr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0";
                wr.Timeout = 1000;
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                return Int32.Parse(response.StatusCode.ToString());

            } catch(Exception e)
            {
                if(verbose)
                {
                    Console.WriteLine("Error: {0}", e.Message);
                }
            }
            return responseCode;
        }
        static string[] GetUrls()
        {
            List<string> urls = new List<string>();

            urls.Add("jmx-console/");
            urls.Add("web-console/ServerInfo.jsp");
            urls.Add("invoker/JMXInvokerServlet");
            urls.Add("system/console");
            urls.Add("axis2/axis2-admin/");
            urls.Add("manager/html/");
            urls.Add("tomcat/manager/html/");
            urls.Add("wp-admin/");
            urls.Add("workorder/FileDownload.jsp");
            urls.Add("ibm/console/logon.jsp?action=OK");
            urls.Add("data/login");
            urls.Add("script/");
            urls.Add("opennms/");
            urls.Add("RDWeb/Pages/en-US/Default.aspx");

            return urls.ToArray();
        }

        static void Main(string[] args)
        {
            string[] ips = RangeToIPs(args[0]);
            string[] ports = args[1].Split(',');
            if(Array.Exists(args, match => match.ToLower() == "-verbose"))
            {
                verbose = true;
            }

            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            foreach(string ip in ips)
            {
                Console.WriteLine("Querying {0}", ip);
                ScanHost(ip, ports);

            }
            Thread.Sleep(10000);
        }
    }
}
