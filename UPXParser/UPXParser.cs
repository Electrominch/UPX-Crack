using NeuralTools;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NeuralTools.Funcs;

namespace UPX_Parser
{
    public class UPXParser
    {
        ChromeDriver b = CreateB();

        public Round GetCurRound()
        {
            WaitTimer();
            var rates = b.FindElement(By.ClassName(@"users-rates"));
            double redRates = double.Parse(rates.FindElement(By.XPath(@"div/div/div[2]/span[2]/span[2]")).Text.Replace(" ", "").Replace(".", ","));
            double greenRates = double.Parse(rates.FindElement(By.XPath(@"div[2]/div/div[2]/span[2]/span[2]")).Text.Replace(" ", "").Replace(".", ","));
            double blackRates = double.Parse(rates.FindElement(By.XPath(@"div[3]/div/div[2]/span[2]/span[2]")).Text.Replace(" ", "").Replace(".", ","));
            return new Round(redRates, greenRates, blackRates, Result.green, DateTime.Now);
        }

        private int WaitTimer()
        {
            int timer = FindTimer();
            while (timer < 0 || timer > 5)
            {
                Thread.Sleep(1000);
                timer = FindTimer();
            }
            return timer;
        }

        public void WaitChangeHistory()
        {
            var lastHistory = FindHistory();
            var curHistory = FindHistory();
            while(EqualArray(lastHistory, curHistory))
            {
                Thread.Sleep(1000);
                curHistory = FindHistory();
            }
        }

        private int[] FindHistory()
        {
            return b.FindElement(By.ClassName(@"make-rate__history")).Text.Split(Environment.NewLine).Select(x => int.Parse(x)).ToArray();
        }

        private int FindTimer()
        {
            if (int.TryParse(b.FindElement(By.ClassName(@"roulette-timer")).Text, out int res))
                return res;
            return -1;
        }
        private static ChromeDriver CreateB()
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
    }
}
