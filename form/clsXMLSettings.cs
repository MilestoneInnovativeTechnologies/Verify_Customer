using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Configuration;



namespace form
{
    public struct xmlSettings
    {
        public string serverName;
        public string dbName;
        public string dbPort;
        public string userName;
        public string password;
        public string timer;
        public Boolean success;
        public string errorMessage;
    }
    public class clsXMLSettings
    {
        private string xmlPath, xmlSettingsFile;
        public static string Server,password,serial, HexKeyArrLen, HexValArrLen;
        public static string app, pid, cmp, brc, email, phn1, phn2, ver;
        public string connectionstring;
        public  KeyInfo[] data, keyinfo;
        
        
        protected DatabaseInfo DBInfo;
        public static Details data1; 
        
        MySqlConnection cnn = new MySqlConnection();
     
        public clsXMLSettings()
        {
        /*    xmlPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\" + Application.ProductName;
            xmlSettingsFile = xmlPath + "\\Settings.xml";       */

              xmlPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
              string[] subDirectory = Directory.GetDirectories(xmlPath);
              for (int ini = 0; ini < subDirectory.Length - 1; ini++)
              {
                  var dirName = subDirectory[ini];
                  if (dirName.StartsWith("C:\\ProgramData\\ePlus"))
                  {
                      if (dirName == xmlPath + "\\ePlus Standard Edition")
                      {
                          DataSet ds = new DataSet();
                          xmlSettingsFile = dirName + "\\Settings.xml";
                          ds.ReadXml(xmlSettingsFile);
                        for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                        {
                            DBInfo.ServerName = ds.Tables[0].Rows[i]["SERVER"].ToString();
                            DBInfo.DBName = ds.Tables[0].Rows[i]["DATABASE"].ToString();
                            DBInfo.DBPort = ds.Tables[0].Rows[i]["PORT"].ToString();

                            break;
                        }
                        break;
                      }
                  }
                  else if (dirName.StartsWith("C:\\ProgramData\\EQL"))
                  {
                      if (dirName == xmlPath + "\\EQL Plus Corporate Edition" || dirName == xmlPath + "\\EQL Plus Enterprise Edition" || dirName == xmlPath + "\\EQL Plus Professional Edition")
                      {
                          DataSet ds = new DataSet();
                          xmlSettingsFile = dirName + "\\Settings.xml";
                          ds.ReadXml(xmlSettingsFile);
                          for (int i = 0; i <= ds.Tables[0].Rows.Count - 1; i++)
                          {
                              DBInfo.ServerName = ds.Tables[0].Rows[i]["SERVER"].ToString();
                              DBInfo.DBName = ds.Tables[0].Rows[i]["DATABASE"].ToString();
                              DBInfo.DBPort = ds.Tables[0].Rows[i]["PORT"].ToString();

                              break;
                          }
                          break;
                      }
                }                
            }            
        }
        public xmlSettings ReadSettings()
        {
            xmlSettings settings = new xmlSettings();
            if (!File.Exists(xmlSettingsFile))
            {
                settings.success = false;
                settings.errorMessage = "File not found (" + xmlSettingsFile + ")";
                return settings;
            }
            string xmlElement = "";
            XmlTextReader reader = new XmlTextReader(xmlSettingsFile);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        xmlElement = reader.Name;
                        //MessageBox.Show(objXmlTextReader.Name);
                        break;
                    case XmlNodeType.Text:
                        switch (xmlElement)
                        {
                            case "SERVER":
                                settings.serverName = reader.Value;
                                Server = settings.serverName;
                                break;
                            case "DATABASE":
                                settings.dbName = reader.Value;
                                break;
                            case "PORT":
                                settings.dbPort = reader.Value;
                                break;
                            case "USER":
                                settings.userName = reader.Value;
                                break;
                            case "PWD":
                                settings.password = reader.Value;
                                break;
                            case "TIMER":
                                settings.timer = reader.Value;
                                break;
                        }
                        break;
                }
            }
            reader.Close();
            settings.success = true;
            settings.errorMessage = "";
            return settings;
        }

        public string getServerName(DataRow currentRow)
        {
            return currentRow["SERVERNAME"].ToString();
        }
        public string getDBName(DataRow currentRow)
        {
            return currentRow["DBNAME"].ToString();
        }
        public string getDBPort(DataRow currentRow)
        {
            return currentRow["PORT"].ToString();
        }
        public string getCompanyCode(DataRow currentRow)
        {
            return currentRow["COCODE"].ToString();
        }
        public string getBranchCode(DataRow currentRow)
        {
            return currentRow["BRCODE"].ToString();
        }
        public string getFiscalYearCode(DataRow currentRow)
        {
            return currentRow["FYCODE"].ToString();
        }           
    }
}


