using Neural_Network.Next;
using NeuralTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static NeuralTools.Funcs;

namespace ComparedLearn
{
    internal class Program
    {
        private static List<NextGen> nets = ReadNets();
        private static double predictConf = 0.638548562257657;

        static void Main(string[] args)
        {
            Console.Write("Количество прошлых предсказаний: ");
            int predictsCount = int.Parse(Console.ReadLine());
            List<int> layers = new List<int>() { predictsCount * 6+3 };
            Console.WriteLine($"Первый слой: {layers[0]}");
            //Console.Write("Увренность предсказания: ");
            //predictConf = double.Parse(Console.ReadLine().Replace('.', ','));
            //Console.Write("Кол-во последних игр для теста: ");
            //int forTest = int.Parse(Console.ReadLine());
            int forTest = 2160;
            Console.Write("Скрытые нейроны (через пробел): ");
            foreach (var n in Console.ReadLine().Split(' ').Where(s => Int32.TryParse(s, out int d)).Select(s => int.Parse(s)))
                layers.Add(n);
            layers.Add(3);
            var compareNet = new NextGen(Sigmoid, DerSigmoid, layers.Select(n=>new NeuronLayer(n)).ToArray());
            Console.Write("LearningRate: ");
            compareNet.LearningRatio = double.Parse(Console.ReadLine().Replace('.', ','));
            Console.Write("Максимальная ошибка: ");
            double maxError = double.Parse(Console.ReadLine().Replace('.', ','));
            Console.WriteLine($"Net: {compareNet}");
            var rounds = DownloadRounds();
            var learnRounds = rounds.Take(rounds.Count - forTest).ToList();
            var prevGamesCount = nets.Max(n=>n.InputGames());
            List<CompareBunch> learnBunches = CreateBunches(learnRounds, prevGamesCount, nets, predictConf); 
            var learnSets = CreateLearnSets(learnBunches, predictsCount);
            Console.WriteLine($"Learning Sets: {learnSets.Count}");
            Learn(learnSets, compareNet, maxError);
            var testRounds = rounds.Skip(rounds.Count - forTest).ToList();
            List<CompareBunch> testBunches = CreateBunches(learnRounds, prevGamesCount, nets, predictConf);
            var testSets = CreateLearnSets(testBunches, predictsCount);
            Console.WriteLine($"Test Sets: {testSets.Count}");
            var stat = Test(testSets, compareNet);
            Console.WriteLine($"Wins: {stat.win} \nLoses: {stat.lose}\n{stat.win * 1.0 / stat.Games}");
            Console.Write("Save? (y/n) ");
            if (Console.ReadKey().KeyChar.ToString().ToLower() == "y")
                NextGen.SaveToFile(compareNet, Path.Combine(@"C:\Users\mrpyt\Desktop\Neurals\Compare", $"compared{stat.win * 1.0 / stat.Games}.txt"));
        }

        private static void Learn(List<CompareLearnSet> sets, NextGen net, double maxErr)
        {
            Console.WriteLine($"Сетов: {sets.Count}");
            double err = 0;
            int count = 0;
            int countErr = 1;
            Random rand = new Random();
            do
            {
                var set = sets[rand.Next(sets.Count)];
                if(countErr%100000==0)
                {
                    Console.WriteLine($"{count} : {err/countErr}");
                    countErr = 0;
                    err = 0;
                }
                err += net.AdjustWeights(set.Input, set.Expected);
                countErr++;
                count++;
            } while(err/countErr > maxErr || countErr<sets.Count);
            Console.WriteLine($"Обучено за {count}");
        }

        private static Stat Test(List<CompareLearnSet> sets, NextGen net)
        {
            int wins = 0;
            int loses = 0;
            foreach(var set in sets)
            {
                var res = net.ForwardPassData(set.Input).ToList();
                bool win = res.IndexOf(res.Max()) == set.Expected.ToList().IndexOf(set.Expected.Max());
                if(win)
                    wins++;
                else
                    loses++;
            }
            return new Stat() { win = wins, lose = loses};
        }

        private static List<CompareLearnSet> CreateLearnSets(List<CompareBunch> bunches, int prevBunches)
        {
            var learnSets = new List<CompareLearnSet>();
            for(int i = prevBunches; i < bunches.Count;i++)
            {
                List<CompareBunch> prev = bunches.Skip(i- prevBunches).Take(prevBunches).ToList();
                learnSets.Add(new CompareLearnSet(prev, bunches[i]));
            }
            return learnSets;
        }
    }
}
