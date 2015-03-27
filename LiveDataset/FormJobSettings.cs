// This class creates or edits new job and save the details in XML
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.IO;

using PowerBIExtensionMethods;
using System.Web.Script.Serialization;
using System.Net;
using System.Threading;
using System.Collections;
using System.Xml;

namespace LiveDataset
{
    //This class creates or edits new job and save the details in XML
    public partial class FormJobSettings : Form
    {
        #region Member Variables

            private bool IsNewJob;//Indicates whether form is opened for new job or exiting job to edit.
            private string Source;//form controls changes based on source - currently source is SQL
            private XmlDocument xdData;//XML Doc - containings all job details
            private string ConnectionString = "";//SQL Connection string
            
            internal delegate void JobDetails(string JobName);
            internal event JobDetails NewJobCreated;//Invokes When new job created and saved
            internal event JobDetails EditJobDetails;//Invokes When existing job updated

        #endregion

        #region Constructor

            /// <summary>
            /// Initializs the form.
            /// </summary>
            public FormJobSettings()
            {
                InitializeComponent();
            }

            /// <summary>
            /// Parameterized constructor for creating new Job
            /// </summary>
            /// <param name="bIsNewJob">bool, indicates the call is for new job or existing job</param>
            /// <param name="sSource">string, containing Source</param>
            public FormJobSettings(bool bIsNewJob, string sSource)
            {
                InitializeComponent();

                xdData = new XmlDocument();
                xdData.Load(Application.StartupPath + @"\JobSettings.xml");

                Source = sSource;
                IsNewJob = bIsNewJob;
                this.Text += " (Create New Job)";

                chkSelectAll.Visible = false;
                listView2.Visible = false;
                JobSettingsStatus.Text = "Create new job settings!";

            }

            /// <summary>
            /// For Editing job details
            /// </summary>
            /// <param name="bIsNewJob">bool, indicates whether call is for existing job or Cloned/New Job</param>
            /// <param name="sSource">string, containing source - SQL, OData</param>
            /// <param name="JobName">string, containing name of job to update</param>
            /// <param name="Interval">string, containing interval of existing job</param>
            /// <param name="Dataset">string, containing datasets for existing job</param>
            public FormJobSettings(bool bIsNewJob, string sSource, string JobName, string Interval, string Dataset)
            {
                InitializeComponent();

                
                xdData = new XmlDocument();
                xdData.Load(Application.StartupPath + @"\JobSettings.xml");

                chkSelectAll.Visible = false;
                listView2.Visible = false;

                Source = sSource;
                IsNewJob = bIsNewJob;

                
                txtInterval.Value = Convert.ToDecimal(Interval);

                if (bIsNewJob)//Clone
                {
                    this.Text += " (Clone Job - " + JobName + ")";
                    txtJobName.Text = "";
                    JobSettingsStatus.Text = "Clone existing job!";
                }
                else
                {
                    this.Text += " (Edit Job - " + JobName + ")";
                    txtJobName.Enabled = false;
                    txtJobName.Text = JobName;
                    //Counter starts with 1 as Page 0 should be enabled in edit mode.
                    for (int iCounter = 1; iCounter < tabControl1.TabPages.Count;iCounter++)
                    {
                        foreach (Control ctlPage in tabControl1.TabPages[iCounter].Controls)
                            ctlPage.Enabled = false;
                    }
                    JobSettingsStatus.Text = "Edit job!";

                }
                FillControlValues(JobName);
                PopulateDatasets();

                if (bIsNewJob)
                    txtDatasetName.Text = "";
                else//the datasets will be checked during edit. Cloning should not checked this by default.
                if (listView1.Items.Count > 0)
                {
                    foreach (ListViewItem lvData in listView1.Items)
                    {
                        if (Dataset.Contains(","))//multiple tables in a job
                        {
                            string[] sarrTables = Dataset.Split(',');
                            foreach (string sTableName in sarrTables)
                                if (lvData.Text == sTableName)
                                    lvData.Checked = true;
                        }
                        else
                        {
                            if (lvData.Text == Dataset)
                                lvData.Checked = true;
                        }
                    }
                }

            }

        #endregion
   
        #region Control Events

