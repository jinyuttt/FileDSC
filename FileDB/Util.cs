using BerkeleyDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDB
{
   public class Util
    {
      public  static void dbtFromString(DatabaseEntry dbt, string s)
        {
            dbt.Data = System.Text.Encoding.ASCII.GetBytes(s);
        }

        public static string strFromDBT(DatabaseEntry dbt)
        {

            System.Text.ASCIIEncoding decode =
                new ASCIIEncoding();
            return decode.GetString(dbt.Data);
        }

        public static string reverse(string s)
        {
            StringBuilder tmp = new StringBuilder(s.Length);
            for (int i = s.Length - 1; i >= 0; i--)
                tmp.Append(s[i]);
            return tmp.ToString();
        }

    }
}
