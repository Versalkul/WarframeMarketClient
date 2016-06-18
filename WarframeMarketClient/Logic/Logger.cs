using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WarframeMarketClient.Logic
{
    public class Logger:IDisposable
    {

        string folderPath;
        string filePath;
        Queue<string> log = new Queue<string>();
        Task writer;

        public Logger()
        {
            folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WarframeMarketClient");
            filePath = Path.Combine(folderPath, "Exceptions.log");

            AppDomain.CurrentDomain.FirstChanceException += GotException;

            writer = Task.Factory.StartNew(LogStack);
        }

        private void LogStack()
        {

            while (true)
            {
                lock (this)
                {
                    Monitor.Wait(this);
                }
                List<string> logCopy = new List<string>(log.Count);

                while (log.Any())
                    logCopy.Add(log.Dequeue());

                File.AppendAllLines(filePath, logCopy);
            }

        }

        private void GotException(object o, FirstChanceExceptionEventArgs args)
        {
            string logmsg = "";
            logmsg += "EXCEPTION: " + DateTime.Now.ToString() + " :" + System.Environment.NewLine;
            logmsg += args.Exception.Message + System.Environment.NewLine;
            logmsg += JsonConvert.SerializeObject(args.Exception) + System.Environment.NewLine;
            logmsg += (new StackTrace(true)).ToString() + System.Environment.NewLine;
            
            log.Enqueue(logmsg);
            lock(this) Monitor.Pulse(this); 
        }

        public void Log(string s)
        {
            string logmsg = "";
            logmsg += "EVENT: " + DateTime.Now.ToString() + " :" + System.Environment.NewLine;
            logmsg += s + System.Environment.NewLine;
            logmsg += (new StackTrace(true)).ToString() + System.Environment.NewLine;

            log.Enqueue(logmsg);
            lock (this) Monitor.Pulse(this);
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.FirstChanceException -= GotException;
        }
    }
}
