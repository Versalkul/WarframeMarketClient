using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Logic
{
    class ItemMap
    {

        string path;

        public ItemMap()
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WarframeMarketClient");

            path = Path.Combine(folderPath, "Chats");
        }

    }
}
