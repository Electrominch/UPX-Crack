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
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("ru-RU");
            var parser = new UPXParser();
            while (true)
            {
                var curRound = parser.GetCurRound();
                var allRounds = DownloadRounds(64);
                double[] predicts = Predictor.Predict(allRounds, curRound, ReadNets());
                Console.Clear();
                Console.WriteLine($"Red:{predicts[0]}\nGreen:{predicts[1]}\nBlack:{predicts[2]}");
                parser.WaitChangeHistory();
                Thread.Sleep(1000);
                Console.WriteLine($"History: {Environment.NewLine}{string.Join(Environment.NewLine, DownloadRounds(10))}");
            }
        }
    }
}
