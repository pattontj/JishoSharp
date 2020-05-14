using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json;

using JishoSharp;


/*
    BuildTest is a series of quick tests to run the library code, and can double as
    a scratchpad to test new features or ideas.
*/

namespace BuildTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            #region Initial Data Query
            var jisho = new Jisho();
            await jisho.QueryPages("jlpt-n5", QueryType.Tagged, 1, 34);

            Console.WriteLine("Data.Count = " + jisho.Data.Count);
            #endregion


            #region GetPage Test
            {
                Console.Write("GetPage test: ");
                var page = await Jisho.Query("jlpt-n5", QueryType.Tagged, 1);
                if (page != jisho.GetPage(1))
                {
                    Console.WriteLine("Failed");
                    throw new Exception("BuildTest Error: GetPage index doesn't align with page numbers");
                }
                Console.WriteLine("Passed");
            }
            #endregion

            #region QueryPages Test
            Console.Write("QueryPages test: ");

            // TEST: Data Validity of query results and cached data

            for (int page = 1; page < jisho.PageRange.Last; page++)
            {
                var p = await Jisho.Query("jlpt-n5", QueryType.Tagged, (uint)page);

                if (p != jisho.GetPage((uint)page))
                {
                    Console.WriteLine("Failed");
                    throw new Exception("BuildTest Error: QueryPages cached data doesn't match static query");
                }

                // TEST: Size of Data member matches PageRange

                if (jisho.Data.Count != jisho.PageRange.Last)
                {
                    Console.WriteLine("Failed; Data.Count=" + jisho.Data.Count + " : PageRange.Last=" + jisho.PageRange.Last);
                    throw new Exception("BuildTest Error: QueryPages returned length of Data does not match PageRange.Last");
                }


                Console.WriteLine("Passed");
            }
            #endregion


            while (true) { }
  
        }
    }
}
