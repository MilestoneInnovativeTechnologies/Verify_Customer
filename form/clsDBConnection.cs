﻿using System;
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
          public static Details getcondition(MySqlConnection conn)
          {
              Details detail = new Details();

              string str = "select max(ver) as version from logsoftwareupdate ";
            
              MySqlCommand com = new MySqlCommand(str, conn);
              MySqlDataReader reader;
              com.Connection = conn;
              try
              {
                  com.CommandText = str;
                  reader = com.ExecuteReader();
                while (reader.Read())
                {
                    detail.ver = reader["version"].ToString();
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

        public static Details getInformation(MySqlConnection conn)
        {
            Details detail = new Details();

            string str = "select c.name as name ,s.defaultbranch,b.email,b.phone1 ";
            str += "from companymaster c join setup s inner join branchmaster b on s.defaultbranch = b.code";
            

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
                   // detail.ver = reader["version"].ToString();
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
        public static Boolean updateTable(MySqlConnection conn,string cust,string seq,string prd,string edn)
        {
            string str = "";
            try
            {
                str = "UPDATE SOFTWAREINFO set CUSTOMERID='" + cust + "',SEQUENCEID='" + seq + "',PRODUCTID='" + prd + "',EDITIONID='" + edn + "' WHERE SLNO=1";  
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
        public static Boolean updateVersion(MySqlConnection conn, string ver)
        {
            string str = "";
            try
            {
                str = "UPDATE SOFTWAREINFO set VERSION='" + ver + "' WHERE SLNO=1";
                MySqlCommand cmd = new MySqlCommand(str, conn);
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                errorstring = ex.Message;
                return false;
            }
            return true;
        }
    }
}
