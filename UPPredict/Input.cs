using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPPredict
{
    internal class Input
    {
        private Round[] _prev;
        private Round _last;
        public double[] In { get; private set; }

        public Input(List<Round> rs, Round cur)
        {
            _prev = rs.ToArray();
            _last = cur;
            In = GetInput();
        }

        private double[] GetInput()
        {
            List<double> ds = new List<double>();
            foreach (Round r in _prev)
            {
                ds.Add(RateTo0_1(r.Red));
                ds.Add(RateTo0_1(r.Green));
                ds.Add(RateTo0_1(r.Black));
                ds.Add(r.Result == Result.red ? 0.9 : 0.1);
                ds.Add(r.Result == Result.green ? 0.9 : 0.1);
                ds.Add(r.Result == Result.black ? 0.9 : 0.1);
            }
            ds.Add(RateTo0_1(_last.Red));
            ds.Add(RateTo0_1(_last.Green));
            ds.Add(RateTo0_1(_last.Black));
            return ds.ToArray();
        }
        private double RateTo0_1(double rate) => Math.Min(rate / 50000.0, 1);
    }
}
