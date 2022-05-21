using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using NeuralTools;
using static NeuralTools.Funcs;

namespace ComparedPredictor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var nets = ReadNets();
            var rs = DownloadRounds(5);
            Console.WriteLine(String.Join(Environment.NewLine, rs));
        }
    }
}
