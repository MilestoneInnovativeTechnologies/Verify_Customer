using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace form
{
   
    public class clsWeb
    {
        public string connectionstring, KeyEncoded, InputText, errorstring;
        public struct Section
        {
            public string custid;
            public string seqno;
            public string prd;
            public string edn;
            public string version;

        }
        public clsWeb(xmlSettings xmlsetting)
        {
            settings = xmlsetting;
            KeyEncoded = begin();
            //DecryptedValue = Decode();         //Testing
        }

        public clsWeb()
        {
        }

        public static string responseFromServer;
        public static string url;
        protected DatabaseInfo DBInfo;
        public KeyInfo[] data; 
        public static Section sect;
        public static Details data1;
        public xmlSettings settings;
        
        public static Boolean DecryptedValue,Verified_Customer;
        public static string Server, password, serial, HexKeyArrLen, HexValArrLen;
        public static string app, pid, cmp, brc, email, phn1, phn2, ver;
        public static string Hdk, prs, ops, com, dbn,Date;
        public static string code, codeString, ArrayString = null, MergeString, KeyValueMerged, Code, response, mergedstring, custid;
        private bool decrypt=true,smallarrlength=true;
        public string Encodedkey, Encodedvalue, KeyArrayString, ValueArrayString;
        public static int keyArrayLength, valueArrayLength, MergedLength, intNum, i = 2;

        public string[] keyArray;
        public string[] valueArray;
        public string[] CodeArray;
        public string[] keyName;
        public static string[] codeArray;
        public static string[] valueArray1 = new string[500];
        public static string[] codeStringArray = new string[100];
        public static int randomNumber;
        Random r = new Random();
        

        public string begin()
        {
            randomNumber = r.Next(2, 5);          
           
            try
            {
                DBInfo.ServerName = settings.serverName;
                DBInfo.DBName = settings.dbName;
                DBInfo.DBPort = settings.dbPort;
                DBInfo.DBUser = settings.userName;
                DBInfo.DBPWD = settings.password;
                password = DBInfo.DBName;

                connectionstring = clsDBConnection.CreateConnectionString(DBInfo);
                MySqlConnection cnn = new MySqlConnection(connectionstring);
                cnn.Open();
                data1 = clsDBConnection.getInformation(cnn);

                Hdk = clsKeyInfo.getKeySerial (cnn,password,"H01",decrypt);
                prs = clsKeyInfo.getKeySerial(cnn, password, "H02", decrypt);
                ops = clsKeyInfo.getKeyName(cnn, password, "S01", decrypt);
                com = clsKeyInfo.getKeyName(cnn, password, "S02", decrypt);
                app = clsKeyInfo.getKeyName(cnn, password, "S03", decrypt);
                dbn = clsKeyInfo.getKeyName(cnn, password, "S04", decrypt);
                Date = clsKeyInfo.getKeySerial(cnn, password, "S06", decrypt);
                pid = clsKeyInfo.getKeySerial(cnn, password, "S07", decrypt);

                sect = getDetail(cnn, ref errorstring);
                
                if (string.IsNullOrEmpty(sect.custid)==true)
                {
                   Verified_Customer=false;
                }
                else
                {
                    Verified_Customer = true;
                }
                Date = Date.Substring(0, 8);
                DateTime dt;
                string[] format = { "yyyyMMdd", "yyyy-MM-dd" };
               DateTime.TryParseExact(Date, format, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt);
               string date = dt.ToString();
               date = date.Substring(0, date.IndexOf(" "));     

                ////////Specifying KeyArray and ValueArray for  stage1 and stage2////////

                if (!Verified_Customer)            // Stage1
                {
                    keyArray = new string[] { "pid", "cmp", "brc", "app", "ver", "eml", "phn", "hdk", "prs", "ops", "com", "dbn", "isd" };
                    valueArray = new string[] { pid, data1.cmp, data1.brc, app, data1.ver, data1.eml, data1.phn, Hdk, prs, ops, com, dbn, date };
                }
                else                      //Stage 2
                {
                 //   sect = getDetail(cnn, ref errorstring);
                    keyArray = new string[] { "cus", "seq","prd","edn", "ver" };
                    valueArray = new string[] { sect.custid, sect.seqno, sect.prd, sect.edn, sect.version };
                }
                for (int i = 0; i < valueArray.Length; i++)
                {
                    if (string.IsNullOrEmpty(valueArray[i]))
                    {
                        valueArray[i] = "0000";
                    }
                }
                /////////////////////////////////////////

                KeyArrayString = clsWeb.AppendArrayToString(keyArray);
                ValueArrayString = clsWeb.AppendArrayToString(valueArray);
                Encodedkey = clsWeb.Base64Encode(KeyArrayString);
                Encodedvalue = clsWeb.Base64Encode(ValueArrayString);               
                keyArray = clsWeb.splitByLength(Encodedkey, randomNumber);
                valueArray = clsWeb.splitByLength(Encodedvalue, randomNumber);
                keyArrayLength = keyArray.Length;
                valueArrayLength = valueArray.Length;
                KeyValueMerged = clsWeb.MergeKeyValueArray(keyArray, valueArray);
                MergedLength = KeyValueMerged.Length;
                intNum = MergedLength / 3;
                if (intNum > 15)
                {
                    intNum = 11;
                }

                string HexintNum = intNum.ToString("x");                             // Converting into hexadecimal
                string[] stringArray = clsWeb.splitByIntNum(KeyValueMerged, intNum);
                HexKeyArrLen = clsWeb.convertKeyToHex(keyArrayLength);
                HexValArrLen = clsWeb.convertValueToHex(valueArrayLength);
                CodeArray = clsWeb.generateCodeArray(intNum, HexintNum, stringArray, HexKeyArrLen, HexValArrLen);
                int len = CodeArray.Length;
                string Code = clsWeb.generateCode(CodeArray);

            }
            catch(Exception ex)
            {
                string error = ex.Message;
                return "";
            }
            return code;
        }
        public Boolean Decode()
        {
            string[] keyarray = new string[100];
            string[] valuearray = new string[100];

            int j = 0, k = 0, startindex ;
            response = clsWeb.responseFromServer;
            // response = "bY3Q1VzVTfHV2DNlAwcXMDxwagExcmMnR8wxZWfFRuBSRDAwMXxFRE4wMDY=h12";      //testing
             //response = "dZmlycYXxi$3Rf5bmFtZXxzZWNvb7gmRfbmFtZQ==$$$h1";                       //test

            if (!((response=="0")|| (response == "1")|| (response == null)))
            {
               clsXMLSettings clsxml = new clsXMLSettings();
                try
                {
                    settings = clsxml.ReadSettings();
                    DBInfo.ServerName = settings.serverName;
                    DBInfo.DBName = settings.dbName;
                    DBInfo.DBPort = settings.dbPort;
                    DBInfo.DBUser = settings.userName;
                    DBInfo.DBPWD = settings.password;
                    password = DBInfo.DBName;

                    connectionstring = clsDBConnection.CreateConnectionString(DBInfo);
                    MySqlConnection cnn = new MySqlConnection(connectionstring);
                    cnn.Open();
                    if (Verified_Customer)
                    {
                        clsDBConnection.updateVersion(cnn, response);        
                        return true;
                    }
                    else
                    {
                    int len = response.Length;
                    string[] stringArray = clsWeb.splitStringToArray(response);
                    string[] stringArr = { stringArray[1], stringArray[3], stringArray[6] };
                    keyArrayLength =Convert.ToInt32(stringArray[4]);
                    valueArrayLength = Convert.ToInt32(stringArray[8]);
                    mergedstring = clsWeb.generateCode(stringArr);
                    int random= Int32.Parse(stringArray[2].ToString());

                    string[] array = clsWeb.splitByIntNum(mergedstring, random);

                        //// Split an Array into two arrays ////
                   if(keyArrayLength<valueArrayLength)
                   {
                        startindex = 0;  
                        keyarray = smallStrArray(startindex,array,keyArrayLength);
                        valuearray= largeStrArray(startindex+1,array,keyArrayLength);
                   }
                   else if(keyArrayLength>valueArrayLength)
                   {
                         startindex = 1;
                         keyarray = largeStrArray(startindex-1,array, valueArrayLength);
                         valuearray = smallStrArray(startindex,array, valueArrayLength);
                    }

                        ///////////////////////// 

                        //// Find last element from array  and if "$" exist , Remove it    ////

                    keyarray = removeLastEle(keyarray.Last(),keyarray);
                    valuearray = removeLastEle(valuearray.Last(),valuearray);
                 
                    ////////////////////////

                    string keystring = clsWeb.generateCode(keyarray);
                    string valuestring = clsWeb.generateCode(valuearray);
                    string keys = clsWeb.Base64Decode(keystring);
                    string values = clsWeb.Base64Decode(valuestring);
                    string[] keysArr = clsWeb.decodedArray(keys);
                    string[] valuessArr = clsWeb.decodedArray(values);
                    string cust = getValue(keysArr, valuessArr, "cus");
                    string seq = getValue(keysArr, valuessArr, "seq");
                    string prd = getValue(keysArr, valuessArr, "prd");
                    string edn = getValue(keysArr, valuessArr, "edn");

                    clsDBConnection.updateTable(cnn, cust, seq,prd,edn);
                    }
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                    return false;
                }
            }
            return true;
        }
        public static Section getDetail(MySqlConnection conn, ref string errorString)
        {
            Section section = new Section();
            string strSql = "select customerid,sequenceid,productid,editionid,max(ver) as verno  from softwareinfo join logsoftwareupdate ";
           // string strSql = "select customerid,sequenceid,max(ver) as verno  from softwareinfo join logsoftwareupdate ";
            MySqlCommand Com = new MySqlCommand();
            MySqlDataReader reader;
            Com.Connection = conn;
            try
            {
                Com.CommandText = strSql;
                reader = Com.ExecuteReader();
                while (reader.Read())
                {
                    section.custid = reader["customerid"].ToString();
                    section.seqno = reader["sequenceid"].ToString();
                    section.prd = reader["productid"].ToString();
                    section.edn = reader["editionid"].ToString();
                    section.version = reader["verno"].ToString();
                }
                reader.Close();
            }
            catch (MySqlException sqlEx)
            {
                errorString = sqlEx.Message;
            }
            return section;
            Com.Dispose();
        }
    /*   public static string[] StrArray(int index,string[] arr, int length,bool key)
        {
            string[] array = new string[100];
            int j = 0;
            if (key == true)
            {
                for (int i = index; i < arr.Length;i+=2)
                {
                    if (j < length)
                    {
                        array[j] = arr[i];
                        j++;
                    }
                    if (j == length)
                    {
                        index = i + 1;
                        break;
                    }
                }
            }
            else
            {
                for (int i = index; i < arr.Length; i += 2)
                {
                    if(j<length)
                    {
                        array[j] = arr[i];
                        j++;
                    }                    
                }
            }
            return array;
        }           */
        public static string[] smallStrArray(int startindex,string[] arr, int length)
        {
            string[] array = new string[length];
            int j = 0;
            for (int i = startindex; i < arr.Length;)
            {

                if (j <length)

                {
                    array[j] = arr[i];
                    j++;
                }
                i+=2;
            }

          return array;
        }
        public static string[] largeStrArray(int startindex,string[] arr, int keylength)
        {
            string[] array = new string[50];
            int j = 0;
            int index=0;
            for (int i = startindex; i < arr.Length; i += 2)
            {
                if (j < keylength)
                {
                    array[j] = arr[i];
                    j++;
                }
                if (j == keylength)
                {
                    if (startindex == 1)
                        index = i + 1;
                    else
                        index = i + 2;
                    break;
                }
            }
            
            for (int k=index; k <arr.Length; k ++)
            {
                array[j] = arr[k];
                    j++;
            }
            array = array.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            return array;
        }
        public static string getValue(string[] arr1, string[] arr2, string strn)
        {
            int index = 0;
            index = Array.IndexOf(arr1,strn);
            string cust = arr2[index];

            return cust;
        }
        public static string AppendArrayToString(string[] Array)
        {
            ArrayString = null;
            for (int i = 0; i < Array.Length; i++)
            {
                ArrayString += Array[i];
                if (i < Array.Length - 1)
                    ArrayString += "|";
            }
            return ArrayString;
        }
        public static string MergeKeyValueArray(string[] keyArray, string[] valueArray)
        {
            int i = 0, j = 0, n;
            int maxIndex = keyArray.Length;

            while (i == j)
            {
                if (i != maxIndex)
                {
                    MergeString += keyArray[i] + valueArray[j];
                    i++; j++;
                }
                if (i == maxIndex)
                    break;
            }
            n = valueArray.Length - keyArray.Length;

            Array.Copy(valueArray, i, valueArray1, 0, n);                              //Copy values from one array to another array
            valueArray1 = valueArray1.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            for (i = 0; i <= valueArray1.Length - 1; i++)
                MergeString += valueArray1[i];
            return MergeString;
        }
        public static string[] splitByLength(string str, int chunksize)
        {
            string[] substr = new string[500];
            int len = str.Length;
            while (!String.IsNullOrEmpty(str))
            {
                for (int j = 0; j <= len; j++)
                {
                    if (str.Length < chunksize)
                    {
                        for (int i = str.Length + 1; i <= chunksize; i++)
                        {
                            str += "$";
                        }
                    }
                    substr[j] = str.Substring(0, chunksize);
                    str = str.Remove(0, chunksize);
                    if (str == "")
                    {
                        substr = substr.Where(x => !string.IsNullOrEmpty(x)).ToArray();   //Remove the blank values in the array
                        return substr;
                    }
                }
            }
            return substr;
        }
        public static string[] splitByIntNum(string str, int chunksize)
        {
            string[] substr = new string[200];
            int len = str.Length;
            while (!String.IsNullOrEmpty(str))
            {
                for (int j = 0; j <= len; j++)
                {

                    if (str.Length < chunksize)
                    {
                        substr[j] = str;
                        str = str.Remove(0, str.Length);
                    }
                    else
                    {
                        substr[j] = str.Substring(0, chunksize);
                        str = str.Remove(0, chunksize);
                    }

                    if (str == "")
                    {
                        substr = substr.Where(x => !string.IsNullOrEmpty(x)).ToArray();   //Remove the blank values in the array
                        return substr;
                    }
                }
            }
            return substr;
        }
        /// <summary>
        /// ///Decoding
        /// </summary>
        public static string[] splitStringToArray(string str)
        {
            string[] strArray = new string[9];
            int Num = Convert.ToInt32(str.Substring(0, 1), 16);
            strArray[0] = Num.ToString(); str = str.Remove(0, 1);
            strArray[1] = str.Substring(0, Num); str = str.Remove(0, Num);
            strArray[2] = str.Substring(0, 1); str = str.Remove(0, 1);
            strArray[3] = str.Substring(0, Num); str = str.Remove(0, Num);
            strArray[4] = Convert.ToInt32(str.Substring(0, str.IndexOf("g")), 16).ToString(); str = str.Remove(0, str.IndexOf("g"));
            strArray[5] = str.Substring(0, str.IndexOf("g") + 1); str = str.Remove(0, str.IndexOf("g") + 1);
            strArray[6] = str.Substring(0, str.LastIndexOf("h")); str = str.Remove(0, str.LastIndexOf("h"));
            strArray[7] = str.Substring(0, str.IndexOf("h") + 1); str = str.Remove(0, str.IndexOf("h") + 1);
            strArray[8] = str.Substring(0, str.Length); str = str.Remove(0, str.Length);

            return strArray;
        }
        public static string[] removeLastEle(string lastkey,string[] array)
        {
            if (lastkey.Contains("$"))
            {
                lastkey = lastkey.Remove(lastkey.IndexOf("$"), (lastkey.LastIndexOf("$") + 1) - (lastkey.IndexOf("$")));
                array[array.Length - 1] = lastkey;
                

            }
            return array;
        }

        public static string[] decodedArray(string str)
        {
            int pos = 0, i = 0, len = str.Length;
            string[] Arraydecoded = new string[100];
            for (i = 0; i <= len; i++)
            {
                if (str.Contains("|"))
                {
                    Arraydecoded[i] = str.Substring(pos, str.IndexOf("|"));
                    str = str.Remove(pos, str.IndexOf("|") + 1);
                }
                else
                {
                    Arraydecoded[i] = str.Substring(pos, str.Length);
                    str = str.Remove(pos, str.Length);
                }
                if (str == "")
                {
                    Arraydecoded = Arraydecoded.Where(x => !string.IsNullOrEmpty(x)).ToArray();   //Remove the blank values in the array
                    return Arraydecoded;
                }
            }
            return Arraydecoded;
        }
        public static string convertKeyToHex(int keyArrayLength)
        {
            string HexKeyArrayLength = keyArrayLength.ToString("x");                  // Converting into hexadecimal
            return HexKeyArrayLength;
        }
        public static string convertValueToHex(int valueArrayLength)
        {
            string HexvalueArrayLength = valueArrayLength.ToString("x");              // Converting into hexadecimal
            return HexvalueArrayLength;
        }
        public static string[] generateCodeArray(int intNum, string HexintNum, string[] stringArray, string HexKeyArrayLength, string HexvalueArrayLength)
        {
            Array.Copy(stringArray, 2, codeStringArray, 0, stringArray.Length - 2);    //Copy values from one array to another array
            for (int i = 0; i < codeStringArray.Length; i++)
                codeString += codeStringArray[i];
            string[] codeArray = { HexintNum, stringArray[0], randomNumber.ToString(), stringArray[1], HexKeyArrayLength, "g", codeString, "h", HexvalueArrayLength };

            return codeArray;
        }
        public static string generateCode(string[] codeArray)
        {
            code = "";
            for (int i = 0; i <= codeArray.Length - 1; i++)
                code += codeArray[i];
            return code;
        } 

        public static void sendDataToSite()
        {
             clsWeb web = new clsWeb();

            url = "http://milestoneit.net/api/appinit/" + clsWeb.code;
            // Create a request using a URL that can receive a post. 
            HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(url);
            webreq.Method = "GET";
            webreq.ContentType = "Text";
            HttpWebResponse webresp = (HttpWebResponse)webreq.GetResponse();
            // Get the stream associated with the response.
            Stream receiveStream = webresp.GetResponseStream();
            // Pipes the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            var encoding = ASCIIEncoding.ASCII;
            using (var reader = new System.IO.StreamReader(webresp.GetResponseStream(), encoding))
            {
                  responseFromServer = reader.ReadToEnd();
                 
            }
            web.Decode();
            webresp.Close();
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
