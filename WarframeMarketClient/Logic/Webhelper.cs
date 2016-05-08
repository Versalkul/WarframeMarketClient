using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.Logic
{
    class Webhelper
    {
        static string csrfToken="";

        public static HttpWebResponse GetPage (string url)
        {
            
            for(int i = 0; i < 10; i++)
            {

                HttpWebRequest page = HttpWebRequest.CreateHttp(url);
                if (ApplicationState.getInstance().SessionToken != null)
                {
                    page.CookieContainer = new CookieContainer();
                    page.CookieContainer.Add(new Uri("https://warframe.market"), new Cookie("session", ApplicationState.getInstance().SessionToken));
                }

                page.Timeout = 10000;
                page.UserAgent = "C# Warframe Market Client";
                page.AllowAutoRedirect = false;
                page.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                HttpWebResponse response=null;
           
                try
                {
                    response= (HttpWebResponse)page.GetResponse();
                    return response;

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR GETTING RESPONSE from {url} Error is:" + ex.Message);

                    Thread.Sleep(10000);
                    if (response != null)
                    {
                        response.Close();
                    }
                }
            
            }
            return null;

        }



        public static HttpWebResponse PostPage(string url,string postData)
        {
            Console.WriteLine("Posting " +postData);
            if (csrfToken.Length < 5)
            {
                getCsrfToken();
            }

            byte[] data = Encoding.UTF8.GetBytes(postData);
            for (int i = 0; i < 10; i++)
            {

                HttpWebRequest page = HttpWebRequest.CreateHttp(url);
                page.CookieContainer = new CookieContainer();
                page.CookieContainer.Add(new Uri("https://warframe.market"), new Cookie("session", ApplicationState.getInstance().SessionToken));
                page.UserAgent = "C# Warframe Market Client";
                page.Method = "POST";
                page.ContentType = "application/x-www-form-urlencoded";
                page.ContentLength = data.Length;
                page.Headers["x-csrf-token"] = csrfToken;
                page.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                try
                {

                using (var stream = page.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                return (HttpWebResponse)page.GetResponse();

                }
                catch(Exception ex)
                {
                    getCsrfToken();
                    Thread.Sleep(10000);



                }
            }
            return null;
        }

        private static void getCsrfToken()
        {
            Console.WriteLine("CSRF Token was:"+csrfToken );
            using (HttpWebResponse response = GetPage("http://warframe.market/orders"))
            {

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {


                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line.Contains("csrf-token"))
                        {
                            string[] parts = line.Split('\"');
                            csrfToken = parts[parts.Length - 2];
                            break;
                        }


                    }


                }
            }
            Console.WriteLine("CSRF Token is:" + csrfToken);

        }

    }
}
