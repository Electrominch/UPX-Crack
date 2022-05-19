using Neural_Network.Next;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace UPPredict
{
    internal class Program
    {
        static double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));
        static double DerSigmoid(double x) => Sigmoid(x) * (1 - Sigmoid(x));

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
            Dictionary<string, NextGen> neurals = new Dictionary<string, NextGen>();
            foreach (var file in new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Neurals")).GetFiles())
            {
                Console.WriteLine(file.Name);
                var net = NextGen.LoadFromFile(file.FullName);
                net.SetFuncs(Sigmoid, DerSigmoid);
                neurals.Add(file.Name, net);
            }
            var b = CreateB();

            while (true)
            {
                if(FindTimer(b) != 5)
                {
                    Thread.Sleep(1000);
                    continue;
                }
                Console.Clear();
                var rates = b.FindElement(By.ClassName(@"users-rates"));
                double redRates = double.Parse(rates.FindElement(By.XPath(@"div/div/div[2]/span[2]/span[2]")).Text.Replace(" ", "").Replace(".", ","));
                double greenRates = double.Parse(rates.FindElement(By.XPath(@"div[2]/div/div[2]/span[2]/span[2]")).Text.Replace(" ", "").Replace(".", ","));
                double blackRates = double.Parse(rates.FindElement(By.XPath(@"div[3]/div/div[2]/span[2]/span[2]")).Text.Replace(" ", "").Replace(".", ","));
                var curRound = new Round(redRates, greenRates, blackRates, Result.green);
                var allRounds = DownloadRounds();
                int[] predicts = new int[3];
                foreach(var kp in neurals)
                {
                    var net = kp.Value;
                    var prevRounds = (net.Layers[0].NumOfInputNeurons - 3) / 6;
                    List<Round> lastRounds = GetLastRounds(allRounds, prevRounds);
                    var curPredict = net.ForwardPassData(new Input(lastRounds, curRound).In).ToList();
                    predicts[curPredict.IndexOf(curPredict.Max())]++;
                    Console.WriteLine($"{kp.Key}: " + string.Join(" ", curPredict) + $" -> {(Result)curPredict.IndexOf(curPredict.Max())}");
                }
                Console.WriteLine(String.Join(" ", predicts));
                Console.WriteLine($"Red:{predicts[0]*1.0/predicts.Sum()}\nGreen:{predicts[1] * 1.0 / predicts.Sum()}\nBlack:{predicts[2] * 1.0 / predicts.Sum()}");
                int[] oldHistory = FindHistory(b);
                int[] curHistory = FindHistory(b);
                while (EqualArray(oldHistory, curHistory))
                {
                    curHistory = FindHistory(b);
                    Thread.Sleep(1000);
                }
                Thread.Sleep(1000);
                Console.WriteLine($"History: {Environment.NewLine}{string.Join(Environment.NewLine, GetLastRounds(DownloadRounds(), 10))}");
            }
        }

        static ChromeDriver CreateB()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("no-sandbox");
            options.AddArguments("headless");
            options.AddArguments("--log-level=3");
            options.AddArguments("--silent");
            ChromeDriver b = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromMinutes(3));
            b.Manage().Timeouts().PageLoad.Add(TimeSpan.FromSeconds(30));
            b.Url = "https://up0r9.tech/games/roulette";
            b.Navigate();
            return b;
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

        static bool EqualArray<T>(T[] l, T[] r)
        {
            if (l.Length != r.Length)
                return false;
            for (int i = 0; i < l.Length; i++)
                if (l[i].Equals(r[i]) == false)
                    return false;
            return true;
        }

        static List<Round> GetLastRounds(List<Round> rs, int count)
        {
            List<Round> rounds = new List<Round>();
            for (int i = rs.Count - count; i < rs.Count; i++)
                rounds.Add(rs[i]);
            return rounds;
        }

        public static List<Round> DownloadRounds()
        {
            using (var client = new WebClient())
            {
                var stream = new StreamReader(client.OpenRead(@"http://135.125.169.130/statsLink"));
                return ReadRounds(stream);
            }
        }

        static int[] FindHistory(ChromeDriver b)
        {
            return b.FindElement(By.ClassName(@"make-rate__history")).Text.Split(Environment.NewLine).Select(x => int.Parse(x)).ToArray();
        }

        static int FindTimer(ChromeDriver b)
        {
            if (int.TryParse(b.FindElement(By.ClassName(@"roulette-timer")).Text, out int res))
                return res;
            return -1;
        }
        static Result GetColor(int num)
        {
            if (num == 0)
                return Result.green;
            else if (num <= 7)
                return Result.red;
            return Result.black;
        }
    }
}
