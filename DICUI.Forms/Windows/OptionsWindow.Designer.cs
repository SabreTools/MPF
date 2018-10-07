namespace DICUI.Forms.Windows
{
    partial class OptionsWindow
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pathsGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.DefaultOutputPathButton = new System.Windows.Forms.Button();
            this.SubDumpPathButton = new System.Windows.Forms.Button();
            this.discImageCreatorPathLabel = new System.Windows.Forms.Label();
            this.subdumpPathLabel = new System.Windows.Forms.Label();
            this.defaultOutputPathLabel = new System.Windows.Forms.Label();
            this.DICPathTextBox = new System.Windows.Forms.TextBox();
            this.SubDumpPathTextBox = new System.Windows.Forms.TextBox();
            this.DefaultOutputPathTextBox = new System.Windows.Forms.TextBox();
            this.DICPathButton = new System.Windows.Forms.Button();
            this.preferredSpeedGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.DumpSpeedCDLabel = new System.Windows.Forms.Label();
            this.DumpSpeedDVDLabel = new System.Windows.Forms.Label();
            this.DumpSpeedBDLabel = new System.Windows.Forms.Label();
            this.DumpSpeedCDSlider = new System.Windows.Forms.TrackBar();
            this.DumpSpeedDVDSlider = new System.Windows.Forms.TrackBar();
            this.DumpSpeedBDSlider = new System.Windows.Forms.TrackBar();
            this.DumpSpeedCDTextBox = new System.Windows.Forms.TextBox();
            this.DumpSpeedDVDTextBox = new System.Windows.Forms.TextBox();
            this.DumpSpeedBDTextBox = new System.Windows.Forms.TextBox();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.QuietModeCheckBox = new System.Windows.Forms.CheckBox();
            this.AutoScanCheckBox = new System.Windows.Forms.CheckBox();
            this.ParanoidModeCheckBox = new System.Windows.Forms.CheckBox();
            this.SkipDetectionCheckBox = new System.Windows.Forms.CheckBox();
            this.C2RereadTimesTextBox = new System.Windows.Forms.TextBox();
            this.C2RereadLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.AcceptButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.pathsGroupBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.preferredSpeedGroupBox.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DumpSpeedCDSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DumpSpeedDVDSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DumpSpeedBDSlider)).BeginInit();
            this.optionsGroupBox.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.pathsGroupBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.preferredSpeedGroupBox, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.optionsGroupBox, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel5, 0, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(475, 417);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.UseWaitCursor = true;
            // 
            // pathsGroupBox
            // 
            this.pathsGroupBox.Controls.Add(this.tableLayoutPanel2);
            this.pathsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pathsGroupBox.Location = new System.Drawing.Point(3, 3);
            this.pathsGroupBox.Name = "pathsGroupBox";
            this.pathsGroupBox.Size = new System.Drawing.Size(469, 121);
            this.pathsGroupBox.TabIndex = 0;
            this.pathsGroupBox.TabStop = false;
            this.pathsGroupBox.Text = "Paths";
            this.pathsGroupBox.UseWaitCursor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29.41176F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 58.82353F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 11.76471F));
            this.tableLayoutPanel2.Controls.Add(this.DefaultOutputPathButton, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this.SubDumpPathButton, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.discImageCreatorPathLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.subdumpPathLabel, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.defaultOutputPathLabel, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.DICPathTextBox, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.SubDumpPathTextBox, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.DefaultOutputPathTextBox, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.DICPathButton, 2, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 15);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(457, 100);
            this.tableLayoutPanel2.TabIndex = 0;
            this.tableLayoutPanel2.UseWaitCursor = true;
            // 
            // DefaultOutputPathButton
            // 
            this.DefaultOutputPathButton.Location = new System.Drawing.Point(405, 69);
            this.DefaultOutputPathButton.Name = "DefaultOutputPathButton";
            this.DefaultOutputPathButton.Size = new System.Drawing.Size(49, 23);
            this.DefaultOutputPathButton.TabIndex = 8;
            this.DefaultOutputPathButton.Text = "...";
            this.DefaultOutputPathButton.UseVisualStyleBackColor = true;
            this.DefaultOutputPathButton.UseWaitCursor = true;
            this.DefaultOutputPathButton.Click += new System.EventHandler(this.BrowseForPathClick);
            // 
            // SubDumpPathButton
            // 
            this.SubDumpPathButton.Location = new System.Drawing.Point(405, 36);
            this.SubDumpPathButton.Name = "SubDumpPathButton";
            this.SubDumpPathButton.Size = new System.Drawing.Size(49, 23);
            this.SubDumpPathButton.TabIndex = 7;
            this.SubDumpPathButton.Text = "...";
            this.SubDumpPathButton.UseVisualStyleBackColor = true;
            this.SubDumpPathButton.UseWaitCursor = true;
            this.SubDumpPathButton.Click += new System.EventHandler(this.BrowseForPathClick);
            // 
            // discImageCreatorPathLabel
            // 
            this.discImageCreatorPathLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.discImageCreatorPathLabel.AutoSize = true;
            this.discImageCreatorPathLabel.Cursor = System.Windows.Forms.Cursors.WaitCursor;
            this.discImageCreatorPathLabel.Location = new System.Drawing.Point(15, 10);
            this.discImageCreatorPathLabel.Name = "discImageCreatorPathLabel";
            this.discImageCreatorPathLabel.Size = new System.Drawing.Size(116, 13);
            this.discImageCreatorPathLabel.TabIndex = 0;
            this.discImageCreatorPathLabel.Text = "DiscImageCreator Path";
            // 
            // subdumpPathLabel
            // 
            this.subdumpPathLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.subdumpPathLabel.AutoSize = true;
            this.subdumpPathLabel.Location = new System.Drawing.Point(56, 43);
            this.subdumpPathLabel.Name = "subdumpPathLabel";
            this.subdumpPathLabel.Size = new System.Drawing.Size(75, 13);
            this.subdumpPathLabel.TabIndex = 1;
            this.subdumpPathLabel.Text = "subdump Path";
            this.subdumpPathLabel.UseWaitCursor = true;
            // 
            // defaultOutputPathLabel
            // 
            this.defaultOutputPathLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.defaultOutputPathLabel.AutoSize = true;
            this.defaultOutputPathLabel.Location = new System.Drawing.Point(30, 76);
            this.defaultOutputPathLabel.Name = "defaultOutputPathLabel";
            this.defaultOutputPathLabel.Size = new System.Drawing.Size(101, 13);
            this.defaultOutputPathLabel.TabIndex = 2;
            this.defaultOutputPathLabel.Text = "Default Output Path";
            this.defaultOutputPathLabel.UseWaitCursor = true;
            // 
            // DICPathTextBox
            // 
            this.DICPathTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.DICPathTextBox.Location = new System.Drawing.Point(138, 6);
            this.DICPathTextBox.Name = "DICPathTextBox";
            this.DICPathTextBox.Size = new System.Drawing.Size(259, 20);
            this.DICPathTextBox.TabIndex = 3;
            this.DICPathTextBox.UseWaitCursor = true;
            // 
            // SubDumpPathTextBox
            // 
            this.SubDumpPathTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.SubDumpPathTextBox.Location = new System.Drawing.Point(138, 39);
            this.SubDumpPathTextBox.Name = "SubDumpPathTextBox";
            this.SubDumpPathTextBox.Size = new System.Drawing.Size(259, 20);
            this.SubDumpPathTextBox.TabIndex = 4;
            this.SubDumpPathTextBox.UseWaitCursor = true;
            // 
            // DefaultOutputPathTextBox
            // 
            this.DefaultOutputPathTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.DefaultOutputPathTextBox.Location = new System.Drawing.Point(138, 73);
            this.DefaultOutputPathTextBox.Name = "DefaultOutputPathTextBox";
            this.DefaultOutputPathTextBox.Size = new System.Drawing.Size(259, 20);
            this.DefaultOutputPathTextBox.TabIndex = 5;
            this.DefaultOutputPathTextBox.UseWaitCursor = true;
            // 
            // DICPathButton
            // 
            this.DICPathButton.Location = new System.Drawing.Point(405, 3);
            this.DICPathButton.Name = "DICPathButton";
            this.DICPathButton.Size = new System.Drawing.Size(49, 23);
            this.DICPathButton.TabIndex = 6;
            this.DICPathButton.Text = "...";
            this.DICPathButton.UseVisualStyleBackColor = true;
            this.DICPathButton.UseWaitCursor = true;
            this.DICPathButton.Click += new System.EventHandler(this.BrowseForPathClick);
            // 
            // preferredSpeedGroupBox
            // 
            this.preferredSpeedGroupBox.Controls.Add(this.tableLayoutPanel3);
            this.preferredSpeedGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.preferredSpeedGroupBox.Location = new System.Drawing.Point(3, 130);
            this.preferredSpeedGroupBox.Name = "preferredSpeedGroupBox";
            this.preferredSpeedGroupBox.Size = new System.Drawing.Size(469, 144);
            this.preferredSpeedGroupBox.TabIndex = 1;
            this.preferredSpeedGroupBox.TabStop = false;
            this.preferredSpeedGroupBox.Text = "Preferred Dump Speed";
            this.preferredSpeedGroupBox.UseWaitCursor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel3.Controls.Add(this.DumpSpeedCDLabel, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.DumpSpeedDVDLabel, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.DumpSpeedBDLabel, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.DumpSpeedCDSlider, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.DumpSpeedDVDSlider, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.DumpSpeedBDSlider, 1, 2);
            this.tableLayoutPanel3.Controls.Add(this.DumpSpeedCDTextBox, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.DumpSpeedDVDTextBox, 2, 1);
            this.tableLayoutPanel3.Controls.Add(this.DumpSpeedBDTextBox, 2, 2);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(457, 119);
            this.tableLayoutPanel3.TabIndex = 0;
            this.tableLayoutPanel3.UseWaitCursor = true;
            // 
            // DumpSpeedCDLabel
            // 
            this.DumpSpeedCDLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.DumpSpeedCDLabel.AutoSize = true;
            this.DumpSpeedCDLabel.Location = new System.Drawing.Point(3, 13);
            this.DumpSpeedCDLabel.Name = "DumpSpeedCDLabel";
            this.DumpSpeedCDLabel.Size = new System.Drawing.Size(50, 13);
            this.DumpSpeedCDLabel.TabIndex = 0;
            this.DumpSpeedCDLabel.Text = "CD-ROM";
            this.DumpSpeedCDLabel.UseWaitCursor = true;
            // 
            // DumpSpeedDVDLabel
            // 
            this.DumpSpeedDVDLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.DumpSpeedDVDLabel.AutoSize = true;
            this.DumpSpeedDVDLabel.Location = new System.Drawing.Point(3, 52);
            this.DumpSpeedDVDLabel.Name = "DumpSpeedDVDLabel";
            this.DumpSpeedDVDLabel.Size = new System.Drawing.Size(58, 13);
            this.DumpSpeedDVDLabel.TabIndex = 1;
            this.DumpSpeedDVDLabel.Text = "DVD-ROM";
            this.DumpSpeedDVDLabel.UseWaitCursor = true;
            // 
            // DumpSpeedBDLabel
            // 
            this.DumpSpeedBDLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.DumpSpeedBDLabel.AutoSize = true;
            this.DumpSpeedBDLabel.Location = new System.Drawing.Point(3, 92);
            this.DumpSpeedBDLabel.Name = "DumpSpeedBDLabel";
            this.DumpSpeedBDLabel.Size = new System.Drawing.Size(50, 13);
            this.DumpSpeedBDLabel.TabIndex = 2;
            this.DumpSpeedBDLabel.Text = "BD-ROM";
            this.DumpSpeedBDLabel.UseWaitCursor = true;
            // 
            // DumpSpeedCDSlider
            // 
            this.DumpSpeedCDSlider.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DumpSpeedCDSlider.Location = new System.Drawing.Point(83, 3);
            this.DumpSpeedCDSlider.Name = "DumpSpeedCDSlider";
            this.DumpSpeedCDSlider.Size = new System.Drawing.Size(331, 33);
            this.DumpSpeedCDSlider.TabIndex = 3;
            this.DumpSpeedCDSlider.UseWaitCursor = true;
            this.DumpSpeedCDSlider.Value = 1;
            this.DumpSpeedCDSlider.Scroll += new System.EventHandler(this.SliderChanged);
            // 
            // DumpSpeedDVDSlider
            // 
            this.DumpSpeedDVDSlider.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DumpSpeedDVDSlider.Location = new System.Drawing.Point(83, 42);
            this.DumpSpeedDVDSlider.Name = "DumpSpeedDVDSlider";
            this.DumpSpeedDVDSlider.Size = new System.Drawing.Size(331, 33);
            this.DumpSpeedDVDSlider.TabIndex = 4;
            this.DumpSpeedDVDSlider.UseWaitCursor = true;
            this.DumpSpeedDVDSlider.Value = 1;
            this.DumpSpeedDVDSlider.Scroll += new System.EventHandler(this.SliderChanged);
            // 
            // DumpSpeedBDSlider
            // 
            this.DumpSpeedBDSlider.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DumpSpeedBDSlider.Location = new System.Drawing.Point(83, 81);
            this.DumpSpeedBDSlider.Name = "DumpSpeedBDSlider";
            this.DumpSpeedBDSlider.Size = new System.Drawing.Size(331, 35);
            this.DumpSpeedBDSlider.TabIndex = 5;
            this.DumpSpeedBDSlider.UseWaitCursor = true;
            this.DumpSpeedBDSlider.Value = 1;
            this.DumpSpeedBDSlider.Scroll += new System.EventHandler(this.SliderChanged);
            // 
            // DumpSpeedCDTextBox
            // 
            this.DumpSpeedCDTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.DumpSpeedCDTextBox.Enabled = false;
            this.DumpSpeedCDTextBox.Location = new System.Drawing.Point(420, 9);
            this.DumpSpeedCDTextBox.Name = "DumpSpeedCDTextBox";
            this.DumpSpeedCDTextBox.ReadOnly = true;
            this.DumpSpeedCDTextBox.Size = new System.Drawing.Size(34, 20);
            this.DumpSpeedCDTextBox.TabIndex = 6;
            this.DumpSpeedCDTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.DumpSpeedCDTextBox.UseWaitCursor = true;
            // 
            // DumpSpeedDVDTextBox
            // 
            this.DumpSpeedDVDTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.DumpSpeedDVDTextBox.Enabled = false;
            this.DumpSpeedDVDTextBox.Location = new System.Drawing.Point(420, 48);
            this.DumpSpeedDVDTextBox.Name = "DumpSpeedDVDTextBox";
            this.DumpSpeedDVDTextBox.ReadOnly = true;
            this.DumpSpeedDVDTextBox.Size = new System.Drawing.Size(34, 20);
            this.DumpSpeedDVDTextBox.TabIndex = 7;
            this.DumpSpeedDVDTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.DumpSpeedDVDTextBox.UseWaitCursor = true;
            // 
            // DumpSpeedBDTextBox
            // 
            this.DumpSpeedBDTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.DumpSpeedBDTextBox.Enabled = false;
            this.DumpSpeedBDTextBox.Location = new System.Drawing.Point(420, 88);
            this.DumpSpeedBDTextBox.Name = "DumpSpeedBDTextBox";
            this.DumpSpeedBDTextBox.ReadOnly = true;
            this.DumpSpeedBDTextBox.Size = new System.Drawing.Size(34, 20);
            this.DumpSpeedBDTextBox.TabIndex = 8;
            this.DumpSpeedBDTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.DumpSpeedBDTextBox.UseWaitCursor = true;
            // 
            // optionsGroupBox
            // 
            this.optionsGroupBox.Controls.Add(this.tableLayoutPanel4);
            this.optionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optionsGroupBox.Location = new System.Drawing.Point(3, 280);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(469, 94);
            this.optionsGroupBox.TabIndex = 2;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Options";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 4;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel4.Controls.Add(this.QuietModeCheckBox, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.AutoScanCheckBox, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.ParanoidModeCheckBox, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.SkipDetectionCheckBox, 2, 1);
            this.tableLayoutPanel4.Controls.Add(this.C2RereadTimesTextBox, 3, 0);
            this.tableLayoutPanel4.Controls.Add(this.C2RereadLabel, 2, 0);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(457, 69);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // QuietModeCheckBox
            // 
            this.QuietModeCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.QuietModeCheckBox.AutoSize = true;
            this.QuietModeCheckBox.Location = new System.Drawing.Point(3, 8);
            this.QuietModeCheckBox.Name = "QuietModeCheckBox";
            this.QuietModeCheckBox.Size = new System.Drawing.Size(81, 17);
            this.QuietModeCheckBox.TabIndex = 0;
            this.QuietModeCheckBox.Text = "Quiet Mode";
            this.QuietModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // AutoScanCheckBox
            // 
            this.AutoScanCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.AutoScanCheckBox.AutoSize = true;
            this.tableLayoutPanel4.SetColumnSpan(this.AutoScanCheckBox, 2);
            this.AutoScanCheckBox.Location = new System.Drawing.Point(3, 43);
            this.AutoScanCheckBox.Name = "AutoScanCheckBox";
            this.AutoScanCheckBox.Size = new System.Drawing.Size(182, 17);
            this.AutoScanCheckBox.TabIndex = 1;
            this.AutoScanCheckBox.Text = "Automatically Scan for Protection";
            this.AutoScanCheckBox.UseVisualStyleBackColor = true;
            // 
            // ParanoidModeCheckBox
            // 
            this.ParanoidModeCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.ParanoidModeCheckBox.AutoSize = true;
            this.ParanoidModeCheckBox.Location = new System.Drawing.Point(117, 8);
            this.ParanoidModeCheckBox.Name = "ParanoidModeCheckBox";
            this.ParanoidModeCheckBox.Size = new System.Drawing.Size(98, 17);
            this.ParanoidModeCheckBox.TabIndex = 2;
            this.ParanoidModeCheckBox.Text = "Paranoid Mode";
            this.ParanoidModeCheckBox.UseVisualStyleBackColor = true;
            // 
            // SkipDetectionCheckBox
            // 
            this.SkipDetectionCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.SkipDetectionCheckBox.AutoSize = true;
            this.tableLayoutPanel4.SetColumnSpan(this.SkipDetectionCheckBox, 2);
            this.SkipDetectionCheckBox.Location = new System.Drawing.Point(231, 43);
            this.SkipDetectionCheckBox.Name = "SkipDetectionCheckBox";
            this.SkipDetectionCheckBox.Size = new System.Drawing.Size(155, 17);
            this.SkipDetectionCheckBox.TabIndex = 3;
            this.SkipDetectionCheckBox.Text = "Skip Media Type Detection";
            this.SkipDetectionCheckBox.UseVisualStyleBackColor = true;
            // 
            // C2RereadTimesTextBox
            // 
            this.C2RereadTimesTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.C2RereadTimesTextBox.Location = new System.Drawing.Point(349, 7);
            this.C2RereadTimesTextBox.Name = "C2RereadTimesTextBox";
            this.C2RereadTimesTextBox.Size = new System.Drawing.Size(100, 20);
            this.C2RereadTimesTextBox.TabIndex = 4;
            // 
            // C2RereadLabel
            // 
            this.C2RereadLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.C2RereadLabel.AutoSize = true;
            this.C2RereadLabel.Location = new System.Drawing.Point(250, 10);
            this.C2RereadLabel.Name = "C2RereadLabel";
            this.C2RereadLabel.Size = new System.Drawing.Size(89, 13);
            this.C2RereadLabel.TabIndex = 5;
            this.C2RereadLabel.Text = "C2 Reread Times";
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.AcceptButton, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.CancelButton, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 380);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(469, 34);
            this.tableLayoutPanel5.TabIndex = 3;
            // 
            // AcceptButton
            // 
            this.AcceptButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.AcceptButton.Location = new System.Drawing.Point(79, 5);
            this.AcceptButton.Name = "AcceptButton";
            this.AcceptButton.Size = new System.Drawing.Size(75, 23);
            this.AcceptButton.TabIndex = 0;
            this.AcceptButton.Text = "Accept";
            this.AcceptButton.UseVisualStyleBackColor = true;
            this.AcceptButton.Click += new System.EventHandler(this.OnAcceptClick);
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.CancelButton.Location = new System.Drawing.Point(314, 5);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 1;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.OnCancelClick);
            // 
            // OptionsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 441);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "OptionsWindow";
            this.ShowIcon = false;
            this.Text = "Options";
            this.UseWaitCursor = true;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pathsGroupBox.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.preferredSpeedGroupBox.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.DumpSpeedCDSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DumpSpeedDVDSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DumpSpeedBDSlider)).EndInit();
            this.optionsGroupBox.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox pathsGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label discImageCreatorPathLabel;
        private System.Windows.Forms.Label subdumpPathLabel;
        private System.Windows.Forms.Label defaultOutputPathLabel;
        private System.Windows.Forms.TextBox DICPathTextBox;
        private System.Windows.Forms.TextBox SubDumpPathTextBox;
        private System.Windows.Forms.TextBox DefaultOutputPathTextBox;
        private System.Windows.Forms.Button DICPathButton;
        private System.Windows.Forms.Button DefaultOutputPathButton;
        private System.Windows.Forms.Button SubDumpPathButton;
        private System.Windows.Forms.GroupBox preferredSpeedGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label DumpSpeedCDLabel;
        private System.Windows.Forms.Label DumpSpeedDVDLabel;
        private System.Windows.Forms.Label DumpSpeedBDLabel;
        private System.Windows.Forms.TrackBar DumpSpeedCDSlider;
        private System.Windows.Forms.TrackBar DumpSpeedDVDSlider;
        private System.Windows.Forms.TrackBar DumpSpeedBDSlider;
        private System.Windows.Forms.TextBox DumpSpeedCDTextBox;
        private System.Windows.Forms.TextBox DumpSpeedDVDTextBox;
        private System.Windows.Forms.TextBox DumpSpeedBDTextBox;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.CheckBox QuietModeCheckBox;
        private System.Windows.Forms.CheckBox AutoScanCheckBox;
        private System.Windows.Forms.CheckBox ParanoidModeCheckBox;
        private System.Windows.Forms.CheckBox SkipDetectionCheckBox;
        private System.Windows.Forms.TextBox C2RereadTimesTextBox;
        private System.Windows.Forms.Label C2RereadLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Button AcceptButton;
        private System.Windows.Forms.Button CancelButton;
    }
}