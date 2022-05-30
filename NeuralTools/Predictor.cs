using NeuralTools;
using static NeuralTools.Funcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neural_Network.Next;

namespace NeuralTools
{
    public static class Predictor
    {
        public static double[] Predict(List<Round> allPrevRounds, Round cur, List<NextGen> nets)
        {
            double[] predicts = new double[3];
            foreach (var net in nets)
            {
                List<Round> lastRounds = GetLast(allPrevRounds, net.InputGames());
                var curNetPredict = Predict(lastRounds, cur, net);
                for (int i = 0; i < predicts.Length; i++)
                    predicts[i] += 1.0 * curNetPredict[i];
            }
            predicts = predicts.Select(p => p / predicts.Sum()).ToArray();
            return predicts;
        }

        public static double[] Predict(List<Round> allPrevRounds, Round cur, NextGen net)
        {
            List<Round> lastRounds = GetLast(allPrevRounds, net.InputGames());
            return net.ForwardPassData(CreateInput(lastRounds, cur).In);
        }

        public static double[] Predict(List<Round> roundsWithCur, NextGen net) => Predict(roundsWithCur.Take(roundsWithCur.Count-1).ToList(), roundsWithCur.Last(), net);
        public static double[] Predict(List<Round> roundsWithCur, List<NextGen> nets) => Predict(roundsWithCur.Take(roundsWithCur.Count-1).ToList(), roundsWithCur.Last(), nets);
    }
}
