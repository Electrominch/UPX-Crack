using Neural_Network.Next;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using NeuralTools;
using static NeuralTools.Funcs;

namespace FindOptimalConf
{
    internal class Program
    {
        static int games = 2160;
        static double ratedGames = 0.5;

        static void Main(string[] args)
        {
            var nets = new List<NextGen>();
            var maxPrevGames = 0;
            foreach (var file in new DirectoryInfo(@"C:\Users\mrpyt\Desktop\Neurals").GetFiles().OrderBy(f => f.CreationTime))
            {
                NextGen net = NextGen.LoadFromFile(file.FullName);
                net.SetFuncs(Sigmoid, DerSigmoid);
                net.Name = file.Name;
                Console.WriteLine(net);
                maxPrevGames = Math.Max(maxPrevGames, Neurons2Games(net.Layers[0].NumOfInputNeurons));
                nets.Add(net);
            }
            var rounds = DownloadRounds(games);
            Console.Write("All? ");
            var netBunches = Console.ReadLine().Contains('y')?GetAllBunches(nets):new HashSet<NextGen>[] { nets.ToHashSet() }.Concat(nets.Select(n=>new HashSet<NextGen>() { n })).ToArray();
            Console.WriteLine(netBunches.Length);
            Console.WriteLine();
            double max = 0;
            Stat maxStat = new Stat();
            HashSet<NextGen> maxBunch = null;
            double minConfWithMaxRes = 0;
            double minForOne = 1;
            NextGen worst = null; 
            foreach (var bunch in netBunches.Where(b=>b.Count>0))
            {
                var res = TestMany(bunch, rounds, maxPrevGames, out Stat stat, out double minConf);
                if(bunch.Count == nets.Count)
                    Console.WriteLine($"ALL: {res} - {stat} in {stat.Games} games ({stat.Games*1.0/games}) with {minConf} conf {Environment.NewLine}");
                if (res > max)
                {
                    max = res;
                    maxBunch = bunch;
                    maxStat = stat;
                    minConfWithMaxRes = minConf;
                }
                if(bunch.Count == 1)
                {
                    if(res < minForOne)
                    {
                        minForOne = res;
                        worst = bunch.Single();
                    }
                }
            }
            Console.WriteLine($"{max} - {maxStat} in {games} games ({1.0*maxStat.Games/ games}) with {minConfWithMaxRes}");
            if (maxBunch != null)
                Console.WriteLine(String.Join(Environment.NewLine, maxBunch));
            Console.WriteLine();
            Console.WriteLine($"Worst {worst.Name}: {minForOne}");
        }

        static double TestMany(HashSet<NextGen> nets, List<Round> rounds, int prevGames, out Stat stat, out double minConf)
        {
            List<Predict> ress = new List<Predict>();
            for(int i = prevGames; i < rounds.Count; i++)
            {
                double[] predicts = new double[3];
                foreach (var net in nets)
                {
                    var prevRoundsForNet = Neurons2Games(net.Layers[0].NumOfInputNeurons);
                    var set = CreateInput(rounds.Skip(i-prevRoundsForNet).Take(prevRoundsForNet).ToList(), rounds[i]);
                    var curPredict = net.ForwardPassData(set.In).ToList();
                    for(int j = 0; j < curPredict.Count; j++)
                        predicts[j] += 1.0 * curPredict[j];
                }
                if(nets.Count>1)
                    predicts = predicts.Select(p => p / predicts.Sum()).ToArray();
                var expected = new LearningSet(new Round[0], rounds[i]);
                bool win = expected.ExpectedRes.ToList().IndexOf(expected.ExpectedRes.Max()) == predicts.ToList().IndexOf(predicts.Max());
                ress.Add(new Predict() { predict = predicts, win = win });
            }
            int wins = 0;
            int loses = 0;
            ress = ress.OrderBy(r=>r.predict.Max()).ToList();
            for(int i = (int)Math.Max(0, ress.Count - 1 - ress.Count * ratedGames); i < ress.Count; i++)
            {
                if (ress[i].win)
                    wins++;
                else
                    loses++;
            }
            stat.win = wins;
            stat.lose = loses;
            minConf = ress[(int)Math.Max(0, ress.Count - 1 - ress.Count * ratedGames)].predict.Max();
            return wins * 1.0 / (loses + wins);
        }

        static HashSet<T>[] GetAllBunches<T>(List<T> aviable)
        {
            List<HashSet<T>> bunches = new List<HashSet<T>>();
            for(long num = 0; num<Math.Pow(2, aviable.Count); num++)
            {
                bunches.Add(new HashSet<T>());
                for (int i = 0; i < aviable.Count; i++)
                    if ((num >> i) % 2 == 0)
                        bunches.Last().Add(aviable[i]);
            }
            return bunches.ToArray();
        }
    }
}
