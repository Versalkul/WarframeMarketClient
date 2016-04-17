using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace WarframeMarketClient.Logic
{
    class RunsGameChecker:IDisposable
    {
       public bool GameOnline {  get; private set; }

        public event EventHandler<AfkStateChangedArgs> afkChanged;
        public event EventHandler<GameStateChangedArgs> gameChanged; 

        private bool isAFK = false;
        public int  afkTimeoutMinutes = 5;

        private Thread worker;
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
            worker = new Thread(checker);
            
        }


        public void Start()
        {
            worker.Start();
        }

        private void checker()
        {
            bool gameRunning = false;
            while (true)
            {
                Thread.Sleep(5000);


                LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
                lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
                lastInputInfo.dwTime = 0;

                if (GetLastInputInfo(out lastInputInfo))
                {
                    TimeSpan afkTime = TimeSpan.FromMilliseconds(Environment.TickCount - lastInputInfo.dwTime);
                    if (afkTime.Minutes > 5 && !isAFK)
                    {
                        isAFK = true;
                        if (afkChanged != null) afkChanged.Invoke(this, new AfkStateChangedArgs(AfkState.Afk));
                    }
                    if (isAFK && afkTime.Minutes < 5)
                    {
                        isAFK = false;
                        if (afkChanged != null) afkChanged.Invoke(this, new AfkStateChangedArgs(AfkState.Available));
                    }
                }


                gameRunning = Process.GetProcessesByName("Warframe.x64").Length > 0 || Process.GetProcessesByName("Warframe").Length > 0;

                if (gameRunning != GameOnline)
                {
                    GameOnline = gameRunning;

                    if (gameChanged != null) gameChanged.Invoke(this, new GameStateChangedArgs(GameOnline ? GameState.running : GameState.notRunning));
                }



            }

        }

        public void Dispose()
        {
            try
            {
                worker.Abort();
            }
            finally
            {
            }
        }
    }
}
