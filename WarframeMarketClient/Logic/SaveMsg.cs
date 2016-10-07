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
        

        private List<KeyValuePair<string,Tuple<List<ChatElement>, List<ChatMessage>>>> getMsg() // returns all messages with users who have 1 or more chats
        {

            List<KeyValuePair<string, Tuple<List<ChatElement>, List<ChatMessage>>>> ret = new List<KeyValuePair<string, Tuple<List<ChatElement>, List<ChatMessage>>>>();
            foreach (ChatViewModel chat  in ApplicationState.getInstance().Chats.ToArray())
            {
                if(chat.ChatMessages.Any()||chat.OldChatElements.Any()) ret.Add(new KeyValuePair<string, Tuple<List<ChatElement>, List<ChatMessage>>>( chat.User.Name, new Tuple<List<ChatElement>, List<ChatMessage>>(chat.OldChatElements.ToList(), chat.ChatMessages.ToList())));
            }

            return ret;
        }

        private ChatViewModel GetModelFromKVP(KeyValuePair<string, Tuple<List<ChatElement>, List<ChatMessage>>> chats)
        {
            return new ChatViewModel(new User(chats.Key), chats.Value.Item2, chats.Value.Item1);
        }

        public void SaveMessages() // make save Save (new file => delete old => changeName)
        {
            string json = JsonConvert.SerializeObject(getMsg(), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects
                });
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

            try
            {
                List<KeyValuePair<string, Tuple<List<ChatElement>, List<ChatMessage>>>>
                    chats = JsonConvert.DeserializeObject<List<KeyValuePair<string, Tuple<List<ChatElement>, List<ChatMessage>>>>>(json, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects
                    });
                return chats.Select((x) => GetModelFromKVP(x)).ToList();
            }
            catch(Exception e)
            {
                DeleteMessages();
                return new List<ChatViewModel>();
            }
        }

        public void DeleteMessages()
        {
            if (File.Exists(path)) File.Delete(path);
        }


    }
}
