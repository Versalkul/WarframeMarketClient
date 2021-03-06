﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
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

            Console.WriteLine("Fetching " +url);

            for(int i = 0; i < 10; i++)
            {

                HttpWebRequest page = HttpWebRequest.CreateHttp(url);
                page.CookieContainer = new CookieContainer();
                if (ApplicationState.getInstance().SessionToken.Length<page.CookieContainer.MaxCookieSize&& ApplicationState.getInstance().SessionToken.Length>10&&url.ToLower().Contains("http://warframe.market/"))  // being sure just the Warframe market site gets the cookie 
                {
                    page.CookieContainer.Add(new Uri("https://warframe.market"), new Cookie("session", ApplicationState.getInstance().SessionToken));
                }

                page.Timeout = 20000;
                page.UserAgent = "C# Warframe Market Client";
                page.AllowAutoRedirect = false;
                page.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                HttpWebResponse response=null;
           
                try
                {
                    response= (HttpWebResponse)page.GetResponse();
                    return response;

                }
                catch (WebException e)
                {

                    if (e == null) continue;
                    if (((HttpWebResponse)e.Response) == null) continue; // This is c# saying you got a timeout -.-'
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotFound) // improve this
                    {
                        return null;
                    }

                    Thread.Sleep(1000);
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
            if (ApplicationState.getInstance().SessionToken.Length<10) return null;
            if (String.IsNullOrWhiteSpace(ApplicationState.getInstance().SessionToken)) return null;

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
                page.Timeout = 20000;
                try
                {

                using (var stream = page.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
                    HttpWebResponse response = (HttpWebResponse)page.GetResponse();
                return response;

                }
                catch (WebException e)
                {

                    if (e == null) continue;
                    if (((HttpWebResponse)e.Response) == null) continue; // This is c# saying you got a timeout -.-'
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.BadRequest)
                    {
                        
                        getCsrfToken();
                        continue;
                    }
                    
                    if (e.Response != null) return (HttpWebResponse) e.Response;
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

        public static Version CheckUpdate()
        {
#if DEBUG // not checking updates when debugging
            return null;      
#endif
            using (HttpWebResponse response = Webhelper.GetPage("https://api.github.com/repos/Versalkul/WarframeMarketClient/releases/latest"))
            {
                if (response == null) return null;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    JObject json = JObject.Parse(reader.ReadToEnd());
                    JToken vNum = json.GetValue("tag_name");
                    if (vNum == null) return null;
                    try // incase the tag name contains bullshit
                    {
                        Version newVersion = new Version(vNum.ToString());
                        return newVersion;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

    }
}
