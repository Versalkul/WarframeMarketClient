using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Logic
{
    public class Logger
    {

        string folderPath;
        string filePath;

        public Logger()
        {
            folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WarframeMarketClient");
            filePath = Path.Combine(folderPath, "Exceptions.log");

            AppDomain.CurrentDomain.FirstChanceException += GotException;
        }


        private void GotException(object o, FirstChanceExceptionEventArgs args)
        {

            List<string> text = new List<string>();

            text.Add("EXCEPTION: "+DateTime.Now.ToString() + " :");
            text.Add(args.Exception.Message);
            text.Add(JsonConvert.SerializeObject(args.Exception));
            // targetSite
            text.Add((new StackTrace(true)).ToString());

            File.AppendAllLines(filePath,text);
        }

        public void Log(string s)
        {

            List<string> text = new List<string>();

            text.Add("EVENT: " + DateTime.Now.ToString() + " :");
            text.Add(s);
            text.Add((new StackTrace(true)).ToString());

            File.AppendAllLines(filePath, text);
        }
    }
}
