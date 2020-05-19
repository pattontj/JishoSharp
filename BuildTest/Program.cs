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
            await jisho.QueryPages("jlpt-n5", QueryType.Tagged, 1, 4);

            Console.WriteLine("Data.Count = " + jisho.Pages.Count);
            #endregion

            #region GetPage Test
            {
                Console.WriteLine("GetPage test: ");
                var page = await Jisho.Query("jlpt-n5", QueryType.Tagged, 1);

                if (!page.Equals(jisho.GetPage(1)))
                {
                    Console.WriteLine("Failed");
                    throw new Exception("BuildTest Error: GetPage index doesn't align with page numbers");
                }
                Console.WriteLine("Passed \n");
            }
            #endregion

            #region QueryPages Test
            Console.WriteLine("QueryPages test: ");

            // TEST: Data Validity of query results and cached data

            for (int page = 1; page < jisho.PageRange.Last; page++)
            {
                var p = await Jisho.Query("jlpt-n5", QueryType.Tagged, (uint)page);

                if (!p.Equals(jisho.GetPage((uint)page)))
                {
                    Console.WriteLine("Failed");
                    throw new Exception("BuildTest Error: QueryPages cached data doesn't match static query");
                }

                // TEST: Size of Data member matches PageRange

                if (jisho.Pages.Count != jisho.PageRange.Last)
                {
                    Console.WriteLine("Failed; Pages.Count=" + jisho.Pages.Count + " : PageRange.Last=" + jisho.PageRange.Last);
                    throw new Exception("BuildTest Error: QueryPages returned length of Pages does not match PageRange.Last");
                }


                Console.WriteLine("Passed \n");
            }
            #endregion

            #region Datum Indexing
            {
                Console.WriteLine("Datum Indexing test: ");
                // Theoretically should contain datum entries 21-40
                var page = jisho.GetPage(1);
                if (page.Data.ElementAt(0) != jisho[1])
                {
                    Console.WriteLine("Failed");
                }
                Console.WriteLine("Passed \n");
            }
            #endregion





            // Currently using this to hang the program so output can be read
            while (true) { }
        }
    }
}
