using Neural_Network.Next;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NeuralTools.Funcs;

namespace NeuralTools
{
    public class Predictor
    {
        public static double[] Predict(List<Round> allRounds, Round cur, List<NextGen> nets)
        {
            double[] predicts = new double[3];
            foreach (var net in nets)
            {
                var prevRounds = Neurons2Games(net.Layers[0].NumOfInputNeurons);
                List<Round> lastRounds = GetLastRounds(allRounds, prevRounds);
                lastRounds.Add(cur);
                var curPredict = net.ForwardPassData(CreateInput(lastRounds).In).ToList();
                for (int i = 0; i < predicts.Length; i++)
                    predicts[i] += 1.0 * curPredict[i];
                Console.WriteLine($"{net.Name + ":",-13}\t" + string.Join(" ", curPredict) + $" -> {(Result)curPredict.IndexOf(curPredict.Max())}");
            }
            predicts = predicts.Select(p => p / predicts.Sum()).ToArray();
            return predicts;
        }
    }
}
