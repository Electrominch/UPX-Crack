using System;
using NeuralTools;
using static NeuralTools.Funcs;

namespace Counter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var rs = DownloadRounds();
            Console.WriteLine(rs.Count);
            double allRates = 0;
            double upxWin = 0;
            double upxLose = 0;
            foreach(var r in rs)
            {
                allRates += r.Red + r.Green + r.Black;
                switch (r.Result)
                {
                    case Result.red:
                        upxWin += r.Green + r.Black;
                        upxLose += r.Red;
                        break;
                    case Result.green:
                        upxWin += r.Red + r.Black;
                        upxLose += 13 * r.Green;
                        break;
                    case Result.black:
                        upxWin += r.Red + r.Green;
                        upxLose += r.Black;
                        break;

                }
            }
            Console.WriteLine($"Win:{upxWin}\t Lose:{upxLose}");
            Console.WriteLine($"AllWin:{upxWin-upxLose}");
            Console.WriteLine($"Rates: {allRates}");
        }
    }
}
