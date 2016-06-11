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
    class Logger
    {

        string folderPath;
        string filePath;

        public Logger()
        {
            folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WarframeMarketClient");
            filePath = Path.Combine(folderPath, "Exceptions.log");

            AppDomain.CurrentDomain.FirstChanceException += GotException;
        }


        public void GotException(object o, FirstChanceExceptionEventArgs args)
        {

            List<string> text = new List<string>();

            text.Add(DateTime.Now.ToString() + " :");
            text.Add(args.Exception.Message);
            text.Add(args.Exception.StackTrace);
            text.Add(args.Exception.ToString());
            text.Add(args.Exception.InnerException.ToString());
            // targetSite


            File.AppendAllLines(filePath,text);

        }
    }
}
