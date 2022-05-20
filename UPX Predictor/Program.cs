using AccordAdapter;
using Neural_Network;
using Neural_Network.Next;
using NeuralTools;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

using static NeuralTools.Funcs;

namespace UPXPredictor
{
    internal class Program
    {
        static double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));
        static double DerSigmoid(double x) => Sigmoid(x) * (1 - Sigmoid(x));

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
            var b = CreateB();

            while (true)
            {
                int timer = FindTimer(b);
                while (timer < 0 || timer > 5)
                {
                    Thread.Sleep(1000);
                    timer = FindTimer(b);
                }
                Console.Clear();
                Console.WriteLine($"Timer: {timer}");
                var rates = b.FindElement(By.ClassName(@"users-rates"));
                double redRates = double.Parse(rates.FindElement(By.XPath(@"div/div/div[2]/span[2]/span[2]")).Text.Replace(" ", "").Replace(".", ","));
                double greenRates = double.Parse(rates.FindElement(By.XPath(@"div[2]/div/div[2]/span[2]/span[2]")).Text.Replace(" ", "").Replace(".", ","));
                double blackRates = double.Parse(rates.FindElement(By.XPath(@"div[3]/div/div[2]/span[2]/span[2]")).Text.Replace(" ", "").Replace(".", ","));
                var curRound = new Round(redRates, greenRates, blackRates, Result.green, DateTime.Now);
                var allRounds = DownloadRounds();
                double[] predicts = new double[3];
                foreach(INeuro net in ReadNets())
                {
                    var prevRounds = Neurons2Games(net.InputNeurons);
                    List<Round> lastRounds = GetLastRounds(allRounds, prevRounds);
                    lastRounds.Add(curRound);
                    var curPredict = net.ForwardPassData(CreateInput(lastRounds).In).ToList();
                    for(int i = 0; i < predicts.Length; i++)
                        predicts[i]+=1.0* curPredict[i];
                    Console.WriteLine($"{net.Name+ ":", -13}\t" + string.Join(" ", curPredict) + $" -> {(Result)curPredict.IndexOf(curPredict.Max())}");
                }
                predicts = predicts.Select(p => p / predicts.Sum()).ToArray();
                Console.WriteLine($"Red:{predicts[0]}\nGreen:{predicts[1]}\nBlack:{predicts[2]}");
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

        static List<INeuro> ReadNets()
        {
            List<INeuro> neurals = new List<INeuro>();
            foreach (var file in new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Accord\\Neurals")).GetFiles())
            {
                var net = AccordNeuro.LoadFromFile(file.FullName);
                net.SetFuncs(Sigmoid, DerSigmoid);
                net.Name = file.Name;
                neurals.Add(net);
            }
            return neurals;
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
    }
}
