using Neural_Network.Next;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NeuralTools
{
    public static class Funcs
    {
        public static List<LearningSet> CreateLearnSets(List<Round> rounds, int games)
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

        public static Input CreateInput(List<Round> rounds)
        {
            List<Round> prev = new List<Round>();
            for (int j = rounds.Count - 2; j >= 0; j--)
                prev.Insert(0, rounds[j]);
            return new Input(prev, rounds[rounds.Count - 1]);
        }

        public static bool EqualArray<T>(T[] l, T[] r)
        {
            if (l.Length != r.Length)
                return false;
            for (int i = 0; i < l.Length; i++)
                if (l[i].Equals(r[i]) == false)
                    return false;
            return true;
        }

        public static List<Round> GetLastRounds(List<Round> rs, int count)
        {
            List<Round> rounds = new List<Round>(count);
            for (int i = rs.Count - count; i < rs.Count; i++)
                rounds.Add(rs[i]);
            return rounds;
        }

        public static List<Round> DownloadRounds(int gamesFromEnd = 0)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"http://135.125.169.130/statsLink");
            request.AddRange(Math.Min(gamesFromEnd*62, gamesFromEnd * -1 * 62));
            using var stream = request.GetResponse().GetResponseStream();
            return ReadRounds(stream);
        }

        public static List<Round> ReadRounds(Stream s) => ReadRounds(new StreamReader(s));
        public static List<Round> ReadRounds(StreamReader s)
        {
            List<Round> rounds = new List<Round>();
            while (s.EndOfStream == false)
            {
                try
                {
                    string[] values = s.ReadLine().Split('\t').Select(s => s.Trim()).ToArray();
                    double r = double.Parse(values[0]);
                    double g = double.Parse(values[1]);
                    double b = double.Parse(values[2]);
                    Result res = (Result)Enum.Parse(typeof(Result), values[3]);
                    var date = DateTime.Parse(values[4]);
                    if(date.Year == DateTime.Now.Year)
                        rounds.Add(new Round(r, g, b, res, date));
                }
                catch { }
            }
            s.Close();
            return rounds;
        }

        public static List<NextGen> ReadNets()
        {
            List<NextGen> neurals = new List<NextGen>();
            foreach (var file in new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Neurals")).GetFiles())
            {
                var net = NextGen.LoadFromFile(file.FullName);
                net.SetFuncs(Sigmoid, DerSigmoid);
                net.Name = file.Name;
                neurals.Add(net);
            }
            return neurals;
        }

        public static int Neurons2Games(int n) => (n - 5) / 6;
        public static int Games2Neurons(int g) => g * 6 + 5;

        public static double RateTo0_1(double rate) => Math.Min(rate / 25000.0, 1);
        public static double DateTo0_1sin(double secs) => (1 + Math.Sin(secs * Math.PI / 43200)) / 2;
        public static double DateTo0_1cos(double secs) => (1 + Math.Cos(secs * Math.PI / 43200)) / 2;

        public static double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));
        public static double DerSigmoid(double x) => Sigmoid(x) * (1 - Sigmoid(x));
    }
}
