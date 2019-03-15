using System;
using System.Collections.Generic;
using System.Text;
using Adf;

namespace AdfConsoleTest
{
    public class DnsHelperTest
    {
        public void Test()
        {
            var domain = "163.com";

            var list = DnsHelper.GetMXRecordList(domain);

            if (list == null)
            {
                Console.WriteLine("query failure");
            }
            else
            {
                foreach (var st in list)
                {
                    Console.WriteLine("MX:\t{0}\t{1}\t{2}\t{3}\n", domain, st.TTL, st.Preference, st.Value);
                }
            }

            Console.ReadLine();
        }
    }
}
