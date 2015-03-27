//Main application form
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Collections;
using System.IO;
using System.Configuration;

namespace LiveDataset
{
    /// <summary>
    /// Refresh Job details during processing.
    /// </summary>
    internal delegate void RefreshJobDetails(DataGridViewRow SelectedRow, string TableName, string iCurrentVersion, string ErrorMessage, int iRecordsAffected);

    //Main form
    public partial class FormMain : Form
    {
        #region Member Variables

            private Hashtable m_hstJobCollections = null;//Job Collection constituting Job name and PollPush Service object
            private XmlDocument m_xdData = null;//XML Doc containing Xml for maintaining all jobs and metadata
            private PollPushService objPollPushService = null;//Class to do polling in database and push new dataset

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes the variables and load the xml.
        /// </summary>
        public FormMain()
        {
            InitializeComponent();
            GridView.ReadOnly = true;
            m_hstJobCollections = new Hashtable();
            m_xdData = new XmlDocument();

            if (!File.Exists(Application.StartupPath + @"\JobSettings.xml"))
            {
                XmlNode docNode = m_xdData.CreateXmlDeclaration("1.0", "UTF-8", null);
                m_xdData.AppendChild(docNode);

                XmlNode JobsNode = m_xdData.CreateElement("Jobs");
                m_xdData.AppendChild(JobsNode);
                m_xdData.Save(Application.StartupPath + @"\JobSettings.xml");
            }
            else
                m_xdData.Load(Application.StartupPath + @"\JobSettings.xml");

            if (ConfigurationManager.AppSettings["LOGLEVEL"] != null)
            {
                Log.LogLevel = Convert.ToInt32(ConfigurationManager.AppSettings["LOGLEVEL"]);
            }
            else
                Log.LogLevel = 2;

            Log.OpenFile();
            Log.WriteMessage("Live Datasets version - " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString());
            this.Text += " - Version: " + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
        }

        #endregion

        #region Events Handled

            #region Form events
            
            /// <summary>
            /// Get all Jobs from XML, if XML is empty then open Create Job form
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void FormMain_Load(object sender, EventArgs e)
            {
                try
                {
                    Log.WriteMessage("Form Load Starts, Get the list of Jobs!");
                    GetInventoryJobs();

                    if (GridView.Rows.Count == 0)
                    {
                        GridView.Visible = false;
                        Log.WriteMessage("Pops up Create New Job form!");
                        FormJobSettings objFormJobSettings = new FormJobSettings(true, "SQL Azure Database");
                        objFormJobSettings.NewJobCreated += FormJobSettings_NewJobCreated;
                        objFormJobSettings.ShowDialog();
                    }

                    Log.WriteMessage("Form Load Completed!");
                }
                catch (Exception ex)
                {
                    Log.WriteError(ex.Message);
                    if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                    Log.WriteError(ex.StackTrace);
                }
            }

