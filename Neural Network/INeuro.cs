using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neural_Network
{
    public interface INeuro
    {
        public string Name { get; set; }
        public int InputNeurons { get; }
        public double LearningRatio { get; set; }

        public double[] ForwardPassData(double[] input);
        public double AdjustWeights(double[] input, double[] targets);
        public double AdjustWeights(double[][] input, double[][] targets);
        public void SetFuncs(Func<double, double> activationFunc, Func<double, double> derivativeFunction);
        public void SaveToFile(string path);
    }
}
