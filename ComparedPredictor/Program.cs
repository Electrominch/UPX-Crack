using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Neural_Network.Next;
using NeuralTools;
using UPX_Parser;
using static NeuralTools.Funcs;

namespace ComparedPredictor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
            var parser = new UPXParser();
            Console.WriteLine("Connected");
            var net = NextGen.LoadFromFile(@"C:\Users\mrpyt\Desktop\Neurals\Compare\compared0,5336538461538461.txt");
            net.SetFuncs(Sigmoid, DerSigmoid);
            double forRate = 0.5219777005228055;
            double[] prevPredicts = new double[3];
            while (true)
            {
                var curRound = parser.GetCurRound();
                var allRounds = DownloadRounds(256);
                allRounds.Add(curRound);
                var nets = ReadNets();
                var prevGamesCount = nets.Max(n => n.InputGames());
                var bunches = CreateBunches(allRounds, prevGamesCount, nets, forRate);
                var set = CreateInputSet(bunches, 8);
                double[] predicts = net.ForwardPassData(set.Input);
                Console.Clear();
                Console.WriteLine($"Red:{predicts[0]}\nGreen:{predicts[1]}\nBlack:{predicts[2]}");
                Console.WriteLine(EqualArray(predicts, prevPredicts) ? "Do not play": "Play");
                prevPredicts = predicts;
                parser.WaitChangeHistory();
                Thread.Sleep(1000);
                Console.WriteLine($"History: {Environment.NewLine}{string.Join(Environment.NewLine, DownloadRounds(10))}");
            }
        }

        private static CompareInputSet CreateInputSet(List<CompareBunch> bunches, int prevBunches)
        {
            List<CompareBunch> prev = bunches.Skip(bunches.Count - prevBunches-1).ToList();
            return new CompareInputSet(prev.Take(prev.Count-1).ToList(), prev.Last());
        }
    }
}