            /// <summary>
            /// Check whether all jobs are not running, if any job is running state then prompt user
            /// Based on user's feedback, close the form
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
            {
                if (GridView.Rows.Count > 0)
                {
                    Log.WriteMessage("Form closing event starts!");
                    foreach (DataGridViewRow drData in GridView.Rows)
                    {
                        Log.WriteMessage("Prompt user as Jobs are in Running state!");
                        if (drData.Cells[2].Value.ToString() == "Running")
                        {
                            DialogResult drUserResponse = MessageBox.Show("Few of the Job(s) are running. Closing form will stop all Job(s). Do you wish to continue?\n\nClicking 'Yes' will stop all jobs and closes form.\nClicking 'No' will do nothing.", "Live Feed", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                            if (drUserResponse == System.Windows.Forms.DialogResult.No)
                            {
                                Log.WriteMessage("User confirmed to cancel the form close event!");
                                e.Cancel = true;
                                return;
                            }
                            else
                            {
                                Log.WriteMessage("User confirmed to abort process and close form!");
                                AbortProcess();
                                m_hstJobCollections = null;
                                m_xdData = null;
                                objPollPushService = null;
                                Log.Close();
                                return;
                            }
                        }
                    }
                }
                Log.Close();
                m_hstJobCollections = null;
                m_xdData = null;
                objPollPushService = null;
               
            }

            #endregion

            #region Grid View events

            /// <summary>
            /// If user clicked first cell to start then start processing. Ignore the click for other cells.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void GridView_CellClick(object sender, DataGridViewCellEventArgs e)
            {
                try
                {
                    if (e.RowIndex > -1)
                    {
                        if (e.ColumnIndex == 0)
                        {
                            Log.WriteMessage("Processing Starts!");
                            DataGridViewRow SelectedRow = GridView.Rows[e.RowIndex];
                            ProcessSelectedFeed(SelectedRow);
                            Log.WriteMessage("Processing Completed!");
                        }
                    }

                }
                catch (Exception ex)
                {
                    JobStatus.Text = "Error - " + ex.Message;
                    Log.WriteError(ex.Message);
                    if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                    Log.WriteError(ex.StackTrace);
                }
            }
            
            /// <summary>
            /// Opens the edit form for the selected row.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void GridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
            {
                try
                {
                    Log.WriteMessage("Editing the Job details starts!");
                    EditJobDetails(false);
                    Log.WriteMessage("Editing the Job details completed!");
                }
                catch (Exception ex)
                {
                    Log.WriteError(ex.Message);
                    Log.WriteError(ex.StackTrace);
                    if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                    JobStatus.Text = "Error - " + ex.Message;
                }
            }
            
            /// <summary>
            /// Apply color based on job state (running, failed or stopped)
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void GridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
            {
                if (GridView.NewRowIndex != e.RowIndex)
                {
                    if (GridView.Rows[e.RowIndex].Cells[2].Value.ToString() == "Stopped")
                    {
                        foreach (DataGridViewCell item in GridView.Rows[e.RowIndex].Cells)
                            item.Style.BackColor = SystemColors.Control;
                    }
                    else if (GridView.Rows[e.RowIndex].Cells[2].Value.ToString() == "Running")
                    {
                        foreach (DataGridViewCell item in GridView.Rows[e.RowIndex].Cells)
                            item.Style.BackColor = Color.LightGreen;
                    }

                    else if (GridView.Rows[e.RowIndex].Cells[2].Value.ToString() == "Failed")
                    {
                        foreach (DataGridViewCell item in GridView.Rows[e.RowIndex].Cells)
                            item.Style.BackColor = Color.Wheat;
                    }
                }
            }

            /// <summary>
            /// Handles the delete key for deletion
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void GridView_KeyDown(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Delete)
                {
                    try
                    {
                        Log.WriteMessage("Delete starts!");
                        DeleteJob();
                        Log.WriteMessage("Delete completed!");
                    }
                    catch (Exception ex)
                    {
                        Log.WriteError(ex.Message);
                        Log.WriteError(ex.StackTrace);
                        if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                        JobStatus.Text = "Error - " + ex.Message;
                    }
                }
            }

            /// <summary>
            /// Enables or disables the context strip.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void GridView_MouseDown(object sender, MouseEventArgs e)
            {
                if (GridView.HitTest(e.X, e.Y).RowIndex < 0)
                {
                    if (GridView.SelectedRows.Count > 0)
                        GridView.SelectedRows[0].Selected = false;
                    deleteFeedToolStripMenuItem.Enabled = false;
                    editFeedToolStripMenuItem.Enabled = false;
                    purgeFeedToolStripMenuItem.Enabled = false;
                    cloneFeedToolStripMenuItem.Enabled = false;
                    ShowError.Visible = false;
                    JobStatus.Text = GridView.Rows.Count + " record(s) populated!";
                }//end (if (GridView.HitTest(e.X, e.Y).RowIndex < 0))
                else
                {
                    GridView.Rows[GridView.HitTest(e.X, e.Y).RowIndex].Selected = true;
                    deleteFeedToolStripMenuItem.Enabled = true;
                    editFeedToolStripMenuItem.Enabled = true;
                    purgeFeedToolStripMenuItem.Enabled = true;
                    cloneFeedToolStripMenuItem.Enabled = true;
                    ShowError.Visible = true;
                }//end (else if (GridView.HitTest(e.X, e.Y).RowIndex < 0)

                if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    if (GridView.SelectedRows.Count > 0)
                    {
                        if (GridView.SelectedRows[0].GetErrorText(GridView.SelectedRows[0].Index) == "")
                            ShowError.Enabled = false;
                        else
                            ShowError.Enabled = true;
                    }
                    else
                        ShowError.Enabled = false;
                }
            }

            /// <summary>
            /// Updates the form Status.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void GridView_SelectionChanged(object sender, EventArgs e)
            {
                if (GridView.SelectedRows.Count > 0)
                    JobStatus.Text = GridView.SelectedRows[0].Cells[Helper.JOBNAME].Value.ToString() + " job is in '" + GridView.SelectedRows[0].Cells[Helper.JOBSTATUS].Value.ToString() + "' state.";
            }

           #endregion

            #region Menu Events

