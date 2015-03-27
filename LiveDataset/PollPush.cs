// Contains functionality to read data from the configuted sources and push it to Power BI
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.IO;

using PowerBIExtensionMethods;
using System.Web.Script.Serialization;
using System.Net;
using System.Threading;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Xml;

namespace LiveDataset
{
    internal class PollPush
    {
        #region Variables

            internal string clientID;//Store Client ID required for Power BI
            internal string CurrentVersion = "0";//Stores Current Version to get latest data from source
            internal string ConnectionString { get; set; }//Stores source connection string
            internal string PowerBIDatasetName { get; set; }//Stores Power BI dataset name
            internal bool IsInitialLoad { get; set; }//Stores true or false to load inital data
            internal string datasetId { get; set; }//stores dataset id

            private static string resourceUri = "https://analysis.windows.net/powerbi/api";  // Power BI resource uri used to get authentication token.
            private static string authority = "https://login.windows.net/common/oauth2/authorize"; // Used to create the authentication context -  OAuth2 authority
            private static string datasetsUri = "https://api.powerbi.com/beta/myorg/datasets";//Used to get the datasets from Power BI
            private AuthenticationContext authContext = null;//Gets the authentication token
            private string token = String.Empty;//Stores token
            private UserCredential UserCredential;//Stores user credentials
            private int iMaxRowsPull;

        #endregion

        #region Constructor

            /// <summary>
            /// Initiazes variables
            /// </summary>
            /// <param name="objUserCredential">UserCredential</param>
            /// <param name="sClientID">string</param>
            internal PollPush(UserCredential objUserCredential, string sClientID)
            {
                UserCredential = objUserCredential;
                clientID = sClientID;
                if (ConfigurationManager.AppSettings["MAXROWSPULL"] != null)
                {
                    iMaxRowsPull = Convert.ToInt32(ConfigurationManager.AppSettings["MAXROWSPULL"]);
                }
                else
                    iMaxRowsPull = 5000;
            }

        #endregion

        #region Internal methods

