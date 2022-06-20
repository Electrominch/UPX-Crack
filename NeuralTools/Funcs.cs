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
        public static List<CompareBunch> CreateBunches(List<Round> rounds, int prevGames, List<NextGen> nets, double predictConf)
        {
            List<CompareBunch> bunches = new List<CompareBunch>();
            List<Round> prevGamesWithCur = rounds.Take(prevGames + 1).ToList();
            for (int i = prevGames + 1; i < rounds.Count; i++)
            {
                var pred = Predictor.Predict(prevGamesWithCur, nets);
                if (pred.Max() >= predictConf)
                    bunches.Add(new CompareBunch(pred, rounds[i - 1]));
                prevGamesWithCur.Add(rounds[i]);
                prevGamesWithCur.RemoveAt(0);
            }
            return bunches;
        }

        public static int InputGames(this NextGen n) => Neurons2Games(n.Layers[0].NumOfInputNeurons);

        public static List<LearningSet> CreateLearnSets(List<Round> rounds, int prevGames)
        {
            List<LearningSet> sets = new List<LearningSet>();
            for (int i = prevGames; i < rounds.Count; i++)
                sets.Add(new LearningSet(rounds.Skip(i - prevGames).Take(prevGames).ToArray(), rounds[i]));
            return sets;
        }

        public static Input CreateInput(List<Round> rounds, Round cur)
        {
            return new Input(rounds.Take(rounds.Count - 1).ToList(), rounds[rounds.Count - 1]);
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

        public static List<T> GetLast<T>(List<T> rs, int count)
        {
            List<T> rounds = new List<T>(count);
            for (int i = rs.Count - count; i < rs.Count; i++)
                rounds.Add(rs[i]);
            return rounds;
        }

        public static List<Round> DownloadRounds(int gamesFromEnd = 0)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"http://135.125.169.130/statsLink");
            request.AddRange(Math.Min(gamesFromEnd * 62, gamesFromEnd * -1 * 62));
            using var stream = request.GetResponse().GetResponseStream();
            return ReadRounds(stream);
        }

        public static List<Round> ReadFromFile(string path)
        {
            return ReadRounds(File.OpenText(path));
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
                    if (date.Year == DateTime.Now.Year)
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

        public static int IndexOfMax<T>(this T[] array)
        {
            T max = array.Max();
            for (int i = 0; i < array.Length; i++)
                if (array[i].Equals(max))
                    return i;
            return -1;
        }
    }
}
