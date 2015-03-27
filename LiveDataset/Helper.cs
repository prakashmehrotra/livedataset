// Contains a set of utility functions

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Security.Cryptography;

namespace LiveDataset
{
    //Logs exception, messages to the text file
    static class Log
    {
        #region Variables

        static TextWriter objTextWriter;
        static int loglevel = 0; //0 => Errors, 1=> Warnings, 2 => Verbose (All messages)
        static bool warning = false;
        static bool error = false;
        static string filename, filename_err, filename_warn;
        static bool DuplicateLogOnConsole { get; set; }
        static string timestamp = DateTime.Now.ToString("yyyy-MM-dd - {0} HHmmss");
       
        #endregion

       public static int LogLevel
        {
            set { loglevel = value; }
        }

        /// <summary>
        /// Opens the file, creates the directory if not exists
        /// </summary>
        internal static void OpenFile()
        {
            string sDir = Path.Combine(Directory.GetCurrentDirectory(), "log");

            Directory.CreateDirectory(sDir);
            filename = Path.Combine(sDir, string.Format(timestamp, "Log") + ".txt");
            filename_err = Path.Combine(sDir, string.Format(timestamp, "Log") + " ERROR.txt");
            filename_warn = Path.Combine(sDir, string.Format(timestamp, "Log") + " WARNING.txt");
            objTextWriter = new StreamWriter(filename);
        }

        /// <summary>
        /// Writes message along with current timestamp.
        /// </summary>
        /// <param name="str">string</param>
        internal static void WriteMessage(string strMessage)
        {
            if (loglevel < 2) return;
            try
            {
                objTextWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + strMessage);
                objTextWriter.Flush();
            }
            catch(Exception) { }
        }

        /// <summary>
        /// Writes warning message
        /// </summary>
        /// <param name="str">string</param>
        internal static void WriteWarning(string strWarning)
        {
            if (loglevel < 1) return;
            objTextWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + "[Warning] " + strWarning);
            objTextWriter.Flush();
            warning = true;
        }

        /// <summary>
        /// Writes error message
        /// </summary>
        /// <param name="str"></param>
        internal static void WriteError(string strError)
        {
            if (strError != null)
            {
                objTextWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss ") + "[*Error*] " + strError);
                objTextWriter.Flush();
                error = true;

            }
        }

        /// <summary>
        /// Closes the file
        /// </summary>
        internal static void Close()
        {
            try
            {
                objTextWriter.Flush();
                objTextWriter.Close();
                if (error)
                {
                    File.Move(filename, filename_err);
                    return;
                }
                if (warning) File.Move(filename, filename_warn);
            }
            catch(Exception) { }
        }
    }

    /// <summary>
    /// Class containing a set of utility functions
    /// </summary>
    static class Helper
    {
        #region Constants

            internal const string JOBNAME = "Name";
            internal const string JOBSTATUS = "Status";
            internal const string JOBINTERVAL = "Interval";
            internal const string JOBDATASET = "Dataset";
            internal const string JOBLASTPOLLED = "LastPolled";
            internal const string JOBRECORDS = "Records";

        #endregion

        static byte[] bytes = ASCIIEncoding.ASCII.GetBytes("ZeroCool");

        /// <summary>
        /// Creates connection string.
        /// </summary>
        /// <param name="ServerName">string</param>
        /// <param name="DbName">string</param>
        /// <param name="UserID">string</param>
        /// <param name="Password">string</param>
        /// <returns></returns>
        internal static string ConnectionString(string ServerName, string DbName, string UserID, string Password)
        {
            string ConnectionString;

            ServerName = "'" + ServerName + "'";
            DbName = "'" + DbName + "'";
            UserID = "'" + UserID + "'";
            Password = "'" + Password + "'";
            if (ServerName.Contains("windows.net")) // SQL Azure
            {
                ConnectionString = string.Format(@"Server={0};Database={1};User ID={2};Password={3};Trusted_Connection=False;Encrypt=True;Connection Timeout=30;", ServerName, DbName, UserID, Password);
            }
            else
            {
                ConnectionString = string.Format(@"Server={0};Database={1};User ID={2};Password={3};TrustServerCertificate=True;Trusted_Connection=True;Encrypt=True;Connection Timeout=30;", ServerName, DbName, UserID, Password);
            }
            return ConnectionString;

        }

        /// <summary>
        /// Encrypts any input string
        /// </summary>
        /// <param name="originalString">string</param>
        /// <returns></returns>
        internal static string Encrypt(string originalString)
        {
            if (String.IsNullOrEmpty(originalString))
            {
                throw new ArgumentNullException
                       ("The string which needs to be encrypted can not be null.");
            }
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateEncryptor(bytes, bytes), CryptoStreamMode.Write);
            StreamWriter writer = new StreamWriter(cryptoStream);
            writer.Write(originalString);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            writer.Flush();
            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        /// <summary>
        /// Decrypts the string.
        /// </summary>
        /// <param name="originalString">string</param>
        /// <returns></returns>
        internal static string Decrypt(string cryptedString)
        {
            if (String.IsNullOrEmpty(cryptedString))
            {
                throw new ArgumentNullException
                   ("The string which needs to be decrypted can not be null.");
            }
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream
                    (Convert.FromBase64String(cryptedString));
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateDecryptor(bytes, bytes), CryptoStreamMode.Read);
            StreamReader reader = new StreamReader(cryptoStream);
            return reader.ReadToEnd();
        }
    }
}
