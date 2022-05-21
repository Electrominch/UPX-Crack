﻿using System;
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
            List<int> layers = new List<int>() { Games2Neurons(games) };
            Console.WriteLine($"Первый слой: {layers[0]}");
            Console.Write("Кол-во последних игр для теста: ");
            int forTest = int.Parse(Console.ReadLine());
            Console.Write("Скрытые нейроны (через пробел): ");
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
            Console.Write("Доля игр для ставки: ");
            double ratedGames = double.Parse(Console.ReadLine().Replace('.',','));

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
                        res = Test(nets[curIndex], rounds.Skip(rounds.Count - forTest).ToList(), games, ratedGames, out var stat);
                        Console.WriteLine($"Обучение{curIndex} - {res} - {stat} in {stat.Games} games {stat.Games*1.0/ rounds.Skip(rounds.Count - forTest).Count()}");
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

        public static double Test(NextGen net, List<Round> rounds, int games, double rateGames, out Stat stat)
        {
            List<LearningSet> sets = CreateLearnSets(rounds, games);
            List<GamePredict> results = new List<GamePredict>();
            foreach (var set in sets)
            {
                double[] netRes = net.ForwardPassData(set.InputData);
                bool win = netRes.ToList().IndexOf(netRes.Max()) == set.ExpectedRes.ToList().IndexOf(set.ExpectedRes.Max());
                results.Add(new GamePredict() { confidence = netRes.Max(), win = win });
            }
            results = results.OrderBy(r => r.confidence).ToList();
            int wins = 0;
            int errors = 0;
            for (int i = (int)(results.Count - results.Count * rateGames - 1); i < results.Count; i++)
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
