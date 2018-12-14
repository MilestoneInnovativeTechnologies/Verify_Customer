using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Net;

namespace form
{
    public struct KeyInfo
    {
        public string code;
        public string category;
        public decimal slno;
        public string name;
        public string serial;
        public decimal strength;
    }
    class clsKeyInfo
    {
     
        public static string getKeyName(MySqlConnection MySqlDatabaseConnection,string password,string category,Boolean decrypt=true)
        {
            string name = "";
            MySqlCommand Com = new MySqlCommand();
            Com.Connection = MySqlDatabaseConnection;
       
            try
            {
                Com.CommandText = "SELECT NAME FROM KEYINFO WHERE CATEGORY='" + category + "'";

                if (decrypt)
                {
                    name = DecryptString((string)Com.ExecuteScalar(), password);
                }
                else
                {
                    name = (string)Com.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            Com.Dispose();
            return name; ;
        }
        public static string getKeySerial(MySqlConnection MySqlDatabaseConnection, string password, string category, Boolean decrypt = true)
        {
            string serial = "";
            MySqlCommand Com = new MySqlCommand();
            Com.Connection = MySqlDatabaseConnection;
      
            try
            {
                Com.CommandText = "SELECT SERIAL FROM KEYINFO WHERE CATEGORY='" + category + "'";
                if (decrypt)
                {
                    serial = DecryptString((string)Com.ExecuteScalar(), password);

                }
                else
                {
                    serial = (string)Com.ExecuteScalar();

                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            Com.Dispose();
            return serial; ;
        }
        public static string EncryptString(string InputText, string Password)
        {
            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            byte[] PlainText = System.Text.Encoding.Unicode.GetBytes(InputText);
            byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());

            //This class uses an extension of the PBKDF1 algorithm defined in the PKCS#5 v2.0 
            //standard to derive bytes suitable for use as key material from a password. 
            //The standard is documented in IETF RRC 2898.

            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);
            //Creates a symmetric encryptor object. 
            ICryptoTransform Encryptor = RijndaelCipher.CreateEncryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
            MemoryStream memoryStream = new MemoryStream();
            //Defines a stream that links data streams to cryptographic transformations
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(PlainText, 0, PlainText.Length);
            //Writes the final state and clears the buffer
            cryptoStream.FlushFinalBlock();
            byte[] CipherBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            string EncryptedData = Convert.ToBase64String(CipherBytes);
            return EncryptedData;

        }
        public static string DecryptString(string InputText, string Password)
        {
           // streamReader.BaseStream.ReadTimeout = 2000;
            try
            {
                RijndaelManaged RijndaelCipher = new RijndaelManaged();
                byte[] EncryptedData = Convert.FromBase64String(InputText);
                byte[] Salt = Encoding.ASCII.GetBytes(Password.Length.ToString());
                //Making of the key for decryption
                PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(Password, Salt);
                //Creates a symmetric Rijndael decryptor object.
                ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
                MemoryStream memoryStream = new MemoryStream(EncryptedData);
                //Defines the cryptographics stream for decryption.THe stream contains decrpted data
                CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);
                byte[] PlainText = new byte[EncryptedData.Length];
                int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);
                memoryStream.Close();
                cryptoStream.Close();
                //Converting to string
                string DecryptedData = Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);
                return DecryptedData;
            }
            catch(Exception ex)
            {
                string error = ex.Message;
                return "";
            }
        }
          
    }
    
}