        /// <summary>
        /// Validates and push data to Power BI - Heart of the system
        /// </summary>
        /// <param name="TableName">string, Table name</param>
        /// <param name="RecordsAffected">int</param>
        /// <param name="IsDatasetOnly">bool</param>
        /// <returns>string</returns>
        internal string ValidatePushData(string TableName, string Columns, out int RecordsAffected, bool IsDatasetOnly = false)
        {
            DataSet dsData = new DataSet();
            SqlDataAdapter objData;
            int iCounter = 0;
            string sCurrentVersion = "";
            try
            {
                Log.WriteMessage("ValidatePushData method Starts .....");
               
               if (PowerBIDatasetName == "")
                    PowerBIDatasetName = TableName;

                if (CurrentVersion == "0" && IsInitialLoad)//if value is true then get all the data - -valid for first time
                {
                    Log.WriteMessage("Getting all records - First time Pull");
                    Columns = BuildSQLColumn(Columns);
                    objData = new SqlDataAdapter("SELECT " + Columns + " FROM [" + TableName + "] T1;SELECT CHANGE_TRACKING_CURRENT_VERSION() AS CurrentVersion  FROM CHANGETABLE(CHANGES [" + TableName + "],0) AS ChangedData", ConnectionString);
                    objData.Fill(dsData);
                    Log.WriteMessage("Filling the dataset");
                    if (dsData.Tables.Count > 0)
                    {
                        if (dsData.Tables[0].Rows.Count > 0)
                        {
                            iCounter = dsData.Tables[0].Rows.Count;
                            Log.WriteMessage(iCounter + " records found. Inner method AddRows starts.");
                            AddRows(TableName, dsData.Tables[0],IsDatasetOnly);
                            Log.WriteMessage("Inner method AddRows completed");
                        }
                        if (dsData.Tables[1].Rows.Count > 0)
                            CurrentVersion = dsData.Tables[1].Rows[0][0].ToString();
                        else
                            CurrentVersion = "-1";
                    }//end (if (dsData.Tables.Count > 0))
                }
                else
                {
                    //Get the changed data
                    Log.WriteMessage("Running Change table quey to get column name");
                    Log.WriteMessage("Table Name " + TableName);
                    Log.WriteMessage("Power BI Dataset Name " + PowerBIDatasetName);

                    if (CurrentVersion == "")
                        CurrentVersion = "0";

                    objData = new SqlDataAdapter("SELECT CHANGE_TRACKING_CURRENT_VERSION() AS CurrentVersion,ChangedData.*  FROM CHANGETABLE(CHANGES [" + TableName + "],0) AS ChangedData ", ConnectionString);
                    objData.Fill(dsData);
                    Log.WriteMessage("Filling the dataset");
                    if (dsData.Tables.Count > 0)
                    {
                        if (dsData.Tables[0].Rows.Count > 0)
                        {
                            Log.WriteMessage("Fetching column name");
                            string sColumnName = dsData.Tables[0].Columns[dsData.Tables[0].Columns.Count - 1].ColumnName;
                            sCurrentVersion = dsData.Tables[0].Rows[0][0].ToString();                                                        
                            dsData.Tables.Clear();
                            dsData.Tables.Add(TableName);
                            Log.WriteMessage("Fetching new records");

                            Columns = BuildSQLColumn(Columns);
                            objData = new SqlDataAdapter("SELECT " + Columns + " FROM [" + TableName + "] T1 RIGHT OUTER JOIN  CHANGETABLE(CHANGES [" + TableName + "],0) AS ChangedData ON ChangedData.[" + sColumnName + "] = T1.[" + sColumnName + "] WHERE SYS_CHANGE_OPERATION = 'I' AND SYS_CHANGE_VERSION > " + CurrentVersion, ConnectionString);
                            objData.Fill(dsData.Tables[0]);

                            iCounter = dsData.Tables[0].Rows.Count;
                            Log.WriteMessage(iCounter + " records found. Inner method AddRows starts.");
                            if (iCounter > 0)
                                AddRows(TableName, dsData.Tables[0], IsDatasetOnly);
                            Log.WriteMessage("Inner method AddRows completed");
                            CurrentVersion = sCurrentVersion;

                        }
                    }
                }
                RecordsAffected = iCounter;

                if (iCounter == 0)
                    Log.WriteMessage("No new records populated!");

                return "";

            }
            catch (Exception ex)
            {
                Log.WriteMessage(ex.Message + " " );
                Log.WriteMessage(ex.StackTrace + " ");
                RecordsAffected = iCounter;
                return ex.Message;
            }
            
        }

        /// <summary>
        /// Retrieves all dataset from Power BI
        /// </summary>
        /// <returns>List<Object>, containing all available datasets in Power BI</returns>
        internal List<Object> GetAllDatasets()
        {
            List<Object> datasets = null;
            try
            {
               HttpWebRequest request = GetDatasetRequest(datasetsUri, "GET", GetAccessToken());
                string responseContent = GetResponse(request);
                datasets = responseContent.ToObject<List<Object>>();

                request = null;
                return datasets;
            }
            finally
            {
                datasets = null;
            }
        }

        /// <summary>
        /// Purges data in Power BI.
        /// </summary>
        /// <param name="datasetName">string Dataset name to fetch ID for deletion</param>
        /// <param name="TableName">string Table name to pass as parameter for deletion</param>
        internal void PurgeData(string datasetName, string TableName,string datasetid)
        {
            //Create a DELETE web request
            HttpWebRequest request = GetDatasetRequest(String.Format("{0}/{1}/tables/{2}/rows", datasetsUri, datasetid, TableName), "DELETE", GetAccessToken());
            request.ContentLength = 0;
            GetResponse(request);
        }

