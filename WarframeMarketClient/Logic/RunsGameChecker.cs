using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using WarframeMarketClient.Model;

namespace WarframeMarketClient.Logic
{
    class RunsGameChecker:IDisposable
    {
       public bool GameOnline {  get; private set; }

        private bool isAFK = false;
        public int  afkTimeoutMinutes = 5;

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

        }



        private void checker(object o,EventArgs args)
        {

                GameOnline = Process.GetProcessesByName("Warframe.x64").Length > 0 || Process.GetProcessesByName("Warframe").Length > 0;
            if (!isAFK&& GameOnline && ApplicationState.getInstance().OnlineState != OnlineState.INGAME)
                ApplicationState.getInstance().OnlineState = OnlineState.INGAME;


            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            if (GetLastInputInfo(out lastInputInfo))
            {
                TimeSpan afkTime = TimeSpan.FromMilliseconds(Environment.TickCount - lastInputInfo.dwTime);
                if (afkTime.Minutes > 5 && !isAFK)
                {
                    isAFK = true;
                    ApplicationState.getInstance().OnlineState = OnlineState.OFFLINE;
                }
                if (isAFK && afkTime.Minutes < 5)
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
