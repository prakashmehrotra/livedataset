namespace LiveDataset
{
    partial class FormJobSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormJobSettings));
            this.lblServer = new System.Windows.Forms.Label();
            this.lblUserID = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tbJobDetails = new System.Windows.Forms.TabPage();
            this.LblInterval = new System.Windows.Forms.Label();
            this.lblSeconds = new System.Windows.Forms.Label();
            this.txtInterval = new System.Windows.Forms.NumericUpDown();
            this.lblJob = new System.Windows.Forms.Label();
            this.txtJobName = new System.Windows.Forms.TextBox();
            this.tbSource = new System.Windows.Forms.TabPage();
            this.txtDbName = new System.Windows.Forms.TextBox();
            this.lblSQLDb = new System.Windows.Forms.Label();
            this.btnValidateSQL = new System.Windows.Forms.Button();
            this.lblSQLPwd = new System.Windows.Forms.Label();
            this.txtSQLPwd = new System.Windows.Forms.TextBox();
            this.txtSQLUserID = new System.Windows.Forms.TextBox();
            this.txtSeverName = new System.Windows.Forms.TextBox();
            this.tbDatasetDetails = new System.Windows.Forms.TabPage();
            this.chkSelectAll = new System.Windows.Forms.CheckBox();
            this.listView2 = new System.Windows.Forms.ListView();
            this.chkIsFullLoad = new System.Windows.Forms.CheckBox();
            this.listView1 = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tbDestination = new System.Windows.Forms.TabPage();
            this.lblDatasetName = new System.Windows.Forms.Label();
            this.txtDatasetName = new System.Windows.Forms.TextBox();
            this.btnValidateBI = new System.Windows.Forms.Button();
            this.lblDestPwd = new System.Windows.Forms.Label();
            this.lblDestUserID = new System.Windows.Forms.Label();
            this.lblClient = new System.Windows.Forms.Label();
            this.txtPwd = new System.Windows.Forms.TextBox();
            this.txtUserID = new System.Windows.Forms.TextBox();
            this.txtClientID = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.JobSettingsCtrl = new System.Windows.Forms.StatusStrip();
            this.JobSettingsStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tabControl1.SuspendLayout();
            this.tbJobDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtInterval)).BeginInit();
            this.tbSource.SuspendLayout();
            this.tbDatasetDetails.SuspendLayout();
            this.tbDestination.SuspendLayout();
            this.JobSettingsCtrl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblServer.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblServer.Location = new System.Drawing.Point(27, 18);
            this.lblServer.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(82, 15);
            this.lblServer.TabIndex = 0;
            this.lblServer.Text = "Server Name:";
            this.lblServer.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblUserID
            // 
            this.lblUserID.AutoSize = true;
            this.lblUserID.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblUserID.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblUserID.Location = new System.Drawing.Point(58, 70);
            this.lblUserID.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUserID.Name = "lblUserID";
            this.lblUserID.Size = new System.Drawing.Size(51, 15);
            this.lblUserID.TabIndex = 3;
            this.lblUserID.Text = "User ID:";
            this.lblUserID.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tbJobDetails);
            this.tabControl1.Controls.Add(this.tbSource);
            this.tabControl1.Controls.Add(this.tbDatasetDetails);
            this.tabControl1.Controls.Add(this.tbDestination);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(494, 178);
            this.tabControl1.TabIndex = 0;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tbJobDetails
            // 
            this.tbJobDetails.BackColor = System.Drawing.SystemColors.Control;
            this.tbJobDetails.Controls.Add(this.LblInterval);
            this.tbJobDetails.Controls.Add(this.lblSeconds);
            this.tbJobDetails.Controls.Add(this.txtInterval);
            this.tbJobDetails.Controls.Add(this.lblJob);
            this.tbJobDetails.Controls.Add(this.txtJobName);
            this.tbJobDetails.Location = new System.Drawing.Point(4, 25);
            this.tbJobDetails.Name = "tbJobDetails";
            this.tbJobDetails.Padding = new System.Windows.Forms.Padding(3);
            this.tbJobDetails.Size = new System.Drawing.Size(486, 149);
            this.tbJobDetails.TabIndex = 0;
            this.tbJobDetails.Text = "Job Details";
            // 
            // LblInterval
            // 
            this.LblInterval.AutoSize = true;
            this.LblInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.LblInterval.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.LblInterval.Location = new System.Drawing.Point(17, 44);
            this.LblInterval.Name = "LblInterval";
            this.LblInterval.Size = new System.Drawing.Size(73, 15);
            this.LblInterval.TabIndex = 22;
            this.LblInterval.Text = "Poll Interval:";
            // 
            // lblSeconds
            // 
            this.lblSeconds.AutoSize = true;
            this.lblSeconds.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblSeconds.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblSeconds.Location = new System.Drawing.Point(222, 44);
            this.lblSeconds.Name = "lblSeconds";
            this.lblSeconds.Size = new System.Drawing.Size(53, 15);
            this.lblSeconds.TabIndex = 21;
            this.lblSeconds.Text = "seconds";
            // 
            // txtInterval
            // 
            this.txtInterval.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtInterval.Location = new System.Drawing.Point(96, 42);
            this.txtInterval.Maximum = new decimal(new int[] {
            900,
            0,
            0,
            0});
            this.txtInterval.Name = "txtInterval";
            this.txtInterval.Size = new System.Drawing.Size(120, 21);
            this.txtInterval.TabIndex = 2;
            this.txtInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtInterval.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // lblJob
            // 
            this.lblJob.AutoSize = true;
            this.lblJob.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblJob.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblJob.Location = new System.Drawing.Point(23, 16);
            this.lblJob.Name = "lblJob";
            this.lblJob.Size = new System.Drawing.Size(67, 15);
            this.lblJob.TabIndex = 16;
            this.lblJob.Text = "Job Name:";
            // 
            // txtJobName
            // 
            this.txtJobName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtJobName.ForeColor = System.Drawing.Color.Black;
            this.txtJobName.Location = new System.Drawing.Point(96, 13);
            this.txtJobName.Name = "txtJobName";
            this.txtJobName.Size = new System.Drawing.Size(291, 21);
            this.txtJobName.TabIndex = 1;
            // 
            // tbSource
            // 
            this.tbSource.BackColor = System.Drawing.SystemColors.Control;
            this.tbSource.Controls.Add(this.txtDbName);
            this.tbSource.Controls.Add(this.lblSQLDb);
            this.tbSource.Controls.Add(this.btnValidateSQL);
            this.tbSource.Controls.Add(this.lblSQLPwd);
            this.tbSource.Controls.Add(this.txtSQLPwd);
            this.tbSource.Controls.Add(this.txtSQLUserID);
            this.tbSource.Controls.Add(this.txtSeverName);
            this.tbSource.Controls.Add(this.lblUserID);
            this.tbSource.Controls.Add(this.lblServer);
            this.tbSource.Location = new System.Drawing.Point(4, 25);
            this.tbSource.Name = "tbSource";
            this.tbSource.Padding = new System.Windows.Forms.Padding(3);
            this.tbSource.Size = new System.Drawing.Size(486, 150);
            this.tbSource.TabIndex = 1;
            this.tbSource.Text = "Source Details";
            // 
            // txtDbName
            // 
            this.txtDbName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtDbName.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.txtDbName.Location = new System.Drawing.Point(116, 42);
            this.txtDbName.Name = "txtDbName";
            this.txtDbName.Size = new System.Drawing.Size(359, 21);
            this.txtDbName.TabIndex = 1;
            // 
            // lblSQLDb
            // 
            this.lblSQLDb.AutoSize = true;
            this.lblSQLDb.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblSQLDb.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblSQLDb.Location = new System.Drawing.Point(9, 44);
            this.lblSQLDb.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSQLDb.Name = "lblSQLDb";
            this.lblSQLDb.Size = new System.Drawing.Size(100, 15);
            this.lblSQLDb.TabIndex = 21;
            this.lblSQLDb.Text = "Database Name:";
            this.lblSQLDb.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btnValidateSQL
            // 
            this.btnValidateSQL.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnValidateSQL.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnValidateSQL.Location = new System.Drawing.Point(335, 122);
            this.btnValidateSQL.Name = "btnValidateSQL";
            this.btnValidateSQL.Size = new System.Drawing.Size(140, 26);
            this.btnValidateSQL.TabIndex = 4;
            this.btnValidateSQL.Text = "Validate Connection";
            this.btnValidateSQL.UseVisualStyleBackColor = true;
            this.btnValidateSQL.Click += new System.EventHandler(this.btnValidateSQL_Click);
            // 
            // lblSQLPwd
            // 
            this.lblSQLPwd.AutoSize = true;
            this.lblSQLPwd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblSQLPwd.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblSQLPwd.Location = new System.Drawing.Point(45, 96);
            this.lblSQLPwd.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSQLPwd.Name = "lblSQLPwd";
            this.lblSQLPwd.Size = new System.Drawing.Size(64, 15);
            this.lblSQLPwd.TabIndex = 19;
            this.lblSQLPwd.Text = "Password:";
            this.lblSQLPwd.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtSQLPwd
            // 
            this.txtSQLPwd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtSQLPwd.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.txtSQLPwd.Location = new System.Drawing.Point(116, 96);
            this.txtSQLPwd.Name = "txtSQLPwd";
            this.txtSQLPwd.Size = new System.Drawing.Size(359, 21);
            this.txtSQLPwd.TabIndex = 3;
            this.txtSQLPwd.UseSystemPasswordChar = true;
            // 
            // txtSQLUserID
            // 
            this.txtSQLUserID.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtSQLUserID.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.txtSQLUserID.Location = new System.Drawing.Point(116, 69);
            this.txtSQLUserID.Name = "txtSQLUserID";
            this.txtSQLUserID.Size = new System.Drawing.Size(359, 21);
            this.txtSQLUserID.TabIndex = 2;
            // 
            // txtSeverName
            // 
            this.txtSeverName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtSeverName.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.txtSeverName.Location = new System.Drawing.Point(116, 15);
            this.txtSeverName.Name = "txtSeverName";
            this.txtSeverName.Size = new System.Drawing.Size(359, 21);
            this.txtSeverName.TabIndex = 0;
            // 
            // tbDatasetDetails
            // 
            this.tbDatasetDetails.BackColor = System.Drawing.SystemColors.Control;
            this.tbDatasetDetails.Controls.Add(this.chkSelectAll);
            this.tbDatasetDetails.Controls.Add(this.listView2);
            this.tbDatasetDetails.Controls.Add(this.chkIsFullLoad);
            this.tbDatasetDetails.Controls.Add(this.listView1);
            this.tbDatasetDetails.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.tbDatasetDetails.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tbDatasetDetails.Location = new System.Drawing.Point(4, 25);
            this.tbDatasetDetails.Name = "tbDatasetDetails";
            this.tbDatasetDetails.Size = new System.Drawing.Size(486, 150);
            this.tbDatasetDetails.TabIndex = 2;
            this.tbDatasetDetails.Text = "Source Datasets";
            // 
            // chkSelectAll
            // 
            this.chkSelectAll.AutoSize = true;
            this.chkSelectAll.Enabled = false;
            this.chkSelectAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkSelectAll.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.chkSelectAll.Location = new System.Drawing.Point(253, 11);
            this.chkSelectAll.Name = "chkSelectAll";
            this.chkSelectAll.Size = new System.Drawing.Size(91, 19);
            this.chkSelectAll.TabIndex = 4;
            this.chkSelectAll.Text = "All Columns";
            this.chkSelectAll.UseVisualStyleBackColor = true;
            this.chkSelectAll.CheckedChanged += new System.EventHandler(this.chkSelectAll_CheckedChanged);
            // 
            // listView2
            // 
            this.listView2.BackColor = System.Drawing.SystemColors.Control;
            this.listView2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView2.CheckBoxes = true;
            this.listView2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.listView2.Location = new System.Drawing.Point(253, 36);
            this.listView2.Name = "listView2";
            this.listView2.ShowItemToolTips = true;
            this.listView2.Size = new System.Drawing.Size(223, 110);
            this.listView2.TabIndex = 3;
            this.listView2.UseCompatibleStateImageBehavior = false;
            this.listView2.View = System.Windows.Forms.View.List;
            this.listView2.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listView2_ItemCheck);
            // 
            // chkIsFullLoad
            // 
            this.chkIsFullLoad.AutoSize = true;
            this.chkIsFullLoad.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.chkIsFullLoad.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.chkIsFullLoad.Location = new System.Drawing.Point(5, 9);
            this.chkIsFullLoad.Name = "chkIsFullLoad";
            this.chkIsFullLoad.Size = new System.Drawing.Size(86, 19);
            this.chkIsFullLoad.TabIndex = 2;
            this.chkIsFullLoad.Text = "Initial Load";
            this.chkIsFullLoad.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            this.listView1.BackColor = System.Drawing.SystemColors.Control;
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView1.CheckBoxes = true;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName});
            this.listView1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.listView1.Location = new System.Drawing.Point(3, 36);
            this.listView1.MultiSelect = false;
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(244, 110);
            this.listView1.TabIndex = 1;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listView1_ItemCheck);
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // colName
            // 
            this.colName.Text = "Available Tables";
            this.colName.Width = 239;
            // 
            // tbDestination
            // 
            this.tbDestination.BackColor = System.Drawing.SystemColors.Control;
            this.tbDestination.Controls.Add(this.lblDatasetName);
            this.tbDestination.Controls.Add(this.txtDatasetName);
            this.tbDestination.Controls.Add(this.btnValidateBI);
            this.tbDestination.Controls.Add(this.lblDestPwd);
            this.tbDestination.Controls.Add(this.lblDestUserID);
            this.tbDestination.Controls.Add(this.lblClient);
            this.tbDestination.Controls.Add(this.txtPwd);
            this.tbDestination.Controls.Add(this.txtUserID);
            this.tbDestination.Controls.Add(this.txtClientID);
            this.tbDestination.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.5F);
            this.tbDestination.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.tbDestination.Location = new System.Drawing.Point(4, 25);
            this.tbDestination.Name = "tbDestination";
            this.tbDestination.Size = new System.Drawing.Size(486, 150);
            this.tbDestination.TabIndex = 3;
            this.tbDestination.Text = "Destination Details";
            // 
            // lblDatasetName
            // 
            this.lblDatasetName.AutoSize = true;
            this.lblDatasetName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblDatasetName.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblDatasetName.Location = new System.Drawing.Point(7, 42);
            this.lblDatasetName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDatasetName.Name = "lblDatasetName";
            this.lblDatasetName.Size = new System.Drawing.Size(89, 15);
            this.lblDatasetName.TabIndex = 27;
            this.lblDatasetName.Text = "Dataset Name:";
            this.lblDatasetName.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtDatasetName
            // 
            this.txtDatasetName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtDatasetName.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.txtDatasetName.Location = new System.Drawing.Point(103, 39);
            this.txtDatasetName.Name = "txtDatasetName";
            this.txtDatasetName.Size = new System.Drawing.Size(375, 21);
            this.txtDatasetName.TabIndex = 1;
            // 
            // btnValidateBI
            // 
            this.btnValidateBI.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnValidateBI.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnValidateBI.Location = new System.Drawing.Point(338, 120);
            this.btnValidateBI.Name = "btnValidateBI";
            this.btnValidateBI.Size = new System.Drawing.Size(140, 26);
            this.btnValidateBI.TabIndex = 4;
            this.btnValidateBI.Text = "Validate Connection";
            this.btnValidateBI.UseVisualStyleBackColor = true;
            this.btnValidateBI.Click += new System.EventHandler(this.btnValidateBI_Click);
            // 
            // lblDestPwd
            // 
            this.lblDestPwd.AutoSize = true;
            this.lblDestPwd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblDestPwd.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblDestPwd.Location = new System.Drawing.Point(32, 93);
            this.lblDestPwd.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDestPwd.Name = "lblDestPwd";
            this.lblDestPwd.Size = new System.Drawing.Size(64, 15);
            this.lblDestPwd.TabIndex = 25;
            this.lblDestPwd.Text = "Password:";
            this.lblDestPwd.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblDestUserID
            // 
            this.lblDestUserID.AutoSize = true;
            this.lblDestUserID.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblDestUserID.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblDestUserID.Location = new System.Drawing.Point(45, 69);
            this.lblDestUserID.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDestUserID.Name = "lblDestUserID";
            this.lblDestUserID.Size = new System.Drawing.Size(51, 15);
            this.lblDestUserID.TabIndex = 24;
            this.lblDestUserID.Text = "User ID:";
            this.lblDestUserID.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblClient
            // 
            this.lblClient.AutoSize = true;
            this.lblClient.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.lblClient.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblClient.Location = new System.Drawing.Point(40, 15);
            this.lblClient.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblClient.Name = "lblClient";
            this.lblClient.Size = new System.Drawing.Size(56, 15);
            this.lblClient.TabIndex = 23;
            this.lblClient.Text = "Client ID:";
            this.lblClient.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // txtPwd
            // 
            this.txtPwd.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtPwd.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.txtPwd.Location = new System.Drawing.Point(103, 93);
            this.txtPwd.Name = "txtPwd";
            this.txtPwd.Size = new System.Drawing.Size(375, 21);
            this.txtPwd.TabIndex = 3;
            this.txtPwd.UseSystemPasswordChar = true;
            // 
            // txtUserID
            // 
            this.txtUserID.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtUserID.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.txtUserID.Location = new System.Drawing.Point(103, 66);
            this.txtUserID.Name = "txtUserID";
            this.txtUserID.Size = new System.Drawing.Size(375, 21);
            this.txtUserID.TabIndex = 2;
            // 
            // txtClientID
            // 
            this.txtClientID.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.txtClientID.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.txtClientID.Location = new System.Drawing.Point(103, 12);
            this.txtClientID.Name = "txtClientID";
            this.txtClientID.Size = new System.Drawing.Size(375, 21);
            this.txtClientID.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Black;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnCancel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnCancel.Location = new System.Drawing.Point(397, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(85, 24);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.Black;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.btnSave.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnSave.Location = new System.Drawing.Point(299, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(92, 24);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // JobSettingsCtrl
            // 
            this.JobSettingsCtrl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.JobSettingsStatus});
            this.JobSettingsCtrl.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.JobSettingsCtrl.Location = new System.Drawing.Point(0, 25);
            this.JobSettingsCtrl.Name = "JobSettingsCtrl";
            this.JobSettingsCtrl.ShowItemToolTips = true;
            this.JobSettingsCtrl.Size = new System.Drawing.Size(494, 5);
            this.JobSettingsCtrl.TabIndex = 2;
            // 
            // JobSettingsStatus
            // 
            this.JobSettingsStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.JobSettingsStatus.Name = "JobSettingsStatus";
            this.JobSettingsStatus.Size = new System.Drawing.Size(0, 0);
            this.JobSettingsStatus.TextChanged += new System.EventHandler(this.JobSettingsStatus_TextChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.JobSettingsCtrl);
            this.splitContainer1.Panel2MinSize = 0;
            this.splitContainer1.Size = new System.Drawing.Size(494, 242);
            this.splitContainer1.SplitterDistance = 211;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 8;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.btnSave);
            this.splitContainer2.Panel2.Controls.Add(this.btnCancel);
            this.splitContainer2.Size = new System.Drawing.Size(494, 211);
            this.splitContainer2.SplitterDistance = 178;
            this.splitContainer2.SplitterWidth = 1;
            this.splitContainer2.TabIndex = 2;
            // 
            // FormJobSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(494, 242);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormJobSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Job Settings";
            this.tabControl1.ResumeLayout(false);
            this.tbJobDetails.ResumeLayout(false);
            this.tbJobDetails.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtInterval)).EndInit();
            this.tbSource.ResumeLayout(false);
            this.tbSource.PerformLayout();
            this.tbDatasetDetails.ResumeLayout(false);
            this.tbDatasetDetails.PerformLayout();
            this.tbDestination.ResumeLayout(false);
            this.tbDestination.PerformLayout();
            this.JobSettingsCtrl.ResumeLayout(false);
            this.JobSettingsCtrl.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.Label lblUserID;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tbJobDetails;
        private System.Windows.Forms.TabPage tbSource;
        private System.Windows.Forms.TabPage tbDatasetDetails;
        private System.Windows.Forms.TabPage tbDestination;
        private System.Windows.Forms.TextBox txtJobName;
        private System.Windows.Forms.Label lblJob;
        private System.Windows.Forms.NumericUpDown txtInterval;
        private System.Windows.Forms.Label lblSeconds;
        private System.Windows.Forms.Label LblInterval;
        private System.Windows.Forms.TextBox txtClientID;
        private System.Windows.Forms.TextBox txtUserID;
        private System.Windows.Forms.TextBox txtPwd;
        private System.Windows.Forms.Label lblSQLPwd;
        private System.Windows.Forms.TextBox txtSQLPwd;
        private System.Windows.Forms.TextBox txtSQLUserID;
        private System.Windows.Forms.TextBox txtSeverName;
        private System.Windows.Forms.Label lblDestPwd;
        private System.Windows.Forms.Label lblDestUserID;
        private System.Windows.Forms.Label lblClient;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnValidateSQL;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnValidateBI;
        private System.Windows.Forms.TextBox txtDbName;
        private System.Windows.Forms.Label lblSQLDb;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.Label lblDatasetName;
        private System.Windows.Forms.TextBox txtDatasetName;
        private System.Windows.Forms.CheckBox chkIsFullLoad;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.CheckBox chkSelectAll;
        private System.Windows.Forms.StatusStrip JobSettingsCtrl;
        private System.Windows.Forms.ToolStripStatusLabel JobSettingsStatus;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
    }
}