            /// <summary>
            /// Validates the missing data and once validated Save all the values in XML
            /// Fires an event to update main form in the grid.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void btnSave_Click(object sender, EventArgs e)
            {
                try
                {
                    string Exception = "";
                    JobSettingsStatus.Text = "";

                    Cursor.Current = Cursors.WaitCursor;

                    if (!IsNewJob)//for editing, edit the Interval
                    {
                        EditData(txtJobName.Text);
                        if (EditJobDetails != null)
                            EditJobDetails(txtJobName.Text);
                        this.Close();
                        return;
                    }

                    if (string.IsNullOrEmpty(txtJobName.Text) || string.IsNullOrEmpty(txtInterval.Text) || string.IsNullOrEmpty(txtDatasetName.Text) || string.IsNullOrEmpty(txtClientID.Text) || string.IsNullOrEmpty(txtUserID.Text) || string.IsNullOrEmpty(txtPwd.Text) || string.IsNullOrEmpty(txtSeverName.Text) || string.IsNullOrEmpty(txtDbName.Text) || string.IsNullOrEmpty(txtSQLUserID.Text) || string.IsNullOrEmpty(txtSQLPwd.Text))
                    {
                        MessageBox.Show("Few field(s) are empty, please enter required values!", "Job Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        JobSettingsStatus.Text = "Few field(s) are empty!";
                        return;
                    }
                    if (TestJobDetails() == false)
                        return;
                    if (TestSourceConnection(out Exception) == false)
                    {
                        if (Exception != "")
                            JobSettingsStatus.Text = "Error - " + Exception;
                        return;
                    }//end (if (TestSourceConnection(out Exception) == false))

                    //Creates dataset
                    string[] NewDataset = null;
                    if (CreateDatasetPowerBI(out NewDataset) == false)
                        return;

                    if (NewDataset != null)//Retrieve ID from Power BI while creating new dataset
                    {
                        string sDatasetID = NewDataset[0].Split(':')[1].ToString();
                        SaveData(sDatasetID.Replace("\"", ""));
                        if (NewJobCreated != null)
                            NewJobCreated(txtJobName.Text);
                    }

                    this.Close();

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("AADSTS70001"))
                        MessageBox.Show("Destination client id or credentials are incorrect.", "Job Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    if (ex.Message.Length > 100)
                        JobSettingsStatus.Text = "Error - " + ex.Message.Substring(0, 100);
                    else
                        JobSettingsStatus.Text = "Error - " + ex.Message;
                    JobSettingsStatus.ToolTipText = "Error - " + ex.Message;
                    Log.WriteError(ex.Message);
                    Log.WriteError(ex.StackTrace);
                }
                Cursor.Current = Cursors.Default;
            }
          
            /// <summary>
            /// Validates destination connection.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void btnValidateBI_Click(object sender, EventArgs e)
            {
                try
                {
                    JobSettingsStatus.Text = "";
                    Cursor.Current = Cursors.WaitCursor;
                    TestDestinationConnection();
                    Cursor.Current = Cursors.Default;
                }
                catch (Exception ex)
                {
                    Cursor.Current = Cursors.Default;
                    if (ex.Message.Length > 100)
                        JobSettingsStatus.Text = "Error - " + ex.Message.Substring(0, 100);
                    else
                        JobSettingsStatus.Text = "Error - " + ex.Message;
                    JobSettingsStatus.ToolTipText = "Error - " + ex.Message;
                    Log.WriteError(ex.Message);
                }
                
            }

            /// <summary>
            /// Validates source connection
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void btnValidateSQL_Click(object sender, EventArgs e)
            {
                string Exception = "";
                try
                {
                    JobSettingsStatus.Text = "";
                    Cursor.Current = Cursors.WaitCursor;
                    TestSourceConnection(out Exception);
                    Cursor.Current = Cursors.Default;
                }
                catch (Exception ex)
                {
                    Cursor.Current = Cursors.Default;
                    if (ex.Message.Length > 100)
                        JobSettingsStatus.Text = "Error - " + ex.Message.Substring(0, 100);
                    else
                        JobSettingsStatus.Text = "Error - " + ex.Message;
                    JobSettingsStatus.ToolTipText = "Error - " + ex.Message;
                    Log.WriteError(ex.Message);
                }
                
            }

            /// <summary>
            /// Displays tables based on source connection if second tab selected.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
            {
                try
                {
                    if (string.IsNullOrEmpty(txtJobName.Text) && tabControl1.SelectedIndex != 0)
                    {
                        tabControl1.SelectedIndex = 0;
                        MessageBox.Show("Job name cannot be blank!", "Job Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtJobName.Focus();
                        return;
                    }
                    else if (IsNewJob && tabControl1.SelectedIndex != 0)
                    {
                        XmlDocument xdJobName = new XmlDocument();//New Instance to avoid any in memory changes.
                        xdJobName.Load(Application.StartupPath + @"\JobSettings.xml");
                        XmlNode xnJob = xdJobName.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']");
                        if (xnJob != null)
                        {
                            tabControl1.SelectedIndex = 0;
                            MessageBox.Show("Job name already exists!", "Job Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            
                            txtJobName.Focus();
                            return;
                        }
                    }
                    Cursor.Current = Cursors.WaitCursor;
                    JobSettingsStatus.Text = "";
                    JobSettingsStatus.ForeColor = Color.Black;

                    if (tabControl1.SelectedIndex == 2 && listView1.CheckedItems.Count == 0)
                        PopulateDatasets();
                    Cursor.Current = Cursors.Default;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Length > 100)
                        JobSettingsStatus.Text = "Error - " + ex.Message.Substring(0, 100);
                    else
                        JobSettingsStatus.Text = "Error - " + ex.Message;
                    JobSettingsStatus.ToolTipText = "Error - " + ex.Message;
                    Log.WriteError(ex.Message);
                    Log.WriteError(ex.StackTrace);
                }
            }

            /// <summary>
            /// Clicking cancel will not save any details, and will close form.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void btnCancel_Click(object sender, EventArgs e)
            {
                this.Close();
            }

            /// <summary>
            /// Check the selected list view item which is a table name has Change Tracking enabled or not. If not enable then that table cannot be selected
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
            {
                if (IsNewJob)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        string sError = "";
                        if (PollPush.IsChangeTrackingEnable(ConnectionString, listView1.Items[e.Index].Text, out sError) == false)
                        {
                            if (sError == "")
                            {
                                MessageBox.Show("'" + listView1.Items[e.Index].Text + "' table does not have Change Tracking enable, so this cannot be selected!", "Job Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                e.NewValue = CheckState.Unchecked;
                                listView1.Items[e.Index].Checked = false;
                            }
                            else
                            {
                                JobSettingsStatus.Text = "Error - " + sError;
                                JobSettingsStatus.ToolTipText = "Error - " + sError;
                            }
                        }
                        else
                        {
                            CreateNode(listView1.Items[e.Index].Text, listView1.Items[e.Index].Tag.ToString());
                            if (listView2.Visible && listView1.Items[e.Index].Selected)
                            {
                                ViewColumns(listView1.Items[e.Index], true);
                                if (listView2.CheckedItems.Count == listView2.Items.Count)
                                    chkSelectAll.Checked = true;
                                if (xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']") != null)
                                    SetDatasets();
                            }
                        }

                    }
                    else if (e.NewValue == CheckState.Unchecked)
                    {
                        if (listView1.Items[e.Index].Selected && listView2.CheckedItems.Count > 0)
                        {
                            foreach (ListViewItem lviChecked in listView2.CheckedItems)
                                lviChecked.Checked = false;
                        }
                        if (listView1.Items[e.Index] != null && listView1.CheckedItems.Count <= 1)
                        {
                            XmlNode xnJobData = xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']");
                            xdData.DocumentElement.RemoveChild(xnJobData);
                        }// end (if (listView1.SelectedItems.Count > 0))
                        else
                        {
                            XmlNode xnJobData = xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']");
                            XmlNode xnMetadata = xnJobData.SelectSingleNode("./JobMetadata[@Datasetname='" + listView1.Items[e.Index].Text + "']");
                            if (xnMetadata != null)
                                xnJobData.RemoveChild(xnMetadata);
                        }

                    }
                }
            }

            /// <summary>
            /// Populates columns for corresponding table.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void listView1_SelectedIndexChanged(object sender, EventArgs e)
            {
                listView2.Items.Clear();
               
                if (listView1.SelectedItems.Count > 0)
                {
                    chkSelectAll.Visible = true;
                    listView2.Visible = true;

                    ViewColumns(listView1.SelectedItems[0],false);
                }
                else
                {
                    chkSelectAll.Checked = false;
                    chkSelectAll.Visible = false;
                    listView2.Visible = false;
                }
            }

            /// <summary>
            /// Selects or Unselects all columns.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
            {
                foreach (ListViewItem lviColName in listView2.Items)
                { 
                    lviColName.Checked = chkSelectAll.Checked;
                }
            }

            /// <summary>
            /// Handles the check and unchek for columns. Check will add the column name in te XML node.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void listView2_ItemCheck(object sender, ItemCheckEventArgs e)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    XmlNode xnJobData = xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']/JobMetadata[@Datasetname='" + listView1.SelectedItems[0].Text + "']");
                    if (xnJobData != null)
                    {
                        if (xnJobData.Attributes["Columns"].Value.Contains(listView2.Items[e.Index].Text) == false)
                        {
                            if (xnJobData.Attributes["Columns"].Value == "")
                                xnJobData.Attributes["Columns"].Value = listView2.Items[e.Index].Text;
                            else
                                xnJobData.Attributes["Columns"].Value += "," + listView2.Items[e.Index].Text;
                        }//end (if (xnJobData.Attributes["Columns"].Value.Contains(listView2.Items[e.Index].Text) == false))
                    }
                    else
                    {
                        XmlNode xnJobs = xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']");
                        if (xnJobs == null)
                            CreateJobNode();

                        //New Child Node(s) for Job Metadata
                        XmlNode xnJob = xdData.CreateNode(XmlNodeType.Element, "JobMetadata", null);
                        XmlAttribute xaData = xdData.CreateAttribute("Datasetname");
                        xaData.Value = listView1.SelectedItems[0].Text;
                        xnJob.Attributes.Append(xaData);

                        xaData = xdData.CreateAttribute("Version");
                        xaData.Value = "0";
                        xnJob.Attributes.Append(xaData);

                        xaData = xdData.CreateAttribute(Helper.JOBRECORDS);
                        xaData.Value = "0";
                        xnJob.Attributes.Append(xaData);

                        xaData = xdData.CreateAttribute("Columns");

                        xaData.Value = listView2.Items[e.Index].Text;
                        xnJob.Attributes.Append(xaData);

                        xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']").AppendChild(xnJob);//Add to parent node

                    }

                }
                else if (e.NewValue == CheckState.Unchecked)
                {
                    XmlNode xnJobData = xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']/JobMetadata[@Datasetname='" + listView1.SelectedItems[0].Text + "']");
                    if (xnJobData.Attributes["Columns"].Value.Contains(listView2.Items[e.Index].Text))
                    {
                        string sColumns = xnJobData.Attributes["Columns"].Value;
                        string Substring = listView2.Items[e.Index].Text;
                        int Index1 = sColumns.IndexOf(Substring);

                        int Index2 = sColumns.LastIndexOf(Substring) + Substring.Length;
                        if (Substring.Length == sColumns.Length)
                            sColumns = sColumns.Remove(0);
                        else if (Index2 >= sColumns.Length)
                            sColumns = sColumns.Remove(Index1 - 1, Substring.Length + 1);
                        else
                            sColumns = sColumns.Remove(Index1, Substring.Length + 1);

                        xnJobData.Attributes["Columns"].Value = sColumns;
                    }
                }
            }