            /// <summary>
            /// Open form to create new job.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void sQLDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
            {
                try
                {
                    FormJobSettings objFormJobSettings = new FormJobSettings(true, "SQL Azure Database");
                    objFormJobSettings.NewJobCreated += FormJobSettings_NewJobCreated;
                    objFormJobSettings.ShowDialog();
                }
                catch (Exception ex)
                {
                    Log.WriteError(ex.Message);
                    Log.WriteError(ex.StackTrace);
                    if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                    JobStatus.Text = "Error - " + ex.Message;
                }
            }

            /// <summary>
            /// Opens the Edit form if selected job is not running.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void editFeedToolStripMenuItem_Click(object sender, EventArgs e)
            {
                try
                {
                    EditJobDetails(false);
                }
                catch (Exception ex)
                {
                    Log.WriteError(ex.Message);
                    Log.WriteError(ex.StackTrace);
                    if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                    JobStatus.Text = "Error - " + ex.Message;
                }

            }

            /// <summary>
            /// Exits the form.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void exitToolStripMenuItem_Click(object sender, EventArgs e)
            {
                this.Close();
            }

            /// <summary>
            /// Clones the selected Job.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void cloneFeedToolStripMenuItem_Click(object sender, EventArgs e)
            {
                try
                {
                    EditJobDetails(true);
                }
                catch (Exception ex)
                {
                    Log.WriteError(ex.Message);
                    Log.WriteError(ex.StackTrace);
                    if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                    JobStatus.Text = "Error - " + ex.Message;
                }
            }


            /// <summary>
            /// Deletes the job if not in running state.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void deleteFeedToolStripMenuItem_Click(object sender, EventArgs e)
            {
                try
                {
                    DeleteJob();
                }
                catch (Exception ex)
                {
                    Log.WriteError(ex.Message);
                    Log.WriteError(ex.StackTrace);
                    if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                    JobStatus.Text = "Error - " + ex.Message;
                }
            }

