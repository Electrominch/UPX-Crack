using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Neural_Network.Next;

namespace UPXLearn
{
    internal class Program
    {
        static double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));
        static double DerSigmoid(double x) => Sigmoid(x) * (1 - Sigmoid(x));
        static NextGen net;
        static int games = 4;
        static int forTest = 208;
        static double maxError = 0;
        static double needForWin = 0;
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");

            Console.Write("Кол-во прошлых игр: ");
            games = int.Parse(Console.ReadLine());
            Console.Write("Кол-во последних игр для теста: ");
            forTest = int.Parse(Console.ReadLine());
            Console.Write("Скрытые нейроны (через пробел): ");
            List<NeuronLayer> layers = new List<NeuronLayer>() { new NeuronLayer(games*6+3) };
            foreach(var n in Console.ReadLine().Split(' ').Select(s=>int.Parse(s)))
                layers.Add(new NeuronLayer(n));
            layers.Add(new NeuronLayer(3));
            net = new NextGen(Sigmoid, DerSigmoid, layers.ToArray());
            Console.Write("LearningRate: ");
            net.LearningRatio = double.Parse(Console.ReadLine().Replace('.',','));
            Console.Write("Максимальная ошибка: ");
            maxError = double.Parse(Console.ReadLine().Replace('.',','));
            Console.Write("Уверенность для ставки: ");
            needForWin = double.Parse(Console.ReadLine().Replace('.',','));
            Console.WriteLine($"Нейросеть: {net}");

            List<Round> rounds = DownloadRounds();
            Console.WriteLine("Learning...");
            Learn(rounds.Take(rounds.Count-forTest).ToList());
            Console.WriteLine("Testing...");
            Console.WriteLine(Test(rounds.Skip(rounds.Count - forTest).ToList()));
            Console.Write("Save?");
            if (Console.ReadLine().Contains("y"))
                NextGen.SaveToFile(net, @"C:\Users\mrpyt\Desktop\fsdsdfsdf.txt");
        }

        public static List<Round> ReadRounds(StreamReader s)
        {
            List<Round> rounds = new List<Round>();
            while (s.EndOfStream == false)
            {
                string[] values = s.ReadLine().Split('\t').Select(s => s.Trim()).ToArray();
                double r = double.Parse(values[0]);
                double g = double.Parse(values[1]);
                double b = double.Parse(values[2]);
                Result res = (Result)Enum.Parse(typeof(Result), values[3]);
                rounds.Add(new Round(r, g, b, res));
            }
            s.Close();
            return rounds;
        }

        public static List<LearningSet> CreateSets(List<Round> rounds)
        {
            List<LearningSet> sets = new List<LearningSet>();
            for (int i = games; i < rounds.Count; i++)
            {
                List<Round> prev8 = new List<Round>();
                for (int j = i - 1; j >= i - games; j--)
                    prev8.Insert(0, rounds[j]);
                sets.Add(new LearningSet(prev8.ToArray(), rounds[i]));
            }
            return sets;
        }

        static void Learn(List<Round> rounds)
        {
            List<LearningSet> sets = CreateSets(rounds);
            Console.WriteLine($"Сетов: {sets.Count}");
            Random rnd = new Random();
            int count = 0;
            double err = 0;
            int countErr = 0;
            do
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
            while (count<1000||(err / countErr) > maxError);
        }

        public static double Test(List<Round> rounds)
        {
            List<LearningSet> sets = CreateSets(rounds);
            Console.WriteLine($"Сетов: {sets.Count}");
            int wins = 0;
            int errors = 0;
            int[] ress = new int[3];
            foreach (var set in sets)
            {
                double[] netRes = net.ForwardPassData(set.InputData);
                if (netRes.Max() < needForWin)
                    continue;
                string expected = string.Join(" ", set.ExpectedRes);
                string res = string.Join(" ", netRes);
                bool win = netRes.ToList().IndexOf(netRes.Max()) == set.ExpectedRes.ToList().IndexOf(set.ExpectedRes.Max());
                Console.WriteLine($"{expected} -> {res} -> {win}");
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
            Console.WriteLine($"Wins: {wins} - Errors: {errors}");
            Console.WriteLine($"Result: {string.Join(" ", ress)}");
            return wins * 1.0 / (wins + errors);
        }

        public static List<Round> DownloadRounds()
        {
            using (var client = new WebClient())
            {
                var stream = new StreamReader(client.OpenRead(@"http://135.125.169.130/statsLink"));
                return ReadRounds(stream);
            }
        }
    }
}
