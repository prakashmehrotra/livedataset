//Pulls data from Source and post to destination. It also does polling after specific interval.
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace LiveDataset
{
    //This class pulls data from Source and post to destination. It also does polling after specific interval.
    internal class PollPushService
    {
        /// <summary>
        /// Refresh Job details during processing.
        /// </summary>
       internal RefreshJobDetails RefreshJobDetailUI;
        
        #region Properties

            internal DataGridViewRow SelectedRow { get; set; }
            internal string ClientID { get; set; }
            internal UserCredential UserCredential { get; set; }
            internal string TableName { get; set; }
            internal int RecordsAffected;
            internal string ConnectionString { get; set; }
            internal string PowerBIDatasetName { get; set; }
            internal Hashtable CurrentVersion { get; set; }
            internal int Interval { get; set; }
            internal bool IsIntialLoad { get; set; }
            internal XmlNode JobDetails { get; set; }
            internal string datasetId { get; set; }

        #endregion

        /// <summary>
        /// Starts infinite loop to pull and push data. This is executed on a separate thread from within the calling code.
        /// </summary>
        internal void PushData()
        {
            Hashtable objDatasetVersions = new Hashtable();
            string sErrorMessage = "";
            PollPush objPollPush = new PollPush(UserCredential, ClientID);
            while (true)
            {
                objPollPush.ConnectionString = ConnectionString;
                objPollPush.PowerBIDatasetName = PowerBIDatasetName;
                objPollPush.IsInitialLoad = IsIntialLoad;
                objPollPush.datasetId = datasetId;
                if (TableName.Contains(","))
                {
                    string[] sarrTables = TableName.Split(',');
                    foreach (string sTableName in sarrTables)
                    {
                        objPollPush.CurrentVersion = CurrentVersion[sTableName].ToString();
                        string sColumns = JobDetails.SelectSingleNode("./JobMetadata[@Datasetname='" + sTableName + "']").Attributes["Columns"].Value;

                        sErrorMessage = objPollPush.ValidatePushData(sTableName,sColumns, out RecordsAffected);
                        CurrentVersion[sTableName] = objPollPush.CurrentVersion;
                        if (RefreshJobDetailUI != null)
                            RefreshJobDetailUI(SelectedRow, sTableName, objPollPush.CurrentVersion, sErrorMessage, RecordsAffected);
                    }
                }
                else
                {
                    if (CurrentVersion[TableName] != null)
                    {
                        objPollPush.CurrentVersion = CurrentVersion[TableName].ToString();
                        string sColumns = JobDetails.SelectSingleNode("./JobMetadata[@Datasetname='" + TableName + "']").Attributes["Columns"].Value;

                        sErrorMessage = objPollPush.ValidatePushData(TableName, sColumns, out RecordsAffected);
                        CurrentVersion[TableName] = objPollPush.CurrentVersion;

                        if (RefreshJobDetailUI != null)
                            RefreshJobDetailUI(SelectedRow, TableName, objPollPush.CurrentVersion, sErrorMessage, RecordsAffected);
                    }
                }

                Thread.Sleep(Interval * 1000);
            }
        }

    }//end (PollPushService)
}
