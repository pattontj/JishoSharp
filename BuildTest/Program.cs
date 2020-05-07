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
            var jisho = new Jisho();
            jisho.QueryPages("jlpt-n5", QueryType.Tagged, 1, 2);

            Console.WriteLine("Data.Count = " + jisho.Data.Count);

            for (uint i = 0; i <= jisho.PageRange.Item2; i++)
            {

                var test = jisho.Get(i);
                Console.WriteLine( test.Data.Count() );
            }


            //Console.WriteLine(test.Data.ElementAt(0).Slug);
            


            while (true) { }
  //          Console.WriteLine( test.Meta );
   //         Console.WriteLine( test.Data.Count );

        }
    }
}
