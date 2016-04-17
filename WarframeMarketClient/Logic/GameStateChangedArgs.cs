using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Logic
{

    public enum GameState {running,notRunning}

    class GameStateChangedArgs:EventArgs
    {

        public GameState newGamestate;

        public GameStateChangedArgs(GameState newState)
        {
            newGamestate = newState;
        }

    }
}
