using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.SQLite;
using System.IO;
using FileBasedDBApplication.Models;

namespace FileBasedDBApplication
{
    public class DatabaseAccess
    {
        private string connection = string.Empty;
        SQLiteConnection con = null;
        SQLiteCommand cmd = null;
        public DatabaseAccess()
        {
            string DbPath = ConfigurationManager.AppSettings["DataBasePath"].ToString();
            if (!string.IsNullOrWhiteSpace(DbPath))
            {
                connection = "data source =" + DbPath + "\\" + "KeyValueJSon.db;";
            }
            else
            {
                connection = "data source =" + AppDomain.CurrentDomain.BaseDirectory + "KeyValueJSon.db;";
            }
            CreateDatabase();
        }

        private void CreateDatabase()
        {

            if (!string.IsNullOrWhiteSpace(connection))
            {


                if (!File.Exists(connection))
                {
                    try
                    {
                        using (con = new SQLiteConnection(connection))
                        {
                            string sql = "create table JsonStorageTable (KEY varchar(32) PRIMARY KEY, Value varchar(2048),TimeToLive int,CreatedDate DateTime, Active bit);";

                            using (cmd = new SQLiteCommand(sql, con))
                            {
                                con.Open();
                                cmd.ExecuteNonQuery();
                            }

                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }
        public bool InserJsonData(JsonStorageTable data)
        {
            bool result = false;

            try
            {
                using (con = new SQLiteConnection(connection))
                {
                    string sql = "insert into JsonStorageTable(KEY,VALUE,TimeToLive,CreatedDate,Active) VALUES(@KEY,@VALUE,@TimeToLive,@CreatedDate,@Active)";

                    using (cmd = new SQLiteCommand(sql, con))
                    {
                        con.Open();
                        SQLiteTransaction T1 = con.BeginTransaction();
                        try
                        {
                            SQLiteParameter keyParam = new SQLiteParameter();
                            keyParam.ParameterName = "KEY";
                            keyParam.Value = data.Key;
                            cmd.Parameters.Add(keyParam);

                            SQLiteParameter valueParam = new SQLiteParameter();
                            valueParam.ParameterName = "VALUE";
                            valueParam.Value = data.Value;
                            cmd.Parameters.Add(valueParam);

                            SQLiteParameter TimeToLiveParam = new SQLiteParameter();
                            TimeToLiveParam.ParameterName = "TimeToLive";
                            TimeToLiveParam.Value = data.TimeToLive;
                            cmd.Parameters.Add(TimeToLiveParam);


                            SQLiteParameter CreatedDateParam = new SQLiteParameter();
                            CreatedDateParam.ParameterName = "CreatedDate";
                            CreatedDateParam.DbType = System.Data.DbType.DateTime;
                            CreatedDateParam.Value = DateTime.Now;
                            cmd.Parameters.Add(CreatedDateParam);

                            SQLiteParameter ActiveParam = new SQLiteParameter();
                            ActiveParam.ParameterName = "Active";
                            ActiveParam.DbType = System.Data.DbType.Boolean;
                            ActiveParam.Value = true;
                            cmd.Parameters.Add(ActiveParam);

                            result = cmd.ExecuteNonQuery() > 0;
                            T1.Commit();
                        }
                        catch (Exception ex)
                        {
                            T1.Rollback();
                            con.Dispose();
                            cmd.Dispose();
                        }
                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public List<JsonStorageTable> GetAllJSonDatas()
        {
            List<JsonStorageTable> listOfJsons = new List<JsonStorageTable>();

            try
            {
                using (con = new SQLiteConnection(connection))
                {
                    string sql = "select * from JsonStorageTable";

                    using (cmd = new SQLiteCommand(sql, con))
                    {
                        con.Open();
                        SQLiteDataReader rd = cmd.ExecuteReader();
                        JsonStorageTable oJsonStorageTable = null;
                        while (rd.Read())
                        {
                            oJsonStorageTable = new JsonStorageTable();
                            oJsonStorageTable.Key = rd["KEY"].ToString();                            
                            oJsonStorageTable.TimeToLive = int.Parse(rd["TimeToLive"].ToString());
                            oJsonStorageTable.CreatedDate = DateTime.Parse(rd["CreatedDate"].ToString());
                            if (oJsonStorageTable.CreatedDate.AddSeconds(int.Parse(rd["TimeToLive"].ToString())) > DateTime.Now)
                            {
                                oJsonStorageTable.Value = rd["Value"].ToString();
                                oJsonStorageTable.Active = Boolean.Parse(rd["Active"].ToString());
                            }
                            else
                            {
                                oJsonStorageTable.Active = false;
                            }
                            
                            listOfJsons.Add(oJsonStorageTable);
                        }
                    }

                }
                return listOfJsons;
            }
            catch (Exception ex)
            {
                return listOfJsons;
            }
        }

        public JsonStorageTable GetAJSonData(string Key)
        {
            JsonStorageTable oJsonStorageTable = new JsonStorageTable();

            try
            {
                using (con = new SQLiteConnection(connection))
                {
                    string sql = "select * from JsonStorageTable Where Active=1 and Key=@Key";

                    using (cmd = new SQLiteCommand(sql, con))
                    {
                        SQLiteParameter keyParam = new SQLiteParameter();
                        keyParam.ParameterName = "KEY";
                        keyParam.Value = Key;
                        cmd.Parameters.Add(keyParam);

                        con.Open();
                        SQLiteDataReader rd = cmd.ExecuteReader();

                        while (rd.Read())
                        {
                            DateTime createdDt = DateTime.Parse(rd["CreatedDate"].ToString());
                            oJsonStorageTable = new JsonStorageTable();
                            oJsonStorageTable.Key = rd["KEY"].ToString();
                            oJsonStorageTable.TimeToLive = int.Parse(rd["TimeToLive"].ToString());
                            oJsonStorageTable.CreatedDate = createdDt;
                            oJsonStorageTable.Active = Boolean.Parse(rd["Active"].ToString());

                            if (createdDt.AddSeconds(int.Parse(rd["TimeToLive"].ToString())) > DateTime.Now)
                            {
                                oJsonStorageTable.Value = rd["Value"].ToString();
                            }
                        }
                    }

                }
                return oJsonStorageTable;
            }
            catch (Exception ex)
            {
                return oJsonStorageTable;
            }
        }

        public bool DeleteKey(JsonStorageTable oJsonStorageTable)
        {
            bool result = false;

            if (oJsonStorageTable == null || oJsonStorageTable.CreatedDate.AddSeconds(oJsonStorageTable.TimeToLive) < DateTime.Now)
            {
                return result;
            }


            try
            {
                using (con = new SQLiteConnection(connection))
                {
                    string sql = "update JsonStorageTable set Active=0 Where Key=@Key";

                    using (cmd = new SQLiteCommand(sql, con))
                    {
                        SQLiteParameter keyParam = new SQLiteParameter();
                        keyParam.ParameterName = "KEY";
                        keyParam.Value = oJsonStorageTable.Key;
                        cmd.Parameters.Add(keyParam);

                        con.Open();
                        SQLiteTransaction T1 = con.BeginTransaction();
                        try
                        {
                            result = cmd.ExecuteNonQuery() > 0;
                            T1.Commit();
                        }
                        catch(Exception ex)
                        {
                            T1.Rollback();
                            con.Dispose();
                            cmd.Dispose();
                        }
                        
                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }
    }
}
