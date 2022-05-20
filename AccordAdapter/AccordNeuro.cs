using Neural_Network;
using System;
using Accord.Neuro;
using Accord.Neuro.Learning;
using System.Linq;
using System.IO;

namespace AccordAdapter
{
    public class AccordNeuro : INeuro
    {
        private ActivationNetwork network;
        private ResilientBackpropagationLearning learn;
        public string Name { get; set; }

        public int InputNeurons => network.InputsCount;

        public double LearningRatio { get => learn.LearningRate; set { learn.LearningRate = value; } }

        public AccordNeuro(params int[] layers)
        {
            network = new ActivationNetwork(new BipolarSigmoidFunction(), layers[0], layers.Skip(1).ToArray());
            network.Randomize();
            SetLearn();
        }

        private AccordNeuro(ActivationNetwork net)
        {
            network = net;
            network.Randomize();
            SetLearn();
        }

        private void SetLearn()
        {
            learn = new ResilientBackpropagationLearning(network);
            new NguyenWidrow(network).Randomize();
        }

        public double AdjustWeights(double[][] input, double[][] targets)
        {
            return learn.RunEpoch(input, targets);
        }
        
        public double AdjustWeights(double[] input, double[] targets)
        {
            return AdjustWeights(new double[][] { input }, new double[][] { targets });
        }

        public double[] ForwardPassData(double[] input)
        {
            return network.Compute(input);
        }

        public void SaveToFile(string path)
        {
            using var fs = File.Create(path);
            network.Save(fs);
        }

        public void SetFuncs(Func<double, double> activationFunc, Func<double, double> derivativeFunction)
        {
            
        }

        public static AccordNeuro LoadFromFile(string path)
        {
            return new AccordNeuro((ActivationNetwork)ActivationNetwork.Load(path));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
