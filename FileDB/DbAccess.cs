using BerkeleyDB;
using System;
using System.Collections.Generic;

namespace FileDB
{
    public class DbAccess
    {
        private static readonly string progName;
        BTreeDatabase btreeDB;
      
       static DatabaseEnvironment env;
        Cursor dbc;
      
       public string dbFileName;

        /// <summary>
        /// 初始化环境
        /// </summary>
        /// <param name="home"></param>
        /// <param name="data_dir"></param>
        /// <returns></returns>
        public static int SetUpEnv(string home, string data_dir)
        {
           
            DatabaseEnvironmentConfig envConfig;

            /* Configure an environment. */
            envConfig = new DatabaseEnvironmentConfig();
            envConfig.MPoolSystemCfg = new MPoolConfig();
            envConfig.MPoolSystemCfg.CacheSize = new CacheInfo(
                0, 64 * 1024, 1);
            envConfig.Create = true;
            envConfig.DataDirs.Add(data_dir);
            envConfig.CreationDir = data_dir;
            envConfig.ErrorPrefix = progName;
            envConfig.UseLogging = true;
            envConfig.UseLocking = true;
            envConfig.UseMPool = true;
            envConfig.UseTxns = true;

            /* Create and open the environment. */
            try
            {
                env = DatabaseEnvironment.Open(home, envConfig);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0}", e.Message);
                return 0;
            }
            return 1;
          
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public void Init()
        {
            /* Configure the database. */
           var btreeConfig = new BTreeDatabaseConfig();
            btreeConfig.Duplicates = DuplicatesPolicy.SORTED;
            btreeConfig.ErrorPrefix = "file_access";
            btreeConfig.Creation = CreatePolicy.IF_NEEDED;
            btreeConfig.CacheSize = new CacheInfo(0, 64 * 1024, 1);
            btreeConfig.PageSize = 8 * 1024;

            /* Create and open a new database in the file. */
            try
            {
                btreeDB = BTreeDatabase.Open(dbFileName, btreeConfig);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error opening {0}.", dbFileName);
                Console.WriteLine(e.Message);
                return;
            }

        

        }

        public DatabaseEntry  GetData(DatabaseEntry key)
        {
           return btreeDB.Get(key).Value;
        }
        public void Put(DatabaseEntry key, DatabaseEntry entry)
        {
             btreeDB.Put(key, entry);

        }

        public void Put(List<KeyValuePair<DatabaseEntry, DatabaseEntry>> kv)
        {
            Transaction txn = env.BeginTransaction();
            foreach (var p in kv)
            {
                btreeDB.Put(p.Key, p.Value,txn);
            }
            txn.Commit();
            txn = null;
        }

        public List<DatabaseEntry> GetMult(DatabaseEntry key)
        {
            var kv= btreeDB.GetMultiple(key);
            List<DatabaseEntry> lst = new List<DatabaseEntry>();
            foreach (var ss in kv.Value)
            {
                lst.Add(ss);
            }
            return lst;
        }
        public List<string> GetMult(string key)
        {
            var k = new DatabaseEntry();
            Util.dbtFromString(k, key);
            var kv = btreeDB.GetMultiple(k);
            List<string> lst = new List<string>();
            foreach (var ss in kv.Value)
            {
                lst.Add(Util.strFromDBT(ss));
            }
            return lst;
        }

        public void Delete(DatabaseEntry key, DatabaseEntry entry)
        {
            btreeDB.Delete(key);
        }

        public string GetData(string key)
        {
            var k = new DatabaseEntry();
            Util.dbtFromString(k, key);
            var v= btreeDB.Get(k).Value;
            return Util.strFromDBT(v);
        }

        public void Put(string key, string entry)
        {
            var k = new DatabaseEntry();
            Util.dbtFromString(k, key);
            var v = new DatabaseEntry();
            Util.dbtFromString(v, key);
            btreeDB.Put(k, v);

        }

        public void Delete(string key)
        {
            var k = new DatabaseEntry();
            Util.dbtFromString(k, key);
           
            btreeDB.Delete(k);

        }
    }
}