            /// <summary>
            /// Purges the job if not in running state.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void purgeFeedToolStripMenuItem_Click(object sender, EventArgs e)
            {
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    PurgeData();
                    Cursor.Current = Cursors.Default;
                }
                catch (Exception ex)
                {
                    Cursor.Current = Cursors.Default;
                    Log.WriteError(ex.Message);
                    Log.WriteError(ex.StackTrace);
                    if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                    JobStatus.Text = "Error - " + ex.Message;
                }
               
            }

            /// <summary>
            /// Opens About form.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
            {
                try
                {
                    FormAbout objAboutForm = new FormAbout();
                    objAboutForm.Show();
                }
                catch (Exception ex)
                {
                    Log.WriteError(ex.Message);
                    Log.WriteError(ex.StackTrace);
                    if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                    JobStatus.Text = "Error - " + ex.Message;
                }
               
            }

            /// <summary>
            /// Shows error messages.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void toolStripMenuItem1_Click(object sender, EventArgs e)
            {
                if (GridView.SelectedRows.Count > 0)
                {
                    DataGridViewRow drSelected = GridView.SelectedRows[0];
                    string sError = drSelected.GetErrorText(drSelected.Index);
                    if (sError != "")
                        MessageBox.Show(sError, "Live Feed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            #endregion

            #region Events from Form Job Settings class

            /// <summary>
            /// Updates the grid datasource when new job is created and display new job in grid as last row.
            /// </summary>
            /// <param name="JobName">string, job name to fetch updated records and add to grid view</param>
            void FormJobSettings_NewJobCreated(string JobName)
            {
                int iGridRowsCount = 0;
                try
                {
                    if (GridView.Visible == false)
                        GridView.Visible = true;

                    Log.WriteMessage("New Job event arrived!");
                    iGridRowsCount = GridView.Rows.Count;
                    DataTable dtJobData = GridView.DataSource as DataTable;
                    if (dtJobData == null)
                    {
                        dtJobData = new DataTable();
                        DataColumn dcData;
                        dcData = new DataColumn(Helper.JOBNAME);
                        dtJobData.Columns.Add(dcData);

                        dcData = new DataColumn(Helper.JOBSTATUS);
                        dtJobData.Columns.Add(dcData);

                        dcData = new DataColumn(Helper.JOBINTERVAL);
                        dtJobData.Columns.Add(dcData);

                        dcData = new DataColumn(Helper.JOBDATASET);
                        dtJobData.Columns.Add(dcData);

                        dcData = new DataColumn(Helper.JOBLASTPOLLED);
                        dtJobData.Columns.Add(dcData);

                        dcData = new DataColumn(Helper.JOBRECORDS);
                        dtJobData.Columns.Add(dcData);

                        GridView.DataSource = dtJobData;
                    }

                    DataRow drNewRow = dtJobData.NewRow();
                    drNewRow[Helper.JOBNAME] = JobName;
                    drNewRow[Helper.JOBSTATUS] = "Stopped";
                    m_xdData.Load(Application.StartupPath + @"\JobSettings.xml");//Reload XML again
                    XmlNode xnJobData = m_xdData.SelectSingleNode("/Jobs/Job[@Name='" + JobName + "']");
                    if (xnJobData != null)
                    {
                        drNewRow[Helper.JOBINTERVAL] = xnJobData.Attributes[Helper.JOBINTERVAL].Value;
                        drNewRow[Helper.JOBDATASET] = xnJobData.Attributes[Helper.JOBDATASET].Value;
                        drNewRow[Helper.JOBLASTPOLLED] = "";
                        drNewRow[Helper.JOBRECORDS] = "0";
                        dtJobData.Rows.Add(drNewRow);

                        GridView.Rows[iGridRowsCount].Cells[0].Value = "Start Job";
                    }
                    JobStatus.Text = GridView.Rows.Count + " record(s) populated!";

                    if (GridView.Rows.Count > 0)
                        InitializeGridView();

                    Log.WriteMessage("New Job event completed!");
                }
                catch (Exception ex)
                {
                    Log.WriteError(ex.Message);
                    if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                    Log.WriteError(ex.StackTrace);
                    JobStatus.Text = "Error - " + ex.Message;
                }
            }

            /// <summary>
            /// Updates the grid datasource
            /// </summary>
            /// <param name="JobName">string, job name</param>
            void FormJobSettings_EditJobDetails(string JobName)
            {
                try
                {
                    Log.WriteMessage("Edit Job event arrived!");
                    m_xdData.Load(Application.StartupPath + @"\JobSettings.xml");//Reload XML again
                    XmlNode xnJobData = m_xdData.SelectSingleNode("/Jobs/Job[@Name='" + JobName + "']");
                    if (xnJobData != null)
                    {
                        DataGridViewRow drSelected = GridView.SelectedRows[0];
                        if (drSelected != null)
                        {
                            drSelected.Cells[Helper.JOBINTERVAL].Value = xnJobData.Attributes[Helper.JOBINTERVAL].Value;
                            drSelected.Cells[Helper.JOBDATASET].Value = xnJobData.Attributes[Helper.JOBDATASET].Value;
                            drSelected.Cells[Helper.JOBLASTPOLLED].Value = "";//this is mandatory as new dataset can be added to the list
                        }
                    }
                    Log.WriteMessage("Edit Job event completed!");

                }
                catch (Exception ex)
                {
                    Log.WriteError("Error: " + ex.Message);
                    if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                    Log.WriteError(ex.StackTrace);
                    JobStatus.Text = "Error - " + ex.Message;
                }
            }

            /// <summary>
            /// Clone the existing job
            /// </summary>
            /// <param name="JobName">string Job Name</param>
            void FormJobSettings_CloneJobDetails(string JobName)
            {
                Log.WriteMessage("Clone Job arrived - trigger new job method!");
                FormJobSettings_NewJobCreated(JobName);
                Log.WriteMessage("Clone Job completed!");
            }

            /// <summary>
            /// Enables or diables based on row selection and status.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void createToolStripMenuItem_Click(object sender, EventArgs e)
            {
                if (GridView.SelectedRows.Count > 0)
                {
                    DataGridViewRow drSelected = GridView.SelectedRows[0];
                    if (drSelected.Cells[Helper.JOBSTATUS].Value.ToString() == "Stopped" || drSelected.Cells[Helper.JOBSTATUS].Value.ToString() == "Failed")
                    {
                        deleteFeedToolStripMenuItem.Enabled = true;
                        editFeedToolStripMenuItem.Enabled = true;
                        purgeFeedToolStripMenuItem.Enabled = true;
                        cloneFeedToolStripMenuItem.Enabled = true;
                    }
                    else
                    {
                        deleteFeedToolStripMenuItem.Enabled = false;
                        editFeedToolStripMenuItem.Enabled = false;
                        purgeFeedToolStripMenuItem.Enabled = false;
                        cloneFeedToolStripMenuItem.Enabled = false;
                    }
                }
                
            }

            #endregion


        #endregion

        #region Private Methods

            /// <summary>
            /// Updates the xml based on status.
            /// </summary>
            /// <param name="SelectedRow">DataGridViewRow, selected row</param>
            /// <param name="TableName">string, table name to update the columns for tha specific table</param>
            /// <param name="iCurrentVersion">string, Current Version</param>
            private void UpdateXMLDoc(DataGridViewRow SelectedRow, string TableName, string iCurrentVersion)
            {
                if (SelectedRow.Index >= 0)
                {
                    Log.WriteMessage("Update XML while processing!");
                    if (SelectedRow.Selected)
                        JobStatus.Text = SelectedRow.Cells[Helper.JOBNAME].Value.ToString() + " job is in '" + SelectedRow.Cells[Helper.JOBSTATUS].Value.ToString() + "' state.";

                    Log.WriteMessage("Update the grid and XML!");
                    string CurrentDateTime = DateTime.Now.ToString();
                    SelectedRow.Cells[Helper.JOBLASTPOLLED].Value = CurrentDateTime;//Updated the Grid

                    XmlNode xnJobs = m_xdData.SelectSingleNode("/Jobs/Job[@Name='" + SelectedRow.Cells[Helper.JOBNAME].Value + "']");
                    xnJobs.Attributes[Helper.JOBLASTPOLLED].Value = CurrentDateTime;//Updated XML

                    XmlNode xnMetadata = xnJobs.SelectSingleNode("./JobMetadata[@Datasetname='" + TableName + "']");
                    if (xnMetadata != null)
                        xnMetadata.Attributes["Version"].Value = iCurrentVersion;//Updated XML 
                    
                    m_xdData.Save(Application.StartupPath + @"\JobSettings.xml");

                    Log.WriteMessage("Update xml completed!");
                }
            }

            /// <summary>
            /// Get all jobs from XML and loads in grid view.
            /// </summary>
            private void GetInventoryJobs()
            {
                DataSet dataSet = null;
                try
                {
                    Log.WriteMessage("Get all Jobs from XML!");
                    dataSet = new DataSet();

                    dataSet.ReadXml(Application.StartupPath + @"\JobSettings.xml", XmlReadMode.Auto);
                    if (dataSet.Tables.Count > 0)
                    {
                        GridView.DataSource = dataSet.Tables[0];
                        if (GridView.Rows.Count > 0)
                            InitializeGridView();

                        foreach (DataGridViewRow drData in GridView.Rows)
                            drData.Cells[0].Value = "Start Job";
                    }

                }
                finally { dataSet = null; }
            }

            /// <summary>
            /// Updates the polled end time and records based on job processing.
            /// </summary>
            /// <param name="SelectedRow">DataGridViewRow, row selected in grid view</param>
            /// <param name="iCurrentVersion">string, Current version in data table</param>
            /// <param name="ErrorMessage">string, any error message if returned while processing</param>
            /// <param name="iRecordsAffected">int, records are affected after polling the data</param>
            private void UpdateJobUI(DataGridViewRow SelectedRow, string TableName, string iCurrentVersion, string ErrorMessage, int iRecordsAffected)
            {
                try
                {
                    if (this.InvokeRequired && SelectedRow.Index > -1)
                        this.Invoke(new RefreshJobDetails(UpdateJobUI), new object[] { SelectedRow, TableName, iCurrentVersion, ErrorMessage, iRecordsAffected });
                    else
                    {
                        //Process the data
                        Log.WriteMessage("Event from Service!");
                        if (SelectedRow.Index > -1)
                        {
                            if (ErrorMessage != "")
                            {
                                SelectedRow.ErrorText += DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss - ") + ErrorMessage + "\n\n";
                                Log.WriteMessage("Error message has arrived!");

                                if (SelectedRow.Selected)
                                    JobStatus.Text = SelectedRow.Cells[Helper.JOBNAME].Value.ToString() + " job is in Failed state! Error - " + ErrorMessage;

                                SelectedRow.Cells[Helper.JOBSTATUS].Value = "Failed";
                                SelectedRow.Cells[0].Value = "Start Job";
                                //Abort thread and remove from collection
                                if (m_hstJobCollections.Contains(SelectedRow.Cells[Helper.JOBNAME].Value))
                                {
                                    Thread objService = m_hstJobCollections[SelectedRow.Cells[Helper.JOBNAME].Value] as Thread;
                                    objService.Abort();
                                    objService.Join();
                                    m_hstJobCollections.Remove(SelectedRow.Cells[Helper.JOBNAME].Value);
                                }
                                return;
                            }
                            Log.WriteMessage("Update XML if no error message!");
                            SelectedRow.Cells[Helper.JOBRECORDS].Value = Convert.ToInt32(SelectedRow.Cells[Helper.JOBRECORDS].Value) + iRecordsAffected;
                            //update xml
                            lock (m_xdData)
                                UpdateXMLDoc(SelectedRow, TableName, iCurrentVersion);

                            Log.WriteMessage("Event from Service completed!");
                        }
                    }
                }
                catch(Exception ex)
                {
                    Log.WriteError(ex.Message);
                    if (ex.InnerException != null) Log.WriteError("Inner Exception: " + ex.InnerException.Message);
                    Log.WriteError(ex.StackTrace);
                }
            }

            /// <summary>
            /// Processes selected job.
            /// </summary>
            /// <param name="SelectedRow">DataGridViewRow, selected row to process</param>
            private void ProcessSelectedFeed(DataGridViewRow SelectedRow)
            {
                if (SelectedRow.Cells[0].Value.ToString() == "Start Job")
                {
                    Log.WriteMessage("Job Started...");
                    XmlNode xnJobs = m_xdData.SelectSingleNode("/Jobs/Job[@Name='" + SelectedRow.Cells[Helper.JOBNAME].Value.ToString() + "']");
                    string sConnectionData = xnJobs.InnerXml;

                    XmlNode xnConnectionData = xnJobs.SelectSingleNode("./Connection");

                    //Source
                    string sSourceServerName = xnConnectionData.Attributes["SourceServerName"].Value;
                    string sSourceDbName = xnConnectionData.Attributes["SourceDatabaseName"].Value;
                    string sSourceUserName = xnConnectionData.Attributes["SourceUserID"].Value;
                    string sSourcePwd = Helper.Decrypt(xnConnectionData.Attributes["SourcePwd"].Value);

                    //Destination
                    string sClientID = xnConnectionData.Attributes["ClientID"].Value;
                    string sUserID = xnConnectionData.Attributes["UserName"].Value;
                    string sUserPwd = Helper.Decrypt(xnConnectionData.Attributes["Password"].Value);
                    string DestDatasetName = xnConnectionData.Attributes["DatasetName"].Value;

                    UserCredential uc = new UserCredential(sUserID, sUserPwd);

                    Log.WriteMessage("Fetching Source and Destination details");

                    string JobName = SelectedRow.Cells[Helper.JOBNAME].Value.ToString();

                    string sConnectionString = Helper.ConnectionString(sSourceServerName, sSourceDbName, sSourceUserName, sSourcePwd );
               
                    Log.WriteMessage("Created Connection string!");

                    objPollPushService = new PollPushService();
                    objPollPushService.ClientID = sClientID;
                    objPollPushService.SelectedRow = SelectedRow;
                    objPollPushService.UserCredential = uc;
                    objPollPushService.TableName = SelectedRow.Cells[Helper.JOBDATASET].Value.ToString();
                    objPollPushService.RecordsAffected = 0;
                    objPollPushService.ConnectionString = sConnectionString;
                    objPollPushService.PowerBIDatasetName = DestDatasetName;
                    objPollPushService.datasetId = xnConnectionData.Attributes["DatasetID"].Value;

                    objPollPushService.Interval = Convert.ToInt32(SelectedRow.Cells[Helper.JOBINTERVAL].Value);
                    objPollPushService.IsIntialLoad = Convert.ToBoolean(xnConnectionData.Attributes["IsFullLoad"].Value);

                    Hashtable hstVersion = new Hashtable();//Get the versions based on table
                    foreach (XmlNode xnMetadata in xnJobs.SelectNodes("./JobMetadata"))
                        hstVersion.Add(xnMetadata.Attributes["Datasetname"].Value.ToString(), xnMetadata.Attributes["Version"].Value.ToString());

                    objPollPushService.CurrentVersion = hstVersion;
                    objPollPushService.JobDetails = xnJobs;

                    objPollPushService.RefreshJobDetailUI = UpdateJobUI;
                    Thread tPollPushService = new Thread(objPollPushService.PushData);
                    tPollPushService.Start();

                    if (m_hstJobCollections.Contains(SelectedRow.Cells[Helper.JOBNAME].Value.ToString()) == false)
                        m_hstJobCollections.Add(SelectedRow.Cells[Helper.JOBNAME].Value.ToString(), tPollPushService);//Adding to collection for tracking
                    Log.WriteMessage("Thread Started");
                    SelectedRow.Cells[Helper.JOBSTATUS].Value = "Running";
                    SelectedRow.Cells[0].Value = "Stop Job";

                }//end ( if (SelectedRow.Cells[0].Value.ToString() == "Start Job"))
                else
                {
                    Log.WriteMessage("Job stopped...");
                    JobStatus.Text = SelectedRow.Cells[Helper.JOBNAME].Value.ToString() + " job is in 'Stopped' status" ;

                    if (m_hstJobCollections.Contains(SelectedRow.Cells[Helper.JOBNAME].Value))
                    {
                        Thread objService = m_hstJobCollections[SelectedRow.Cells[Helper.JOBNAME].Value] as Thread;
                        objService.Abort();
                        objService.Join();
                        m_hstJobCollections.Remove(SelectedRow.Cells[Helper.JOBNAME].Value);
                        SelectedRow.Cells[0].Value = "Start Job";

                        if (SelectedRow.Cells[Helper.JOBSTATUS].Value.ToString() != "Failed")
                            SelectedRow.Cells[Helper.JOBSTATUS].Value = "Stopped";
                    }//end (if (m_hstJobCollections.Contains(SelectedRow)))

                }//end (else of  if (SelectedRow.Cells[0].Value.ToString() == "Start Job"))

            
            }
        
            /// <summary>
            /// Opens the Edit form if job is not running.
            /// </summary>
            /// <param name="IsClone">bool, indicates whether data to be edited for cloned job or existing job</param>
            private void EditJobDetails(bool IsClone)
            {
                if (GridView.SelectedRows.Count > 0)
                {
                    DataGridViewRow drSelected = GridView.SelectedRows[0];
                    if (drSelected.Cells[Helper.JOBSTATUS].Value.ToString() == "Stopped" || drSelected.Cells[Helper.JOBSTATUS].Value.ToString() == "Failed")
                    {
                        if (IsClone)
                            MessageBox.Show("Cloning will copy all the fields except Job name & Destination dataset name.\nSource table(s) also need to be selected explicitly!", "Live Feed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        Cursor.Current = Cursors.WaitCursor;
                        FormJobSettings objFormJobSettings;
                        objFormJobSettings = new FormJobSettings(IsClone, "SQL Azure Database", drSelected.Cells[Helper.JOBNAME].Value.ToString(), drSelected.Cells[Helper.JOBINTERVAL].Value.ToString(), drSelected.Cells[Helper.JOBDATASET].Value.ToString());
                        Cursor.Current = Cursors.Default;

                        if (IsClone)
                            objFormJobSettings.NewJobCreated += FormJobSettings_NewJobCreated;
                        else//For Edit
                            objFormJobSettings.EditJobDetails += FormJobSettings_EditJobDetails;

                        objFormJobSettings.ShowDialog();
                    }
                    else
                        JobStatus.Text = "Job with running state cannot be modified / cloned!";
                }
                else
                    JobStatus.Text = "No Jobs is selected to edit!";
            }

            /// <summary>
            /// Purges Data if selected row is not in running state and based on user's response.
            /// </summary>
            private void PurgeData()
            {
                if (GridView.SelectedRows.Count > 0)
                {
                    DataGridViewRow drSelected = GridView.SelectedRows[0];
                    if (drSelected.Cells[Helper.JOBSTATUS].Value.ToString() == "Stopped" || drSelected.Cells[Helper.JOBSTATUS].Value.ToString() == "Failed")
                    {
                        DialogResult drUserResponse = MessageBox.Show("Are you sure you want to purge the selected job?", "Live Feed", MessageBoxButtons.YesNo, MessageBoxIcon.Question,MessageBoxDefaultButton.Button2);
                        if (drUserResponse == System.Windows.Forms.DialogResult.Yes)
                        {
                            XmlNode xnDestnDataset = m_xdData.SelectSingleNode("/Jobs/Job[@Name='" + drSelected.Cells[Helper.JOBNAME].Value.ToString() + "']");

                            if (xnDestnDataset != null)
                            {
                                XmlNode xnConnection = xnDestnDataset.SelectSingleNode("./Connection");
                                string sDestnDataset = xnConnection.Attributes["DatasetName"].Value.ToString();
                                string sTables = drSelected.Cells[Helper.JOBDATASET].Value.ToString();
                                string sPassword = Helper.Decrypt(xnConnection.Attributes["Password"].Value.ToString());
                                string datasetID = xnConnection.Attributes["DatasetID"].Value.ToString();

                                UserCredential uc = new UserCredential(xnConnection.Attributes["UserName"].Value.ToString(), sPassword);
                                PollPush objPollPush = new PollPush(uc, xnConnection.Attributes["ClientID"].Value.ToString());

                                if (sTables.Contains(","))//multiple tables in a job
                                {
                                    string[] sarrTables = sTables.Split(',');
                                    foreach (string sTableName in sarrTables)
                                    {
                                        objPollPush.PurgeData(sDestnDataset, sTableName, datasetID);
                                        xnConnection.Attributes["CurrentVersion"].Value = "0";
                                        xnDestnDataset.Attributes[Helper.JOBLASTPOLLED].Value = "";
                                        xnDestnDataset.SelectSingleNode("./JobMetadata[@Datasetname='" + sTableName + "']").Attributes["Version"].Value = "0";
                                        drSelected.Cells[Helper.JOBRECORDS].Value = "0";
                                        drSelected.Cells[Helper.JOBLASTPOLLED].Value = "";
                                        m_xdData.Save(Application.StartupPath + @"\JobSettings.xml");
                                    }
                                }
                                else
                                {
                                    objPollPush.PurgeData(sDestnDataset, sTables, datasetID);
                                    xnDestnDataset.SelectSingleNode("./JobMetadata[@Datasetname='" + sTables + "']").Attributes["Version"].Value = "0";
                                    xnDestnDataset.Attributes[Helper.JOBLASTPOLLED].Value = "";
                                    drSelected.Cells[Helper.JOBRECORDS].Value = "0";
                                    drSelected.Cells[Helper.JOBLASTPOLLED].Value = "";
                                    m_xdData.Save(Application.StartupPath + @"\JobSettings.xml");
                                }
                            }
                        }
                        
                    }
                    else
                        JobStatus.Text = "Job with running state cannot be purged!";
                }
                else
                    JobStatus.Text = "No Job is selected to purge!";
            }

            /// <summary>
            /// Deletes Data if selected row is not in running state and based on user's response.
            /// </summary>
            private void DeleteJob()
            {
                if (GridView.SelectedRows.Count > 0)
                {
                    DataGridViewRow drSelected = GridView.SelectedRows[0];
                    if (drSelected.Cells[Helper.JOBSTATUS].Value.ToString() == "Stopped" || drSelected.Cells[Helper.JOBSTATUS].Value.ToString() == "Failed")
                    {
                        DialogResult drUserResponse = MessageBox.Show("Are you sure you want to delete the selected job?", "Live Feed", MessageBoxButtons.YesNo, MessageBoxIcon.Question,MessageBoxDefaultButton.Button2);
                        if (drUserResponse == System.Windows.Forms.DialogResult.Yes)
                        {
                            DataTable dtSource = GridView.DataSource as DataTable;
                            if (dtSource != null)
                            {
                                string JobName = drSelected.Cells[Helper.JOBNAME].Value.ToString();
                                DataRow[] drDeletedRow = dtSource.Select("Name='" + JobName + "'");
                                if (drDeletedRow != null)
                                {
                                    drDeletedRow[0].Delete();
                                    XmlNode xnJobs = m_xdData.SelectSingleNode("/Jobs/Job[@Name='" + JobName + "']");
                                    m_xdData.DocumentElement.RemoveChild(xnJobs);
                                    m_xdData.Save(Application.StartupPath + @"\JobSettings.xml");
                                }
                            }
                        }
                    }
                    else
                        JobStatus.Text = "Job with running state cannot be deleted!";
                }
                else
                    JobStatus.Text = "No Jobs is selected to delete!";
                if (GridView.Rows.Count == 0)
                {
                    GridView.Visible = false;
                    JobStatus.Text = GridView.Rows.Count + " record(s) populated!";
                    deleteFeedToolStripMenuItem.Enabled = false;
                    editFeedToolStripMenuItem.Enabled = false;
                    purgeFeedToolStripMenuItem.Enabled = false;
                    cloneFeedToolStripMenuItem.Enabled = false;
                }
                
            }

            /// <summary>
            /// Aborts all running threads and closes the main form.
            /// </summary>
            private void AbortProcess()
            {
                if (m_hstJobCollections.Count > 0)
                {
                    IEnumerator objEnumerator = m_hstJobCollections.GetEnumerator();
                    DictionaryEntry objDictionaryEntry;
                    while (objEnumerator.MoveNext())
                    {
                        objDictionaryEntry = (DictionaryEntry)objEnumerator.Current;
                        Thread objService = objDictionaryEntry.Value as Thread;

                        objService.Abort();
                        objService.Join();

                        objService = null;

                    }
                    m_hstJobCollections.Clear();
                }
            }

            /// <summary>
            /// Initilaizes the Grid when loaded.
            /// </summary>
            private void InitializeGridView()
            {
                deleteFeedToolStripMenuItem.Enabled = true;
                editFeedToolStripMenuItem.Enabled = true;
                purgeFeedToolStripMenuItem.Enabled = true;
                cloneFeedToolStripMenuItem.Enabled = true;

                GridView.Columns[GridView.Columns.Count - 1].Width = 200;
                GridView.Columns[GridView.Columns.Count - 2].Width = 300;
                GridView.Columns[GridView.Columns.Count - 3].Width = 300;
                GridView.Columns[1].Width = 200;
                
                GridView.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                GridView.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
                GridView.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;
                GridView.Columns[4].SortMode = DataGridViewColumnSortMode.NotSortable;
                GridView.Columns[5].SortMode = DataGridViewColumnSortMode.NotSortable;
                GridView.Columns[6].SortMode = DataGridViewColumnSortMode.NotSortable;


                JobStatus.Text = GridView.Rows.Count + " record(s) populated!";
            }
        #endregion

           
                    
    }//end (FormMain)

}
