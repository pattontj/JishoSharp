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
            await jisho.QueryPages("jlpt-n5", QueryType.Tagged, 1, 3);

            Console.WriteLine("Data.Count = " + jisho.Data.Count);


    
            /*

            List<JishoQuery> array = new List<JishoQuery>();
            for (int i = 0; i < 2; i++)
            {
               array.Add(
                    await Jisho.Query("jlpt-n5", QueryType.Tagged, page: (uint)i + 1)
                    );

            }

            foreach (var thing in array)
            {
                Console.WriteLine(thing.Meta.Status);
            }
      */



            //Console.WriteLine(test.Data.ElementAt(0).Slug);



            while (true) { }
  //          Console.WriteLine( test.Meta );
   //         Console.WriteLine( test.Data.Count );

        }
    }
}
