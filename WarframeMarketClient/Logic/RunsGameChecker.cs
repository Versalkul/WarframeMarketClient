using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using WarframeMarketClient.Model;
using System.Linq;

namespace WarframeMarketClient.Logic
{
    public class RunsGameChecker:IDisposable
    {
       public bool GameOnline {  get; private set; }

        private bool isAFK = false;
        public int AfkTimeoutMinutes { get; set; } = 4;

        private Timer worker;
        DateTime lastMovedMouse = DateTime.Now;

        #region pinvoke

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(out LASTINPUTINFO lipi);

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public static readonly int SizeOf =
                   Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int dwTime;
        }

        #endregion

        public RunsGameChecker()
        {
            worker = new Timer();
            worker.Elapsed += new ElapsedEventHandler(checker);
            worker.AutoReset = true;
            worker.Interval = 5000;
            worker.Start();
            checker(null, null);

        }

        public void changeRunning()
        {
            worker.Enabled = !worker.Enabled;

            if (!worker.Enabled)
            {
                ApplicationState.getInstance().Market.setOffline();
                ApplicationState.getInstance().OnlineState = OnlineState.DISABLED;
            }
            else
            {
                ApplicationState.getInstance().OnlineState = ApplicationState.getInstance().DefaultState;
                checker(null, null);
            }
        }


        private void checker(object o,EventArgs args)
        {


            Process[] GameX64 = Process.GetProcessesByName("Warframe.x64");
            Process[] GameX86 = Process.GetProcessesByName("Warframe");
            GameOnline = GameX64.Where(g => g.MainWindowTitle.ToLower().Equals("warframe")).Any() || GameX86.Where(g => g.MainWindowTitle.ToLower().Equals("warframe")).Any();
            if (!isAFK&& GameOnline && ApplicationState.getInstance().OnlineState != OnlineState.INGAME) ApplicationState.getInstance().OnlineState = OnlineState.INGAME;
            if(!isAFK && !GameOnline && ApplicationState.getInstance().OnlineState == OnlineState.INGAME) ApplicationState.getInstance().OnlineState = ApplicationState.getInstance().DefaultState;
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            if (GetLastInputInfo(out lastInputInfo))
            {
                TimeSpan afkTime = TimeSpan.FromMilliseconds(Environment.TickCount - lastInputInfo.dwTime); 
                if (afkTime.Minutes > AfkTimeoutMinutes && !isAFK)
                {
                    isAFK = true;
                    ApplicationState.getInstance().OnlineState = OnlineState.OFFLINE;
                }
                if (isAFK && afkTime.Minutes <= AfkTimeoutMinutes)
                {
                    isAFK = false;
                    ApplicationState.getInstance().OnlineState = GameOnline?OnlineState.INGAME : ApplicationState.getInstance().DefaultState;
                }
            }


        }

        public void Dispose()
        {
            try
            {
                worker.Dispose();
            }
            finally
            {
            }
        }
    }
}
