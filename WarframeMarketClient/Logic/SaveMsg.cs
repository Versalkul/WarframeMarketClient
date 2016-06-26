using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WarframeMarketClient.Model;
using WarframeMarketClient.ViewModel;

namespace WarframeMarketClient.Logic
{
    class SaveMsg
    {
        string path;

        public SaveMsg()
        {
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WarframeMarketClient");

            path =  Path.Combine(folderPath, "Chats");

        }
        

        private List<KeyValuePair<string,List<ChatMessage>>> getMsg() // returns all messages with users who have 1 or more chats
        {

            List<KeyValuePair<string, List<ChatMessage>>> ret = new List<KeyValuePair<string, List<ChatMessage>>>();
            foreach (ChatViewModel chat  in ApplicationState.getInstance().Chats.ToArray())
            {
                if(chat.ChatMessages.Any()) ret.Add(new KeyValuePair<string, List<ChatMessage>>( chat.User.Name, chat.ChatMessages.ToList()));
            }

            return ret;
        }

        private ChatViewModel GetModelFromKVP(KeyValuePair<string, List<ChatMessage>> chats)
        {
            return new ChatViewModel(new User(chats.Key), chats.Value);
        }

        public void SaveMessages() // make save Save (new file => delete old => changeName)
        {
            string json = JsonConvert.SerializeObject(getMsg());
            using (StreamWriter WriteStream = new StreamWriter(new GZipStream(new FileStream(path+"2", FileMode.Create), CompressionLevel.Optimal)))
            {
                
                WriteStream.WriteLine(json);
            }
            if(File.Exists(path))File.Delete(path);
            File.Move(path + "2", path);
            
        }
        public List<ChatViewModel> LoadMessages()
        {
            if (File.Exists(path + 2)) // may lead to problems when crash WHILE saveing 
            {
                if(File.Exists(path))File.Delete(path);
                File.Move(path + "2", path);
            }


            if (!File.Exists(path)) return new List<ChatViewModel>();
            string json;
            using (StreamReader readStream = new StreamReader(new GZipStream(new FileStream(path, FileMode.Open), CompressionMode.Decompress)))
            {
                json = readStream.ReadToEnd();
            }
            List<KeyValuePair<string, List<ChatMessage>>> chats = JsonConvert.DeserializeObject<List<KeyValuePair<string, List<ChatMessage>>>>(json);
            return chats.Select((x) => GetModelFromKVP(x)).ToList();
        }


    }
}
