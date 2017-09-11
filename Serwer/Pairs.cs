using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Serwer
{
    public class Pair
    {
        public string Sour { get; set; }
        public string Dest { get; set; }

        public Pair(string a, string b)
        {
            Sour = a;
            Dest = b;
        }


    }
}
