using NeuralTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComparedLearn
{
    internal class CompareLearnSet
    {
        public double[] Input { get; private set; }
        public double[] Expected { get; private set; }
        public CompareLearnSet(List<CompareBunch> prevPredicts, CompareBunch curBunch)
        {
            List<double> inp = new List<double>();
            foreach(var pp in prevPredicts)
                inp.AddRange(pp.Input);
            inp.AddRange(curBunch.Predicts);
            Input = inp.ToArray();
            Expected = curBunch.Expected;
        }
    }
}
