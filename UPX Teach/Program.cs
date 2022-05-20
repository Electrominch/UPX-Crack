using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AccordAdapter;
using Neural_Network;
using Neural_Network.Next;
using NeuralTools;
using static NeuralTools.Funcs;

namespace UPX_Teach
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
            List<INeuro> nets = new List<INeuro>();
            Console.Write("Кол-во прошлых игр: ");
            int games = int.Parse(Console.ReadLine());
            List<int> layers = new List<int>() { Games2Neurons(games) };
            Console.WriteLine($"Входных нейронов {layers[0]}");
            Console.Write("Кол-во последних игр для теста: ");
            int forTest = int.Parse(Console.ReadLine());
            Console.Write("Скрытые нейроны (через пробел): ");
            foreach(var n in Console.ReadLine().Split(' ').Where(s=>Int32.TryParse(s, out int d)).Select(s=>int.Parse(s)))
                layers.Add(n);
            layers.Add(3);
            Console.Write("Количество сетей: ");
            int count = int.Parse(Console.ReadLine());
            nets = new AccordNeuro[count].Cast<INeuro>().ToList();
            Console.Write("LearningRate: ");
            double rate = double.Parse(Console.ReadLine().Replace('.', ','));
            Console.Write("Максимальная ошибка: ");
            double maxError = double.Parse(Console.ReadLine().Replace('.',','));
            Console.Write("Доля игр для ставки: ");
            double rateGames = double.Parse(Console.ReadLine().Replace('.',','));

            List<Round> rounds = DownloadRounds();
            List<Task> tasks = new List<Task>();
            for(int i = 0; i < rounds.Count; i++)
            {
                var t = Task.Run(() =>
                {
                    int curIndex = i;
                    double res = 0;
                    while(res <= 0.505)
                    {
                        nets[curIndex] = new AccordNeuro(layers.ToArray());
                        nets[curIndex].LearningRatio = rate;
                        Learn(nets[curIndex], rounds.Take(rounds.Count - forTest).ToList(), games, maxError);
                        res = Test(nets[curIndex], rounds.Skip(rounds.Count - forTest).ToList(), games, rateGames, out Stat stat);
                        Console.WriteLine($"Обучение{curIndex} - {res} - {stat} in {stat.Games} games ({stat.Games*1.0/ rounds.Skip(rounds.Count - forTest).Count()})");
                    }

                    nets[curIndex].SaveToFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),  "Accord", "Saves", $"net{curIndex}-{res}.accord"));
                    Console.WriteLine($"Saved {curIndex}-{res}");
                });
                Thread.Sleep(100);
                tasks.Add(t);
            }
            foreach (var task in tasks)
                task.Wait();
        }

        static void Learn(INeuro net, List<Round> rounds, int games, double maxErr)
        {
            List<LearningSet> sets = CreateLearnSets(rounds, games);
            double[][] input = new double[sets.Count][];
            double[][] output = new double[sets.Count][];
            for(int i = 0; i < sets.Count; i++)
            {
                input[i] = sets[i].InputData;
                output[i] = sets[i].ExpectedRes;
            }
            double err = 0;
            int count = 0;
            do
            {
                err = net.AdjustWeights(input, output)/input.Length;
                //if (count++ % 100000 == 0)
                    Console.WriteLine($"{count++}: {err}");
            }
            while (err > maxErr);
        }

        public static double Test(INeuro net, List<Round> rounds, int games, double rateGames, out Stat stat)
        {
            List<LearningSet> sets = CreateLearnSets(rounds, games);
            List<GamePredict> results = new List<GamePredict>();
            foreach (var set in sets)
            {
                double[] netRes = net.ForwardPassData(set.InputData);
                bool win = netRes.ToList().IndexOf(netRes.Max()) == set.ExpectedRes.ToList().IndexOf(set.ExpectedRes.Max());
                results.Add(new GamePredict() { confidence = netRes.Max(), win = win});
            }
            results = results.OrderBy(r=>r.confidence).ToList();
            int wins = 0;
            int errors = 0;
            for(int i = (int)(results.Count - results.Count * rateGames-1); i<results.Count; i++)
            {
                if (results[i].win)
                    wins++;
                else
                    errors++;
            }
            stat.win = wins;
            stat.lose = errors;
            return wins * 1.0 / (wins + errors);
        }

        struct GamePredict
        {
            public double confidence;
            public bool win;
        }
    }
}
