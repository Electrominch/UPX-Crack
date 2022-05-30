using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralTools
{
    public struct Predict
    {
        public double[] predict;
        public bool win;

        public override string ToString()
        {
            return String.Join(" ", predict) + $" ({(win ? "win" : "lose")})";
        }
    }
}
