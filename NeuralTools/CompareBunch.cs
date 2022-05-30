using NeuralTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuralTools
{
    public class CompareBunch
    {
        public double[] Predicts { get; private set; }
        public double[] Expected { get; private set; }
        public double[] Input { get; private set; }
        public bool Win { get; private set; }

        public CompareBunch(double[] pr, Round r = null)
        {
            Predicts = pr;
            Expected = GetExpected(r);
            Input = Predicts.Concat(Expected).ToArray();
            Win = Predicts.ToList().IndexOf(Predicts.Max()) == Expected.ToList().IndexOf(Expected.Max());
        }

        private double[] GetExpected(Round r)
        {
            List<double> ds = new List<double>();
            ds.Add(r.Result == Result.red ? 0.9 : 0.1);
            ds.Add(r.Result == Result.green ? 0.9 : 0.1);
            ds.Add(r.Result == Result.black ? 0.9 : 0.1);
            return ds.ToArray();
        }
    }
}
