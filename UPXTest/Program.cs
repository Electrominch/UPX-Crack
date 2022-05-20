using Neural_Network.Next;
using NeuralTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using static NeuralTools.Funcs;

namespace UPXTest
{
    internal class Program
    {
        static NextGen net;
        static int lastCount = 700;

        static void Main(string[] args)
        {
            var rs = DownloadRounds();
            foreach (var file in new DirectoryInfo(@"C:\Users\mrpyt\Desktop\Neurals").GetFiles().OrderBy(f => f.CreationTime))
            {
                Console.WriteLine(file.Name);
                net = NextGen.LoadFromFile(file.FullName);
                net.SetFuncs(Sigmoid, DerSigmoid);
                int games = Neurons2Games(net.InputNeurons);
                Console.WriteLine($"Games: {games}");
                Console.WriteLine(Test(rs.Skip(rs.Count - lastCount - games).ToList(), games));
                Console.WriteLine();
            }
        }

        public static double Test(List<Round> rounds, int games)
        {
            List<LearningSet> sets = CreateLearnSets(rounds, games);
            int wins = 0;
            int errors = 0;
            int[] ress = new int[3];
            foreach (var set in sets)
            {
                double[] netRes = net.ForwardPassData(set.InputData);
                if (netRes.Max() < 0.95)
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
            Console.WriteLine($"Wins: {wins} - Errors: {errors}");
            Console.WriteLine($"Result: {string.Join(" ", ress)}");
            return wins * 1.0 / (wins+errors);
        }
    }
}
