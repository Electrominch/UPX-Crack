using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralTools
{
    public struct Stat
    {
        public int win;
        public int lose;
        public int Games => win + lose;

        public override string ToString()
        {
            return $"win:{win} lose:{lose}";
        }
    }
}
