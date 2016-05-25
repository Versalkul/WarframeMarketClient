using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarframeMarketClient.Extensions
{
     static class ListFunctions
    {
        public static bool EqualContent<T>(this List<T> me,List<T> other)
        {

            if (me.Count != other.Count) return false;
            for(int i = 0; i < me.Count; i++)
            {
                if (!me[i].Equals(other[i])) return false;
            }
            return true;

        }

        public static bool EqualContent<T>(this Collection<T> me, Collection<T> other)
        {

            if (me.Count != other.Count) return false;
            for (int i = 0; i < me.Count; i++)
            {
                if (!me[i].Equals(other[i])) return false;
            }
            return true;
        }


    }
}