        /// <summary>
        /// Validates Power BI connections.
        /// </summary>
        /// <returns>bool, true if validated successfully</returns>
        internal bool ValidatePowerBIConnecton(out string Error)
        {
            Boolean bIsConnectionValid = false;
            string datasetName = "";
            try
            {
                HttpWebRequest request = System.Net.WebRequest.Create(datasetsUri) as System.Net.HttpWebRequest;
                request.KeepAlive = true;
                request.Method = "GET";
                request.ContentLength = 0;
                request.ContentType = "application/json";
                request.Headers.Add("Authorization", String.Format("Bearer {0}", GetAccessToken()));
                request.AllowAutoRedirect = false;

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.TemporaryRedirect)
                    datasetName = response.Headers["Location"];

                request = null;
                response = null;
                bIsConnectionValid = true;

                Error = "";
                return bIsConnectionValid;
            }
            catch (Exception ex)
            {
                if (ex.Message.Length > 100)
                    Error = ex.Message.Substring(0, 100);
                else
                    Error = ex.Message;
                Log.WriteError(ex.Message);
                if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                Log.WriteError(ex.StackTrace);
                
                return false;
            }

        }

        /// <summary>
        /// Validates SQL connection.
        /// </summary>
        /// <param name="ConnectionString">string, Connection string</param>
        /// <param name="Exception">string, Exception if any</param>
        /// <returns>boolm true if connection validated</returns>
        internal static bool ValidateSQLConnection(string ConnectionString, out string Exception)
        {
            SqlConnection objConnection = new SqlConnection(ConnectionString);
            if (objConnection.State == System.Data.ConnectionState.Closed)
            {
                try
                {
                    objConnection.Open();
                    Exception = "";
                    return true;
                }
                catch (SqlException ex)
                {
                    if (ex.Message.Length > 100)
                        Exception = ex.Message.Substring(0, 100);
                    else
                        Exception = ex.Message;
                    Log.WriteError(ex.Message);
                    if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                    Log.WriteError(ex.StackTrace);

                    return false;
                }
                finally
                {
                    objConnection.Close();
                }
            }//end (if (objConnection.State == System.Data.ConnectionState.Closed))
            else
            {
                Exception = "";
                return true;
            }
        }

        /// <summary>
        /// Checks if table has change tracking enabled
        /// </summary>
        /// <param name="ConnectionString">string, SQl connection string</param>
        /// <param name="TableName">string, table name for validation</param>
        /// <param name="Exception">string, any exception</param>
        /// <returns>bool, true if enabled</returns>
        internal static bool IsChangeTrackingEnable(string ConnectionString, string TableName, out string Exception)
        {
            SqlDataAdapter objData;
            DataSet dsTableName;
            try
            {
                dsTableName = new DataSet();
                objData = new SqlDataAdapter(" SELECT name FROM sys.change_tracking_tables t INNER JOIN sys.objects o ON t.object_id = o.object_id WHERE o.name = '" + TableName + "'", ConnectionString);
                objData.Fill(dsTableName);
                Exception = "";
                if (dsTableName.Tables[0].Rows.Count > 0)
                    return true;
                else
                    return false;
            }
            catch (SqlException ex)
            {
                Exception = ex.Message;
                Log.WriteError(ex.Message);
                if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                Log.WriteError(ex.StackTrace);
                return false;
            }
            finally
            {
                dsTableName = null;
                objData = null;
            }
        }

        /// <summary>
        /// Get all the tables in the database
        /// </summary>
        /// <returns>Dataset</returns>
        internal DataSet ListAllDatasets()
        {
            DataSet dsData = null;
            try
            {
                dsData = new System.Data.DataSet();
                SqlDataAdapter objData = new SqlDataAdapter("SELECT name, object_id as id FROM sys.tables", ConnectionString);
                objData.Fill(dsData);

            }
            catch (SqlException ex)
            {
                Log.WriteError(ex.Message);
                Log.WriteError(ex.StackTrace);
            }

            return dsData;
        }

