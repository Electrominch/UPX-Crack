using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPredict
{
    internal class Round
    {
        public readonly double Red = 0;
        public readonly double Green = 0;
        public readonly double Black = 0;
        public readonly Result Result = Result.red;
        public Round(double r, double g, double b, Result res)
        {
            Red = r;
            Green = g;
            Black = b;
            Result = res;
        }

        public override string ToString()
        {
            return $"{Red,-10}\t{Green,-10}\t{Black,-10}\t{Result,-8}";
        }
    }
}
