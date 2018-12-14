using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace form
{
    public struct DatabaseInfo
    {
        public string DBUser;
        public string DBPWD;
        public string ServerName;
        public string DBName;
        public string DBPort;
        public string ConnectionString;
    }
    public struct Details
    {
        public string pid;
        public string app;
        public string ver;
        public string cmp;
        public string brc;
        public string eml;
        public string phn;
        
    }        

    class clsDBConnection
    {
        MySqlConnection conn = new MySqlConnection();       
        
        public static string[] valueArray=null,keyArray=null;
        public static string password=null, errorstring;

        public static string CreateConnectionString(DatabaseInfo dbInfo)
        {
            return "uid=root"  + "; password=metalic" + "; host = " + dbInfo.ServerName + "; database=" + dbInfo.DBName + "; port = " + dbInfo.DBPort;
        }
      /*  public static string getcondition(MySqlConnection conn, ref string errorString)
        {
            string custid = "";

            string strSql = "select customerid from softwareinfo";
            MySqlCommand Com = new MySqlCommand();
            MySqlDataReader reader;
            Com.Connection = conn;
            try
            {
                Com.CommandText = strSql;
                reader = Com.ExecuteReader();
                while (reader.Read())
                {
                    custid = reader["customerid"].ToString();

                }
                reader.Close();
            }
            catch (MySqlException sqlEx)
            {
                errorString = sqlEx.Message;

            }
            return custid;
            Com.Dispose();
        }     */
    
        public static Details getInformation(MySqlConnection conn)
        {
            Details detail = new Details();

            string str = "select c.name as name ,max(l.ver) as version,s.defaultbranch,b.email,b.phone1 ";
            str += "from companymaster c join logsoftwareupdate l join setup s inner join branchmaster b on s.defaultbranch = b.code";
            

            MySqlCommand com = new MySqlCommand(str, conn);
            MySqlDataReader reader;
            com.Connection = conn;
            try
            {
                com.CommandText = str;
                reader = com.ExecuteReader();
                while (reader.Read())
                {
                   // detail.app = reader["name"].ToString();
                    detail.cmp = reader["name"].ToString();
                    detail.ver = reader["version"].ToString();
                    detail.brc = reader["defaultbranch"].ToString();
                    detail.eml = reader["email"].ToString();
                    detail.phn = reader["phone1"].ToString();
                    
                }
                reader.Close();
            }
            catch (SqlException ex)
            {
                errorstring = ex.Message;
            }
            com.Dispose();
            return detail;
        }
        public static Boolean updateTable(MySqlConnection conn,string cust,string seq)
        {
            string str = "";
            try
            {
                str = "UPDATE SOFTWAREINFO set CUSTOMERID=" + cust + ",SEQUENCEID=" + seq +"WHERE SLNO=1";
                MySqlCommand cmd = new MySqlCommand(str, conn);
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            catch(MySqlException ex)
            {
                errorstring = ex.Message;
                return false;
            }
            return true;
        }
    }
}
