using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.Logic
{
    class HtmlParser
    {

        public static Tuple<bool,string> verifyToken() // worked and username
        {
            using (HttpWebResponse response = Webhelper.GetPage("http://warframe.market/account"))
            {

                if (response == null || ((HttpWebResponse)response).StatusCode != HttpStatusCode.OK) return new Tuple<bool, string>(false,"");
                MarketManager.timeOffset = DateTime.Now - DateTimeOffset.Parse(response.Headers["Date"]).DateTime;

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    while (!reader.EndOfStream)
                    {

                        string s = reader.ReadLine();
                        if (s.Contains("waiting for your message ...")) // dirty hack for getting the username (semi-parsing the html)
                        {
                            s = s.Substring(s.IndexOf("value") + 7);

                            return new Tuple<bool, string>(true, s.Substring(0, s.IndexOf('"')));
                        }

                    }
                }
            }

            return new Tuple<bool, string>(false,"");
        }

        public static List<WarframeItem> getOffers()
        {
            using (HttpWebResponse response = Webhelper.GetPage("http://warframe.market/orders"))
            {
                if (response == null) return new List<WarframeItem>();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {

                    bool sell = true;
                    bool ready = false;
                    string name = "";
                    int price = 0;
                    int count = 0;
                    int modrank = -1;
                    int stepcount = 0;
                    string id = "";
                    List<WarframeItem> offers = new List<WarframeItem>(50);


                    while (!reader.EndOfStream)
                    {
                        if (!ready)
                        {
                            if (reader.ReadLine().Contains("Sell orders")) ready = true; // first want to sell stuff
                            continue;
                        }
                        string line = reader.ReadLine();
                        if (line.Contains("Buy orders")) // after that want to buy 
                        {
                            sell = false;
                            continue;
                        }

                        if (line.Contains("<tr data-id="))
                        {
                            stepcount++;
                            id = line.Replace("<tr data-id=\"", "").Replace("\">", "").Trim();
                        }

                        else if (line.Contains("<td>") && line.Contains("</td>")) // got name info step=>2
                        {
                            stepcount++;
                            name = line.Replace("<td>", "").Replace("</td>", "").Trim();

                        }
                        else if (line.Contains("<span>") && line.Contains("</span>")) //step=> 3-5
                        {
                            line = line.Replace("<span>", "").Replace("</span>", "").Trim();
                            stepcount++;
                            switch (stepcount)
                            {
                                case 3:
                                    price = Convert.ToInt32(line);
                                    break;
                                case 4:
                                    count = Convert.ToInt32(line);

                                    break;
                                case 5:
                                    if (line.Length < 1) modrank = -1;
                                    else modrank = Convert.ToInt32(line);
                                    stepcount = 0;
                                    offers.Add(new WarframeItem(name, price, count, modrank, sell, id));
                                    break;

                                default:
                                    break;
                            }
                        }

                    }
                    return offers;
                }
            }
        }


        public static List<string> getChatUser()
        {
            using (HttpWebResponse response = Webhelper.GetPage("http://warframe.market/messages"))
            {
                if (response == null) return new List<string>();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {

                    List<string> user = new List<string>();
                    bool ready = false;
                    while (!reader.EndOfStream)
                    {

                        string line = reader.ReadLine();
                        if (line.Contains("List of users.") && !ready)
                        {
                            ready = true;
                            continue;
                        }
                        if (!ready) continue;
                        if (line.Contains("<li data-name=") && !line.Contains("ingame_name"))
                        {
                            user.Add(line.Split('"')[1]);
                        }

                        if (line.Contains("</div>")) return user;

                    }
                    return user;

                }

            }
        }

    }
}
