using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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
            List<NextGen> nets = new List<NextGen>();

            Console.Write("Кол-во прошлых игр: ");
            int games = int.Parse(Console.ReadLine());
            Console.Write("Кол-во последних игр для теста: ");
            int forTest = int.Parse(Console.ReadLine());
            Console.Write("Скрытые нейроны (через пробел): ");
            List<int> layers = new List<int>() { Games2Neurons(games) };
            foreach(var n in Console.ReadLine().Split(' ').Where(s=>Int32.TryParse(s, out int d)).Select(s=>int.Parse(s)))
                layers.Add(n);
            layers.Add(3);
            Console.Write("Количество сетей: ");
            int count = int.Parse(Console.ReadLine());
            nets = new NextGen[count].ToList();
            Console.Write("LearningRate: ");
            double rate = double.Parse(Console.ReadLine().Replace('.', ','));
            Console.Write("Максимальная ошибка: ");
            double maxError = double.Parse(Console.ReadLine().Replace('.',','));
            Console.Write("Уверенность для ставки: ");
            double needForWin = double.Parse(Console.ReadLine().Replace('.',','));

            List<Round> rounds = DownloadRounds();
            List<Task> tasks = new List<Task>();
            for(int i = 0; i < rounds.Count; i++)
            {
                var t = Task.Run(() =>
                {
                    int curIndex = i;
                    double res = 0;
                    while(res <= 0.5)
                    {
                        nets[curIndex] = new NextGen(Sigmoid, DerSigmoid, layers.Select(n => new NeuronLayer(n)).ToArray());
                        nets[curIndex].LearningRatio = rate;
                        Learn(nets[curIndex], rounds.Take(rounds.Count - forTest).ToList(), games, maxError);
                        res = Test(nets[curIndex], rounds.Skip(rounds.Count - forTest).ToList(), games, needForWin);
                        Console.WriteLine($"Обучение{curIndex} - {res}");
                    }

                    NextGen.SaveToFile(nets[curIndex], Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "saves", $"net{curIndex}-{res}.txt"));
                    Console.WriteLine($"Saved {curIndex}-{res}");
                });
                Thread.Sleep(100);
                tasks.Add(t);
            }
            foreach (var task in tasks)
                task.Wait();
        }

        static void Learn(NextGen net, List<Round> rounds, int games, double maxErr)
        {
            maxErr = new Random().NextDouble()*0.02+maxErr;
            Console.WriteLine($"ыыыых{maxErr}");
            List<LearningSet> sets = CreateLearnSets(rounds, games);
            Console.WriteLine($"Сетов: {sets.Count}");
            Random rnd = new Random();
            int count = 0;
            double err = 0;
            int countErr = 0;
            while (countErr < sets.Count || (err / countErr) > maxErr)
            {
                var set = sets[rnd.Next(sets.Count)];
                if ((++count) % 100000 == 0)
                {
                    Console.WriteLine($"{count}: {err / countErr}");
                    err = 0;
                    countErr = 0;
                }
                err += net.AdjustWeights(set.InputData, set.ExpectedRes);
                countErr++;
            }
        }

        public static double Test(NextGen net, List<Round> rounds, int games, double chanceToRate)
        {
            List<LearningSet> sets = CreateLearnSets(rounds, games);//
            int wins = 0;
            int errors = 0;
            int[] ress = new int[3];
            foreach (var set in sets)
            {
                double[] netRes = net.ForwardPassData(set.InputData);
                if (netRes.Max() < chanceToRate)
                    continue;
                string expected = string.Join(" ", set.ExpectedRes);
                string res = string.Join(" ", netRes);
                bool win = netRes.ToList().IndexOf(netRes.Max()) == set.ExpectedRes.ToList().IndexOf(set.ExpectedRes.Max());
                //Console.WriteLine($"{expected} -> {res} -> {win}");
                if (win)
                {
                    wins++;
                    ress[netRes.ToList().IndexOf(netRes.Max())]++;
                }
                else
                {
                    errors++;
                }
            }
            return wins * 1.0 / (wins + errors);
        }
    }
}
