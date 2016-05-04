using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Logic
{
    class JsonFrame<T>
    {
        public int code;
        public T response;
    }
}
