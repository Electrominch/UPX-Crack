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
        static int games = 1000;
        static double forRate = 0;

        static void Main(string[] args)
        {
            var nets = new List<NextGen>();
            var maxPrevGames = 0;
            var rs = DownloadRounds();
            foreach (var file in new DirectoryInfo(@"C:\Users\mrpyt\Desktop\Neurals").GetFiles().OrderBy(f => f.CreationTime))
            {
                NextGen net = NextGen.LoadFromFile(file.FullName);
                net.SetFuncs(Sigmoid, DerSigmoid);
                net.Name = file.Name;
                Console.WriteLine(net);
                maxPrevGames = Math.Max(maxPrevGames, Neurons2Games(net.Layers[0].NumOfInputNeurons));
                nets.Add(net);
            }
            var rounds = GetLastRounds(DownloadRounds(), games);
            var netBunches = GetAllBunches(nets);
            Console.WriteLine(netBunches.Length);
            Console.WriteLine();
            double max = 0;
            Stat maxStat = new Stat();
            HashSet<NextGen> maxBunch = null;
            double minForOne = 1;
            NextGen worst = null; 
            foreach (var bunch in netBunches.Where(b=>b.Count>0))
            {
                var res = TestMany(bunch, rounds, maxPrevGames,forRate, out Stat stat);
                if(bunch.Count == nets.Count)
                    Console.WriteLine($"ALL: {res} - {stat}");
                if (res > max)
                {
                    max = res;
                    maxBunch = bunch;
                    maxStat = stat;
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
            Console.WriteLine($"{max} - {maxStat} in {games} games ({1.0*maxStat.Games/ games})");
            Console.WriteLine(String.Join(Environment.NewLine, maxBunch));
            Console.WriteLine();
            Console.WriteLine($"Worst {worst.Name}: {minForOne}");
        }

        static double TestMany(HashSet<NextGen> nets, List<Round> rounds, int prevGames, double rate, out Stat stat)
        {
            int win = 0;
            int err = 0;
            for(int i = prevGames; i < rounds.Count; i++)
            {
                double[] predicts = new double[3];
                foreach (var net in nets)
                {
                    var prevRoundsForNet = Neurons2Games(net.Layers[0].NumOfInputNeurons);
                    var set = CreateInput(rounds.Skip(i-prevRoundsForNet).Take(prevRoundsForNet+1).ToList());
                    var curPredict = net.ForwardPassData(set.In).ToList();
                    for(int j = 0; j < curPredict.Count; j++)
                        predicts[j] += 1.0 * curPredict[j];
                }
                predicts = predicts.Select(p => p / predicts.Sum()).ToArray();
                if (predicts.Max() < rate)
                    continue;
                var expected = new LearningSet(new Round[0], rounds[i]);
                if (expected.ExpectedRes.ToList().IndexOf(expected.ExpectedRes.Max()) == predicts.ToList().IndexOf(predicts.Max()))
                    win++;
                else
                    err++;
            }
            stat.win = win;
            stat.lose = err;
            return win * 1.0 / (err+win);
        }

        struct Stat
        {
            public int win;
            public int lose;
            public int Games => win+lose;

            public override string ToString()
            {
                return $"win:{win} lose:{lose}";
            }
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
