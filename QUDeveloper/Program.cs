using System;
using System.Collections.Generic;
using System.Linq;

namespace QUDeveloper
{
    class Program
    {
        static void Main(string[] args)
        {
            var matrix = new List<string> { "ABCCC", "FGWOO", "CHILL", "PQNDD", "UVDXY" };
            var wordstream = new List<string> { "CHILL", "WIND", "COLD" };

            var result = new WordFinder(matrix).Find(wordstream);
            
            Console.WriteLine("Top 10 most found words are:");
            result.ToList().ForEach(r => Console.WriteLine(r));
            Console.ReadLine();
        }
    }
}
