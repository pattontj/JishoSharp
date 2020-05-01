using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;

using JishoSharp;


namespace BuildTest
{

    class Program
    {
        static async Task Main(string[] args)
        {
            var test = await Jisho.Query("jlpt-n5", QueryType.Tagged);

            Console.WriteLine(test.Data.ElementAt(0).Slug);
            


            while (true) { }
  //          Console.WriteLine( test.Meta );
   //         Console.WriteLine( test.Data.Count );

        }
    }
}
