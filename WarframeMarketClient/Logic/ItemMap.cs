using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Logic
{
    class ItemMap
    {
        static Dictionary<string, Tuple<string, int>> itemInfoMap = new Dictionary<string, Tuple<string, int>>(1000);
        public static bool initialized = false;

        static string path= Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WarframeMarketClient"), "ItemMap");

        public static void SaveMap()
        {    
            string json = JsonConvert.SerializeObject(itemInfoMap);
            using (StreamWriter WriteStream = new StreamWriter(new GZipStream(new FileStream(path, FileMode.Create), CompressionLevel.Optimal)))
            {
                WriteStream.WriteLine(json);
            }
        }

        public static void LoadMap()
        {
            if (!File.Exists(path)) return;
            string json;
            using (StreamReader readStream = new StreamReader(new GZipStream(new FileStream(path, FileMode.Open), CompressionMode.Decompress)))
            {
                json = readStream.ReadToEnd();
            }
            itemInfoMap = JsonConvert.DeserializeObject<Dictionary<string, Tuple<string, int>>>(json);
        }

        public static void getTypeMap()
        {
            Dictionary<string, Tuple<string, int>> map = new Dictionary<string, Tuple<string, int>>(1000);
            using (HttpWebResponse response = Webhelper.GetPage("http://warframe.market/api/get_all_items_v2"))
            {
                if (response == null || response.StatusCode != HttpStatusCode.OK) return;

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {

                    List<ItemTypeMap> mapList = JsonConvert.DeserializeObject<List<ItemTypeMap>>(reader.ReadToEnd());

                    foreach (ItemTypeMap elem in mapList)
                    {
                        map.Add(elem.item_name, new Tuple<string, int>(elem.item_type, elem.mod_max_rank));
                    }
                }


            }
            itemInfoMap = map;
            initialized = true;
        }

        public static string GetCategory(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) return "";
            if (itemInfoMap.ContainsKey(name)) return itemInfoMap[name].Item1;
            return "";
        }

        public static bool IsValidItemName(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) return false;
            if (itemInfoMap.ContainsKey(name)) return true;
            return false;
        }
        public static int GetMaxModRank(string name)
        {
            if (String.IsNullOrWhiteSpace(name)) return -1;
            if (itemInfoMap.ContainsKey(name)) return itemInfoMap[name].Item2;
            return -1;
        }

        public static List<string> GetValidItemNames()
        {
            return itemInfoMap.Keys.ToList();
        }

    }
}
