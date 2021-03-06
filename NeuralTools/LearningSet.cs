using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NeuralTools.Funcs;

namespace NeuralTools
{
    public class LearningSet
    {
        private Round[] _prev;
        private Round _last;
        public double[] InputData { get; private set; }
        public double[] ExpectedRes { get; private set; }
        private double[] _prevPredicts = new double[] { 0.5, 0.5, 0.5 };
        public double[] PrevPredict
        {
            private get => _prevPredicts;
            set
            {
                _prevPredicts = value;
                InputData = GetInput();
            }
        }

        public LearningSet(Round[] prev, Round last)
        {
            _prev = prev;
            _last = last;
            InputData = GetInput();
            ExpectedRes = GetExpected();
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
            ds.AddRange(PrevPredict);
            ds.Add(RateTo0_1(_last.Red));
            ds.Add(RateTo0_1(_last.Green));
            ds.Add(RateTo0_1(_last.Black));
            ds.Add(DateTo0_1sin(_last.date.TimeOfDay.TotalSeconds));
            ds.Add(DateTo0_1cos(_last.date.TimeOfDay.TotalSeconds));
            return ds.ToArray();
        }

        public double[] GetExpected()
        {
            List<double> ds = new List<double>();
            ds.Add(_last.Result == Result.red ? 0.9 : 0.1);
            ds.Add(_last.Result == Result.green ? 0.9 : 0.1);
            ds.Add(_last.Result == Result.black ? 0.9 : 0.1);
            return ds.ToArray();
        }
    }
}