        /// <summary>
        /// Creates datasets in Power BI
        /// </summary>
        /// <param name="TableName">string</param>
        /// <returns>bool, true if dataset created</returns>
        internal bool CreateDatasets(string TableName, XmlNode xnData, out string[] NewDataset)
        {
            DataSet dsData = new DataSet();
            SqlDataAdapter objData;

            if (TableName.Contains(","))
            {
                string[] sarrTables = TableName.Split(',');

                foreach (string sTableName in sarrTables)
                {
                    //Fetch columns from XML
                    string sColumns = xnData.SelectSingleNode("./JobMetadata[@Datasetname='" + sTableName + "']").Attributes["Columns"].Value;
                    sColumns = BuildSQLColumn(sColumns);
                    objData = new SqlDataAdapter("SELECT " + sColumns + " FROM [" + sTableName + "] T1 ", ConnectionString);
                    objData.Fill(dsData, sTableName);
                    Log.WriteMessage("Filling the dataset");
                }

            }
            else
            {
                //Fetch columns from XML
                string sColumns = xnData.SelectSingleNode("./JobMetadata[@Datasetname='" + TableName + "']").Attributes["Columns"].Value;
                sColumns = BuildSQLColumn(sColumns);

                objData = new SqlDataAdapter("SELECT " + sColumns + " FROM [" + TableName + "] T1 ", ConnectionString);
                objData.Fill(dsData, TableName);
                Log.WriteMessage("Filling the dataset");
            }

            //Create a POST web request to list all datasets
            HttpWebRequest request = GetDatasetRequest(datasetsUri, "POST", GetAccessToken());
            Log.WriteMessage("Posting request using Json Schema");
            string strJason = JSONBuilder.GetJsonSchema(dsData, PowerBIDatasetName);
            string responseContent = PostRequest(request, strJason);

            //Get list from response
            if (responseContent != "")
                NewDataset = responseContent.Split(',');
            else
                NewDataset = null;

            request = null;

            return true;

        }