            /// <summary>
            /// Hide the control if no status is available for user.
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            private void JobSettingsStatus_TextChanged(object sender, EventArgs e)
            {
                if (string.IsNullOrEmpty(JobSettingsStatus.Text))
                    splitContainer1.Panel2Collapsed = true;
                else
                    splitContainer1.Panel2Collapsed = false;
            }

        #endregion    

        #region Private Methods

            /// <summary>
            /// Test source connection and return bool based on validation
            /// </summary>
            /// <param name="Exception">will return the exception if any</param>
            /// <returns>bool, true is connection validated successfully</returns>
            private bool TestSourceConnection(out string Exception)
            {
                if (string.IsNullOrEmpty(txtSeverName.Text) || string.IsNullOrEmpty(txtSQLUserID.Text) || string.IsNullOrEmpty(txtSQLPwd.Text))
                {
                    Exception = "";
                    JobSettingsStatus.Text = "Few field(s) value are missing in Source details!";
                    MessageBox.Show("Few field(s) value are missing in Source details!", "Job Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
                string sConnectionString = Helper.ConnectionString(txtSeverName.Text, txtDbName.Text, txtSQLUserID.Text, txtSQLPwd.Text);
                ConnectionString = sConnectionString;

                if (PollPush.ValidateSQLConnection(sConnectionString, out Exception))
                {
                    Exception = "";
                    JobSettingsStatus.Text = "Source Connection validated!";
                    return true;
                }
                else
                {
                    JobSettingsStatus.Text = "Error - " + "Source Connection is not valid! " + Exception;
                    JobSettingsStatus.ToolTipText = "Error - " + "Source Connection is not valid! " + Exception;
                    return false;
                }
            }

            /// <summary>
            /// Test destination connection and return bool based on validation
            /// </summary>
            /// <returns>bool, true is connection validated successfully</returns>
            private bool TestDestinationConnection()
            {
                bool bIsValidated = false;
                PollPush objPollPush;

                if (string.IsNullOrEmpty(txtDatasetName.Text) || string.IsNullOrEmpty(txtClientID.Text) || string.IsNullOrEmpty(txtUserID.Text) || string.IsNullOrEmpty(txtPwd.Text))
                {
                    JobSettingsStatus.Text = "Few field(s) value are missing in Destination details!";
                    MessageBox.Show("Few field(s) value are missing in Destination details!", "Job Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }

                if (listView1.CheckedItems.Count == 0)
                {
                    MessageBox.Show("No table(s) are selected in Source dataset tab, please select at-least one table!","Job Settings",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                    bIsValidated = false;
                    return bIsValidated;
                }

                
                UserCredential uc = new UserCredential(txtUserID.Text, txtPwd.Text);

                if (uc == null)
                {
                    MessageBox.Show("User credentials provided are incorrect!", "Job Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    bIsValidated = false;

                    return bIsValidated;
                }
                objPollPush = new PollPush(uc, txtClientID.Text);
                string Error;
                if (objPollPush.ValidatePowerBIConnecton(out Error))
                {
                    JobSettingsStatus.Text = "Destination Connection is validated!";
                    return true;
                }
                else
                {
                    if (Error != "")
                    {
                        if (Error.Contains("AADSTS70001"))
                            MessageBox.Show("Destination client id or credentials are incorrect.", "Job Settings", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        JobSettingsStatus.Text = "Error - " + Error;
                        JobSettingsStatus.ToolTipText = "Error - " + Error;
                    }
                    else
                        JobSettingsStatus.Text = "Destination Connection is not validated!";
                    bIsValidated = false;
                }
                return bIsValidated;

            }

            /// <summary>
            /// Test Job name and Interval and return bool based on validation
            /// </summary>
            /// <returns>bool, true is job details validated successfully</returns>
            private bool TestJobDetails()
            {
                if (string.IsNullOrEmpty(txtJobName.Text) || string.IsNullOrEmpty(txtInterval.Text))
                {
                    JobSettingsStatus.Text = "Few field(s) value are missing in Job Details!";
                    MessageBox.Show("Few field(s) value are missing in Job Details!", "Job Settings", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
                else
                {
                    return true;
                }
            }

            /// <summary>
            /// Create new attributes for XML and Saves the data in xml
            /// </summary>
            private void SaveData(string DatasetID)
            {
               XmlAttribute xaData;

                XmlNode xnJobs = xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']");
                if (xnJobs == null)
                    xnJobs = CreateJobNode();
                else
                    SetDatasets();

                //New Child Node
                XmlNode xnConnection = xdData.CreateNode(XmlNodeType.Element, "Connection", null);
                xaData = xdData.CreateAttribute("Source");
                xaData.Value = Source;
                xnConnection.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute("SourceServerName");
                xaData.Value = txtSeverName.Text;
                xnConnection.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute("SourceDatabaseName");
                xaData.Value = txtDbName.Text;
                xnConnection.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute("SourceUserID");
                xaData.Value = txtSQLUserID.Text;
                xnConnection.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute("SourcePwd");
                xaData.Value = Helper.Encrypt(txtSQLPwd.Text);
                xnConnection.Attributes.Append(xaData);


                xaData = xdData.CreateAttribute("ClientID");
                xaData.Value = txtClientID.Text;
                xnConnection.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute("UserName");
                xaData.Value = txtUserID.Text;
                xnConnection.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute("Password");
                xaData.Value = Helper.Encrypt(txtPwd.Text);
                xnConnection.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute("DatasetName");
                xaData.Value = txtDatasetName.Text;
                xnConnection.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute("CurrentVersion");
                xaData.Value = "0";
                xnConnection.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute("IsFullLoad");
                xaData.Value = chkIsFullLoad.Checked.ToString();
                xnConnection.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute("DatasetID");
                xaData.Value = DatasetID;
                xnConnection.Attributes.Append(xaData);

                xnJobs.AppendChild(xnConnection);

                //New Child Node(s) for Job Metadata
                foreach (ListViewItem lviChecked in listView1.CheckedItems)
                {
                    XmlNode xnJobMetadata = xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']/JobMetadata[@Datasetname='" + lviChecked.Text + "']");
                    if (xnJobMetadata != null)
                    {
                        lviChecked.Tag = xnJobMetadata.Attributes["Columns"].Value;
                    }
                    else
                    {
                        XmlNode xnJob = xdData.CreateNode(XmlNodeType.Element, "JobMetadata", null);
                        xaData = xdData.CreateAttribute("Datasetname");
                        xaData.Value = lviChecked.Text;
                        xnJob.Attributes.Append(xaData);

                        xaData = xdData.CreateAttribute("Version");
                        xaData.Value = "0";
                        xnJob.Attributes.Append(xaData);

                        xaData = xdData.CreateAttribute(Helper.JOBRECORDS);
                        xaData.Value = "0";
                        xnJob.Attributes.Append(xaData);

                        xaData = xdData.CreateAttribute("Columns");
                        if (lviChecked.Tag != null)
                            xaData.Value = lviChecked.Tag.ToString();
                        else
                            xaData.Value = "";
                        xnJob.Attributes.Append(xaData);

                        xnJobs.AppendChild(xnJob);//Add to parent node
                    }

                }//end (foreach (ListViewItem lviChecked in listView1.CheckedItems)

               
                xdData.Save(Application.StartupPath + @"\JobSettings.xml");
            }

            /// <summary>
            /// Fill the data in the controls based on Job Name from XML
            /// </summary>
            /// <param name="JobName">string, Job name to get the existing values from XML</param>
            private void FillControlValues(string JobName)
            {
                XmlNode xnJobData = xdData.SelectSingleNode("/Jobs/Job[@Name='" + JobName + "']/Connection");
                if (xnJobData != null)
                {
                    txtSeverName.Text = xnJobData.Attributes["SourceServerName"].Value;
                    txtDbName.Text = xnJobData.Attributes["SourceDatabaseName"].Value;
                    txtSQLUserID.Text = xnJobData.Attributes["SourceUserID"].Value;
                    txtSQLPwd.Text = Helper.Decrypt(xnJobData.Attributes["SourcePwd"].Value);

                    txtUserID.Text = xnJobData.Attributes["UserName"].Value;
                    txtPwd.Text = Helper.Decrypt(xnJobData.Attributes["Password"].Value);
                    txtClientID.Text = xnJobData.Attributes["ClientID"].Value;
                    txtDatasetName.Text = xnJobData.Attributes["DatasetName"].Value;

                    chkIsFullLoad.Checked = Convert.ToBoolean(xnJobData.Attributes["IsFullLoad"].Value);
                }

            }
        
            /// <summary>
            /// Saves the Edited data based on Job Name.
            /// </summary>
            /// <param name="JobName">string, Job name to edit same job details</param>
            private void EditData(string JobName)
            {
                xdData.Load(Application.StartupPath + @"\JobSettings.xml");//Reload XML again
                XmlNode xnJobData = xdData.SelectSingleNode("/Jobs/Job[@Name='" + JobName + "']");
                if (xnJobData != null)
                {
                    xnJobData.Attributes[Helper.JOBINTERVAL].Value = txtInterval.Value.ToString();
                    xdData.Save(Application.StartupPath + @"\JobSettings.xml");
                }
            }

            /// <summary>
            /// Populates all tables for the source connection string
            /// </summary>
            private void PopulateDatasets()
            {
                if (txtSeverName.Text != "" && txtDbName.Text != "" && txtSQLUserID.Text != "")
                {
                    listView1.Items.Clear();

                    string sConnectionString = Helper.ConnectionString(txtSeverName.Text, txtDbName.Text, txtSQLUserID.Text, txtSQLPwd.Text);
                    ConnectionString = sConnectionString;
                    string Exception = "";
                    if (TestSourceConnection(out Exception))
                    {
                        PollPush objPollPush = new PollPush(null, null);
                        objPollPush.ConnectionString = sConnectionString;
                        DataSet dsTables = objPollPush.ListAllDatasets();

                        if (dsTables != null)
                        {
                            if (dsTables.Tables.Count > 0)
                            {
                                ListViewItem lviDataSetName = null;
                                foreach (DataRow drData in dsTables.Tables[0].Rows)
                                {
                                    lviDataSetName = new ListViewItem(drData["name"].ToString());
                                    DataSet dsColumns = objPollPush.PopulateColumns(drData["name"].ToString());
                                    lviDataSetName.Tag = PopulateColumns(dsColumns);
                                    lviDataSetName.ToolTipText = drData["name"].ToString();
                                    listView1.Items.Add(lviDataSetName);
                                }
                            }

                            JobSettingsStatus.Text = "Source Connection Datasets populated!";

                        }
                        else
                            JobSettingsStatus.Text = "No dataset exists for given Source Connection!";
                    }
                    else
                    {
                        if (Exception == "")
                            JobSettingsStatus.Text = "Source Connection is not valid, datasets cannot be loaded!";
                        else
                        { 
                            JobSettingsStatus.Text = "Error - " + Exception; 
                            JobSettingsStatus.ToolTipText = "Error - " + Exception; 
                        }
                    }
                }
            }

            /// <summary>
            /// Creates the dataset in Power BI when user clicks save. Dataset will be created based on destination connection string and source tables
            /// </summary>
            /// <returns>bool, true if dataset does not exists while creating job</returns>
            private bool CreateDatasetPowerBI(out string[] NewDataset)
            {
                NewDataset = null;
               
                //Check the datasets exists or not...
                UserCredential uc = new UserCredential(txtUserID.Text, txtPwd.Text);
                PollPush objPollPush = new PollPush(uc,txtClientID.Text);
                bool bIsValidated = false;
                var datasets = objPollPush.GetAllDatasets().Datasets(txtDatasetName.Text);

                if (datasets.Count() != 0)
                {
                    MessageBox.Show("'" + txtDatasetName.Text.ToUpper() + "' dataset already exists! Delete it before attempting to re-create.", "Job Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtDatasetName.Text = "";
                    txtDatasetName.Focus();
                    bIsValidated = false;
                }
                else
                {
                    //Create new dataset
                    objPollPush.ConnectionString = ConnectionString;
                    objPollPush.PowerBIDatasetName = txtDatasetName.Text;
                    objPollPush.IsInitialLoad = chkIsFullLoad.Checked;
                    string sTableName = "";
                    foreach (ListViewItem lviChecked in listView1.CheckedItems)
                    {
                        if (sTableName == "")
                            sTableName = lviChecked.Text;
                        else
                            sTableName += "," + lviChecked.Text;
                    }

                   bIsValidated = objPollPush.CreateDatasets(sTableName,xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']"),out NewDataset);
                    
                }

                return bIsValidated;
            }

            /// <summary>
            /// Checks column name exists in XML or not
            /// </summary>
            /// <param name="xnJobData">XMLNode, to get details of column values</param>
            /// <param name="TableName">string Tablename, to check columns is allowed for given table name</param>
            /// <param name="ColName">string, name of columns to check columns is allowed for given table name</param>
            /// <returns></returns>
            private bool IsColumnChecked(XmlNode xnJobData, string TableName, string ColName)
            {
                bool bIsColumnChecked = false;
                string sColumns = xnJobData.Attributes["Columns"].Value.ToString();
                if (sColumns.Contains(","))
                {
                    string[] sarrColumns = sColumns.Split(',');
                    foreach (string sCol in sarrColumns)
                        if (sCol == ColName)
                            return true;
                        else
                            bIsColumnChecked = false;
                }
                else if (sColumns == ColName)
                    return true;

                return bIsColumnChecked;
            }

            /// <summary>
            /// Populates columns in a string.
            /// </summary>
            /// <param name="dsColumns">DataSet, contains all columns</param>
            /// <returns>string, comma seperated columns</returns>
            private string PopulateColumns(DataSet dsColumns)
            {
                string sColumns = "";
                if (dsColumns != null)
                {
                    if (dsColumns.Tables.Count > 0)
                    {
                        foreach (DataRow drData in dsColumns.Tables[0].Rows)
                            if (sColumns == "")
                                sColumns = drData["Name"].ToString();
                            else
                                sColumns += "," + drData["Name"].ToString();
                    }
                }
                return sColumns;
            }

            /// <summary>
            /// View Columns in list view for selected table.
            /// </summary>
            /// <param name="lviItem">ListViewItem, containings table name for getting columns</param>
            private void ViewColumns(ListViewItem lviItem, bool IsNewCheck)
            {
                chkSelectAll.Enabled = true;
                listView2.Items.Clear();
                bool bIsColumnChecked = false;

                XmlNode xnJobData = xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']/JobMetadata[@Datasetname='" + lviItem.Text + "']");

                if (xnJobData == null)
                    bIsColumnChecked = true;

                string[] sarrColName = lviItem.Tag.ToString().Split(',');
                ListViewItem lviDataSetName = null;
                foreach (string sColname in sarrColName)
                {
                    lviDataSetName = new ListViewItem(sColname);

                    if (bIsColumnChecked)
                    {
                        if (lviItem.Checked || IsNewCheck)
                        {
                            lviDataSetName.Checked = bIsColumnChecked;
                            chkSelectAll.Checked = bIsColumnChecked;
                        }
                    }
                    else
                        if (lviItem.Checked || IsNewCheck)
                            lviDataSetName.Checked = IsColumnChecked(xnJobData, lviItem.Text, sColname);

                    lviDataSetName.ToolTipText = sColname;
                    listView2.Items.Add(lviDataSetName);
                }
            }

            /// <summary>
            /// Creates Job node for XML.
            /// </summary>
            private XmlNode CreateJobNode()
            {
                XmlNode xnDocumentElement = xdData.SelectSingleNode("Jobs");

                XmlNode xnJobs = xdData.CreateNode(XmlNodeType.Element, "Job", null);

                XmlAttribute xaData;
                xaData = xdData.CreateAttribute(Helper.JOBNAME);
                xaData.Value = txtJobName.Text;
                xnJobs.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute(Helper.JOBSTATUS);
                xaData.Value = "Stopped";
                xnJobs.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute(Helper.JOBINTERVAL);
                xaData.Value = txtInterval.Text;
                xnJobs.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute(Helper.JOBDATASET);
                string sTableName = "";
                //Multiple tables
                foreach (ListViewItem lviChecked in listView1.CheckedItems)
                {
                    if (sTableName == "")
                        sTableName = lviChecked.Text;
                    else
                        sTableName += "," + lviChecked.Text;
                }
                xaData.Value = sTableName;
                xnJobs.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute(Helper.JOBLASTPOLLED);
                xaData.Value = "";
                xnJobs.Attributes.Append(xaData);

                xaData = xdData.CreateAttribute(Helper.JOBRECORDS);
                xaData.Value = "0";
                xnJobs.Attributes.Append(xaData);
                xnDocumentElement.AppendChild(xnJobs);

                return xnJobs;

            }

            /// <summary>
            /// Creates Job Metadata node during table selection.
            /// </summary>
            /// <param name="TableName">string, table name to add data in xml</param>
            /// <param name="Columns">string, column name to add data in xml</param>
            private void CreateNode(string TableName, string Columns)
            {
                 XmlNode xnJobs = xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']");
                if (xnJobs == null)
                    CreateJobNode();

                //New Child Node(s) for Job Metadata
                XmlNode xnJobMetadata = xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']/JobMetadata[@Datasetname='" + TableName + "']");
                if (xnJobMetadata == null)
                {
                    XmlNode xnJob = xdData.CreateNode(XmlNodeType.Element, "JobMetadata", null);
                    XmlAttribute xaData = xdData.CreateAttribute("Datasetname");
                    xaData.Value = TableName;
                    xnJob.Attributes.Append(xaData);

                    xaData = xdData.CreateAttribute("Version");
                    xaData.Value = "0";
                    xnJob.Attributes.Append(xaData);

                    xaData = xdData.CreateAttribute(Helper.JOBRECORDS);
                    xaData.Value = "0";
                    xnJob.Attributes.Append(xaData);

                    xaData = xdData.CreateAttribute("Columns");

                    xaData.Value = Columns;
                    xnJob.Attributes.Append(xaData);

                    xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']").AppendChild(xnJob);//Add to parent node
                }
                else
                    xnJobMetadata.Attributes["Columns"].Value = Columns;

            }

            /// <summary>
            /// Update xml with checked tables.
            /// </summary>
            private void SetDatasets()
            {
                //Multiple tables
                string sTableName = "";
                foreach (ListViewItem lviChecked in listView1.CheckedItems)
                {
                    if (sTableName == "")
                        sTableName = lviChecked.Text;
                    else
                        sTableName += "," + lviChecked.Text;
                }
                xdData.SelectSingleNode("/Jobs/Job[@Name='" + txtJobName.Text + "']").Attributes[Helper.JOBDATASET].Value = sTableName;
            }

        #endregion     
                       
    }//end (FormJobSettings)

}