        /// <summary>
        /// Populates columns for the corresponding table.
        /// </summary>
        /// <param name="TableName">string</param>
        /// <returns></returns>
        internal DataSet PopulateColumns(string TableName)
        {
            DataSet dsData = null;
            try
            {
                dsData = new System.Data.DataSet();
                SqlDataAdapter objData = new SqlDataAdapter("SELECT Col.name FROM SYS.Columns Col INNER JOIN SYS.Objects O ON Col.object_id = O.object_id WHERE O.name = '" +TableName+"'", ConnectionString);
                objData.Fill(dsData);

            }
            catch (SqlException ex)
            {
                Log.WriteError(ex.Message);
                if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                Log.WriteError(ex.StackTrace);
            }

            return dsData;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Add rows to the Power BI datasets.
        /// </summary>
        /// <param name="TableName">string</param>
        /// <param name="TableData">DataTable</param>
        /// <param name="IsDatasetOnly">bool</param>
        private void AddRows(string TableName, DataTable TableData, bool IsDatasetOnly)
        {
            Log.WriteMessage("Add Rows starts, getting dataset id from Power BI");

            if (IsDatasetOnly == false)//Insert the records also
            {
                Log.WriteMessage("Posting the rows in Power BI");
                if (TableData.Rows.Count <= iMaxRowsPull)
                    PostRows(TableName, TableData);
                else
                {
                    Log.WriteMessage("Splitting the rows and post splitted rows- " + TableData.Rows.Count);
                    DataTable dtNewData = TableData.Clone();
                    int iRowsCount = 0;
                    for (int iCounter = 0; iCounter < TableData.Rows.Count - 1; iCounter++)
                    {
                        dtNewData.ImportRow(TableData.Rows[iCounter]);
                        iRowsCount++;

                        if (iRowsCount == iMaxRowsPull)
                        {
                            Log.WriteMessage("Iteration starts to post row");
                            PostRows(TableName, dtNewData);
                            iRowsCount = 0;
                            dtNewData.Clear();
                        }
                    }
                    Log.WriteMessage("Last Iteration starts - " + dtNewData.Rows.Count);
                    if (dtNewData.Rows.Count > 0)
                        PostRows(TableName, dtNewData);
                }


            }//end (if (IsDatasetOnly == false))
        }

        /// <summary>
        /// Gets dataset request to validate dataset exists in Power BI or not.
        /// </summary>
        /// <param name="datasetsUri">string</param>
        /// <param name="method">string</param>
        /// <param name="authorizationToken">string</param>
        /// <returns></returns>
        private HttpWebRequest GetDatasetRequest(string datasetsUri, string method, string authorizationToken)
        {
            HttpWebRequest request = System.Net.WebRequest.Create(datasetsUri) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = method;
            request.ContentLength = 0;
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", String.Format("Bearer {0}", authorizationToken));

            return request;
        }

        /// <summary>
        /// Returns the access token acquired from ADAL (Active Directory Authentication Library)
        /// </summary>
        /// <returns>string</returns>
        private string GetAccessToken()
        {

            if (token == String.Empty)
            {
                TokenCache TC = new TokenCache();
                authContext = new AuthenticationContext(authority, TC);
                token = authContext.AcquireToken(resourceUri, clientID, UserCredential).AccessToken.ToString();
            }
            else
                token = authContext.AcquireTokenSilent(resourceUri, clientID).AccessToken;


            return token;
        }

        /// <summary>
        /// Post request to Power BI
        /// </summary>
        /// <param name="request">HttpWebRequest</param>
        /// <param name="json">string</param>
        /// <returns>string</returns>
        private static string PostRequest(HttpWebRequest request, string json)
        {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(json);
            request.ContentLength = byteArray.Length;

            //Write JSON byte[] into a Stream
            using (Stream writer = request.GetRequestStream())
            {
                writer.Write(byteArray, 0, byteArray.Length);
            }
            return GetResponse(request);
        }

        /// <summary>
        /// Reads HTTP request and converts to string as response.
        /// </summary>
        /// <param name="request">HttpWebRequest</param>
        /// <returns>string</returns>
        private static string GetResponse(HttpWebRequest request)
        {
            string response = string.Empty;
            using (HttpWebResponse httpResponse = request.GetResponse() as System.Net.HttpWebResponse)
            {
                //Get StreamReader that holds the response stream
                using (StreamReader reader = new System.IO.StreamReader(httpResponse.GetResponseStream()))
                {
                    response = reader.ReadToEnd();
                }
                request.Abort();
            }     
            
            return response;
        }

        /// <summary>
        /// Post the rows to Power BI.
        /// </summary>
        private void PostRows(string TableName, DataTable TableData)
        {
            HttpWebRequest request;

            request = GetDatasetRequest(String.Format("{0}/{1}/tables/{2}/rows", datasetsUri, datasetId, TableName), "POST", GetAccessToken());
            string strJasonRows = string.Format("{0}\"rows\":", "{");
            strJasonRows += JSONBuilder.ConvertDataTableTojSonString(TableData);
            strJasonRows += "}";
            PostRequest(request, strJasonRows);
            Log.WriteMessage("Post request completed");
        }

        /// <summary>
        /// Builds columns with appropriate SQl syntax.
        /// </summary>
        /// <param name="Columns">string, contains columns</param>
        private string BuildSQLColumn(string Columns)
        {
            if (Columns.Contains(","))
            {
                string[] sarrColName = Columns.Split(',');
                Columns = "";

                foreach (string sCol in sarrColName)
                {
                    if (Columns == "")
                        Columns = "T1.[" + sCol + "]";
                    else
                        Columns += ",T1.[" + sCol + "]";
                }
            }
            else
                Columns = "T1.[" + Columns + "]";

            return Columns;
        }

        #endregion

    }//End (PollPush)
}
