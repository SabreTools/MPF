namespace DICUI.Forms.Windows
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.MenuBar = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mainWindowLayout = new System.Windows.Forms.TableLayoutPanel();
            this.settingsGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.parametersLabel = new System.Windows.Forms.Label();
            this.driveSpeedLabel = new System.Windows.Forms.Label();
            this.driveLetterLabel = new System.Windows.Forms.Label();
            this.outputDirectoryLabel = new System.Windows.Forms.Label();
            this.outputFilenameLabel = new System.Windows.Forms.Label();
            this.systemMediaTypeLabel = new System.Windows.Forms.Label();
            this.OutputFilenameTextBox = new System.Windows.Forms.TextBox();
            this.SystemTypeComboBox = new System.Windows.Forms.ComboBox();
            this.MediaTypeComboBox = new System.Windows.Forms.ComboBox();
            this.OutputDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.OutputDirectoryBrowseButton = new System.Windows.Forms.Button();
            this.DriveLetterComboBox = new System.Windows.Forms.ComboBox();
            this.DriveSpeedComboBox = new System.Windows.Forms.ComboBox();
            this.ParametersTextBox = new System.Windows.Forms.TextBox();
            this.EnableParametersCheckBox = new System.Windows.Forms.CheckBox();
            this.controlsGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.StartStopButton = new System.Windows.Forms.Button();
            this.DiscScanButton = new System.Windows.Forms.Button();
            this.CopyProtectScanButton = new System.Windows.Forms.Button();
            this.EjectWhenDoneCheckBox = new System.Windows.Forms.CheckBox();
            this.statusGroupBox = new System.Windows.Forms.GroupBox();
            this.StatusLabel = new System.Windows.Forms.TextBox();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showLogWindowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuBar.SuspendLayout();
            this.mainWindowLayout.SuspendLayout();
            this.settingsGroupBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.controlsGroupBox.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.statusGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MenuBar
            // 
            this.MenuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.MenuBar.Location = new System.Drawing.Point(0, 0);
            this.MenuBar.Name = "MenuBar";
            this.MenuBar.Size = new System.Drawing.Size(584, 24);
            this.MenuBar.TabIndex = 0;
            this.MenuBar.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F)));
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.T)));
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.H)));
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // mainWindowLayout
            // 
            this.mainWindowLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainWindowLayout.ColumnCount = 1;
            this.mainWindowLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainWindowLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainWindowLayout.Controls.Add(this.settingsGroupBox, 0, 0);
            this.mainWindowLayout.Controls.Add(this.controlsGroupBox, 0, 1);
            this.mainWindowLayout.Controls.Add(this.statusGroupBox, 0, 2);
            this.mainWindowLayout.Location = new System.Drawing.Point(13, 28);
            this.mainWindowLayout.Name = "mainWindowLayout";
            this.mainWindowLayout.RowCount = 3;
            this.mainWindowLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 82.66254F));
            this.mainWindowLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 17.33746F));
            this.mainWindowLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.mainWindowLayout.Size = new System.Drawing.Size(559, 371);
            this.mainWindowLayout.TabIndex = 1;
            // 
            // settingsGroupBox
            // 
            this.settingsGroupBox.Controls.Add(this.tableLayoutPanel2);
            this.settingsGroupBox.Location = new System.Drawing.Point(3, 3);
            this.settingsGroupBox.Name = "settingsGroupBox";
            this.settingsGroupBox.Size = new System.Drawing.Size(553, 259);
            this.settingsGroupBox.TabIndex = 0;
            this.settingsGroupBox.TabStop = false;
            this.settingsGroupBox.Text = "Settings";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 26.32794F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 73.67206F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 145F));
            this.tableLayoutPanel2.Controls.Add(this.parametersLabel, 0, 5);
            this.tableLayoutPanel2.Controls.Add(this.driveSpeedLabel, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.driveLetterLabel, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.outputDirectoryLabel, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.outputFilenameLabel, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.systemMediaTypeLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.OutputFilenameTextBox, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.SystemTypeComboBox, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.MediaTypeComboBox, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.OutputDirectoryTextBox, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.OutputDirectoryBrowseButton, 2, 2);
            this.tableLayoutPanel2.Controls.Add(this.DriveLetterComboBox, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.DriveSpeedComboBox, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.ParametersTextBox, 1, 5);
            this.tableLayoutPanel2.Controls.Add(this.EnableParametersCheckBox, 2, 5);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 19);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.Padding = new System.Windows.Forms.Padding(0, 5, 0, 5);
            this.tableLayoutPanel2.RowCount = 6;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(553, 234);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // parametersLabel
            // 
            this.parametersLabel.AutoSize = true;
            this.parametersLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parametersLabel.Location = new System.Drawing.Point(3, 190);
            this.parametersLabel.Name = "parametersLabel";
            this.parametersLabel.Size = new System.Drawing.Size(101, 39);
            this.parametersLabel.TabIndex = 10;
            this.parametersLabel.Text = "Parameters";
            this.parametersLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // driveSpeedLabel
            // 
            this.driveSpeedLabel.AutoSize = true;
            this.driveSpeedLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.driveSpeedLabel.Location = new System.Drawing.Point(3, 153);
            this.driveSpeedLabel.Name = "driveSpeedLabel";
            this.driveSpeedLabel.Size = new System.Drawing.Size(101, 37);
            this.driveSpeedLabel.TabIndex = 8;
            this.driveSpeedLabel.Text = "Drive Speed";
            this.driveSpeedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // driveLetterLabel
            // 
            this.driveLetterLabel.AutoSize = true;
            this.driveLetterLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.driveLetterLabel.Location = new System.Drawing.Point(3, 116);
            this.driveLetterLabel.Name = "driveLetterLabel";
            this.driveLetterLabel.Size = new System.Drawing.Size(101, 37);
            this.driveLetterLabel.TabIndex = 6;
            this.driveLetterLabel.Text = "Drive Letter";
            this.driveLetterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // outputDirectoryLabel
            // 
            this.outputDirectoryLabel.AutoSize = true;
            this.outputDirectoryLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputDirectoryLabel.Location = new System.Drawing.Point(3, 79);
            this.outputDirectoryLabel.Name = "outputDirectoryLabel";
            this.outputDirectoryLabel.Size = new System.Drawing.Size(101, 37);
            this.outputDirectoryLabel.TabIndex = 4;
            this.outputDirectoryLabel.Text = "Output Directory";
            this.outputDirectoryLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // outputFilenameLabel
            // 
            this.outputFilenameLabel.AutoSize = true;
            this.outputFilenameLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outputFilenameLabel.Location = new System.Drawing.Point(3, 42);
            this.outputFilenameLabel.Name = "outputFilenameLabel";
            this.outputFilenameLabel.Size = new System.Drawing.Size(101, 37);
            this.outputFilenameLabel.TabIndex = 2;
            this.outputFilenameLabel.Text = "Output Filename";
            this.outputFilenameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // systemMediaTypeLabel
            // 
            this.systemMediaTypeLabel.AutoSize = true;
            this.systemMediaTypeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.systemMediaTypeLabel.Location = new System.Drawing.Point(3, 5);
            this.systemMediaTypeLabel.Name = "systemMediaTypeLabel";
            this.systemMediaTypeLabel.Size = new System.Drawing.Size(101, 37);
            this.systemMediaTypeLabel.TabIndex = 0;
            this.systemMediaTypeLabel.Text = "System/Media Type";
            this.systemMediaTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // OutputFilenameTextBox
            // 
            this.OutputFilenameTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tableLayoutPanel2.SetColumnSpan(this.OutputFilenameTextBox, 2);
            this.OutputFilenameTextBox.Location = new System.Drawing.Point(111, 50);
            this.OutputFilenameTextBox.Name = "OutputFilenameTextBox";
            this.OutputFilenameTextBox.Size = new System.Drawing.Size(438, 20);
            this.OutputFilenameTextBox.TabIndex = 11;
            this.OutputFilenameTextBox.TextChanged += new System.EventHandler(this.OutputFilenameTextBoxTextChanged);
            // 
            // SystemTypeComboBox
            // 
            this.SystemTypeComboBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.SystemTypeComboBox.DisplayMember = "Name";
            this.SystemTypeComboBox.DropDownWidth = 250;
            this.SystemTypeComboBox.FormattingEnabled = true;
            this.SystemTypeComboBox.Location = new System.Drawing.Point(110, 13);
            this.SystemTypeComboBox.Name = "SystemTypeComboBox";
            this.SystemTypeComboBox.Size = new System.Drawing.Size(294, 21);
            this.SystemTypeComboBox.TabIndex = 13;
            this.SystemTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.SystemTypeComboBoxSelectionChanged);
            // 
            // MediaTypeComboBox
            // 
            this.MediaTypeComboBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.MediaTypeComboBox.DisplayMember = "Name";
            this.MediaTypeComboBox.FormattingEnabled = true;
            this.MediaTypeComboBox.Location = new System.Drawing.Point(414, 13);
            this.MediaTypeComboBox.Name = "MediaTypeComboBox";
            this.MediaTypeComboBox.Size = new System.Drawing.Size(131, 21);
            this.MediaTypeComboBox.TabIndex = 14;
            this.MediaTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.MediaTypeComboBoxSelectionChanged);
            // 
            // OutputDirectoryTextBox
            // 
            this.OutputDirectoryTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.OutputDirectoryTextBox.Location = new System.Drawing.Point(110, 87);
            this.OutputDirectoryTextBox.Name = "OutputDirectoryTextBox";
            this.OutputDirectoryTextBox.Size = new System.Drawing.Size(294, 20);
            this.OutputDirectoryTextBox.TabIndex = 15;
            this.OutputDirectoryTextBox.TextChanged += new System.EventHandler(this.OutputDirectoryTextBoxTextChanged);
            // 
            // OutputDirectoryBrowseButton
            // 
            this.OutputDirectoryBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.OutputDirectoryBrowseButton.Location = new System.Drawing.Point(410, 84);
            this.OutputDirectoryBrowseButton.Name = "OutputDirectoryBrowseButton";
            this.OutputDirectoryBrowseButton.Size = new System.Drawing.Size(140, 26);
            this.OutputDirectoryBrowseButton.TabIndex = 16;
            this.OutputDirectoryBrowseButton.Text = "Browse";
            this.OutputDirectoryBrowseButton.UseVisualStyleBackColor = true;
            this.OutputDirectoryBrowseButton.Click += new System.EventHandler(this.OutputDirectoryBrowseButtonClick);
            // 
            // DriveLetterComboBox
            // 
            this.DriveLetterComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.DriveLetterComboBox.DisplayMember = "Letter";
            this.DriveLetterComboBox.FormattingEnabled = true;
            this.DriveLetterComboBox.Location = new System.Drawing.Point(110, 124);
            this.DriveLetterComboBox.Name = "DriveLetterComboBox";
            this.DriveLetterComboBox.Size = new System.Drawing.Size(121, 21);
            this.DriveLetterComboBox.TabIndex = 17;
            this.DriveLetterComboBox.SelectedIndexChanged += new System.EventHandler(this.DriveLetterComboBoxSelectionChanged);
            // 
            // DriveSpeedComboBox
            // 
            this.DriveSpeedComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.DriveSpeedComboBox.FormattingEnabled = true;
            this.DriveSpeedComboBox.Location = new System.Drawing.Point(110, 161);
            this.DriveSpeedComboBox.Name = "DriveSpeedComboBox";
            this.DriveSpeedComboBox.Size = new System.Drawing.Size(121, 21);
            this.DriveSpeedComboBox.TabIndex = 18;
            this.DriveSpeedComboBox.SelectedIndexChanged += new System.EventHandler(this.DriveSpeedComboBoxSelectionChanged);
            // 
            // ParametersTextBox
            // 
            this.ParametersTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ParametersTextBox.Enabled = false;
            this.ParametersTextBox.Location = new System.Drawing.Point(110, 199);
            this.ParametersTextBox.Name = "ParametersTextBox";
            this.ParametersTextBox.ReadOnly = true;
            this.ParametersTextBox.Size = new System.Drawing.Size(294, 20);
            this.ParametersTextBox.TabIndex = 19;
            // 
            // EnableParametersCheckBox
            // 
            this.EnableParametersCheckBox.AutoSize = true;
            this.EnableParametersCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EnableParametersCheckBox.Location = new System.Drawing.Point(410, 193);
            this.EnableParametersCheckBox.Name = "EnableParametersCheckBox";
            this.EnableParametersCheckBox.Size = new System.Drawing.Size(140, 33);
            this.EnableParametersCheckBox.TabIndex = 20;
            this.EnableParametersCheckBox.Text = "Enable Editing";
            this.EnableParametersCheckBox.UseVisualStyleBackColor = true;
            this.EnableParametersCheckBox.CheckedChanged += new System.EventHandler(this.EnableParametersCheckBoxClick);
            // 
            // controlsGroupBox
            // 
            this.controlsGroupBox.Controls.Add(this.tableLayoutPanel3);
            this.controlsGroupBox.Location = new System.Drawing.Point(3, 270);
            this.controlsGroupBox.Name = "controlsGroupBox";
            this.controlsGroupBox.Size = new System.Drawing.Size(553, 50);
            this.controlsGroupBox.TabIndex = 1;
            this.controlsGroupBox.TabStop = false;
            this.controlsGroupBox.Text = "Controls";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 4;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel3.Controls.Add(this.StartStopButton, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.DiscScanButton, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.CopyProtectScanButton, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.EjectWhenDoneCheckBox, 3, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 20);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(553, 32);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // StartStopButton
            // 
            this.StartStopButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StartStopButton.Enabled = false;
            this.StartStopButton.Location = new System.Drawing.Point(3, 3);
            this.StartStopButton.Name = "StartStopButton";
            this.StartStopButton.Size = new System.Drawing.Size(132, 26);
            this.StartStopButton.TabIndex = 0;
            this.StartStopButton.Text = "Start Dumping";
            this.StartStopButton.UseVisualStyleBackColor = true;
            this.StartStopButton.Click += new System.EventHandler(this.StartStopButtonClick);
            // 
            // DiscScanButton
            // 
            this.DiscScanButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DiscScanButton.Location = new System.Drawing.Point(141, 3);
            this.DiscScanButton.Name = "DiscScanButton";
            this.DiscScanButton.Size = new System.Drawing.Size(132, 26);
            this.DiscScanButton.TabIndex = 1;
            this.DiscScanButton.Text = "Scan For Discs";
            this.DiscScanButton.UseVisualStyleBackColor = true;
            this.DiscScanButton.Click += new System.EventHandler(this.DiscScanButtonClick);
            // 
            // CopyProtectScanButton
            // 
            this.CopyProtectScanButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CopyProtectScanButton.Location = new System.Drawing.Point(279, 3);
            this.CopyProtectScanButton.Name = "CopyProtectScanButton";
            this.CopyProtectScanButton.Size = new System.Drawing.Size(132, 26);
            this.CopyProtectScanButton.TabIndex = 2;
            this.CopyProtectScanButton.Text = "Scan For Protection";
            this.CopyProtectScanButton.UseVisualStyleBackColor = true;
            this.CopyProtectScanButton.Click += new System.EventHandler(this.CopyProtectScanButtonClick);
            // 
            // EjectWhenDoneCheckBox
            // 
            this.EjectWhenDoneCheckBox.AutoSize = true;
            this.EjectWhenDoneCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EjectWhenDoneCheckBox.Location = new System.Drawing.Point(417, 3);
            this.EjectWhenDoneCheckBox.Name = "EjectWhenDoneCheckBox";
            this.EjectWhenDoneCheckBox.Size = new System.Drawing.Size(133, 26);
            this.EjectWhenDoneCheckBox.TabIndex = 3;
            this.EjectWhenDoneCheckBox.Text = "Eject When Done";
            this.EjectWhenDoneCheckBox.UseVisualStyleBackColor = true;
            // 
            // statusGroupBox
            // 
            this.statusGroupBox.Controls.Add(this.StatusLabel);
            this.statusGroupBox.Location = new System.Drawing.Point(3, 326);
            this.statusGroupBox.Name = "statusGroupBox";
            this.statusGroupBox.Size = new System.Drawing.Size(553, 42);
            this.statusGroupBox.TabIndex = 2;
            this.statusGroupBox.TabStop = false;
            this.statusGroupBox.Text = "Status";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.StatusLabel.Enabled = false;
            this.StatusLabel.Location = new System.Drawing.Point(3, 16);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.ReadOnly = true;
            this.StatusLabel.Size = new System.Drawing.Size(547, 20);
            this.StatusLabel.TabIndex = 0;
            this.StatusLabel.Text = "Waiting for media...";
            this.StatusLabel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.X)));
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.AppExitClick);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.O)));
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.optionsToolStripMenuItem.Text = "Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.OptionsClick);
            // 
            // showLogWindowToolStripMenuItem
            // 
            this.showLogWindowToolStripMenuItem.Checked = true;
            this.showLogWindowToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.showLogWindowToolStripMenuItem.Name = "showLogWindowToolStripMenuItem";
            this.showLogWindowToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.L)));
            this.showLogWindowToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.showLogWindowToolStripMenuItem.Text = "Show Log Window";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.A)));
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.AboutClick);
            // 
            // MainWindow
            // 
            this.AcceptButton = this.StartStopButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 411);
            this.Controls.Add(this.mainWindowLayout);
            this.Controls.Add(this.MenuBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.MenuBar;
            this.Name = "MainWindow";
            this.Text = "DiscImageCreator GUI";
            this.Activated += new System.EventHandler(this.MainWindowActivated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindowClosing);
            this.Load += new System.EventHandler(this.OnContentRendered);
            this.LocationChanged += new System.EventHandler(this.MainWindowLocationChanged);
            this.MenuBar.ResumeLayout(false);
            this.MenuBar.PerformLayout();
            this.mainWindowLayout.ResumeLayout(false);
            this.settingsGroupBox.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.controlsGroupBox.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.statusGroupBox.ResumeLayout(false);
            this.statusGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip MenuBar;
        private System.Windows.Forms.TableLayoutPanel mainWindowLayout;
        private System.Windows.Forms.GroupBox settingsGroupBox;
        private System.Windows.Forms.GroupBox controlsGroupBox;
        private System.Windows.Forms.GroupBox statusGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label systemMediaTypeLabel;
        private System.Windows.Forms.Label parametersLabel;
        private System.Windows.Forms.Label driveSpeedLabel;
        private System.Windows.Forms.Label driveLetterLabel;
        private System.Windows.Forms.Label outputDirectoryLabel;
        private System.Windows.Forms.Label outputFilenameLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Button StartStopButton;
        private System.Windows.Forms.Button DiscScanButton;
        private System.Windows.Forms.Button CopyProtectScanButton;
        private System.Windows.Forms.CheckBox EjectWhenDoneCheckBox;
        private System.Windows.Forms.TextBox OutputFilenameTextBox;
        private System.Windows.Forms.ComboBox SystemTypeComboBox;
        private System.Windows.Forms.ComboBox MediaTypeComboBox;
        private System.Windows.Forms.TextBox OutputDirectoryTextBox;
        private System.Windows.Forms.Button OutputDirectoryBrowseButton;
        private System.Windows.Forms.ComboBox DriveLetterComboBox;
        private System.Windows.Forms.ComboBox DriveSpeedComboBox;
        private System.Windows.Forms.TextBox ParametersTextBox;
        private System.Windows.Forms.CheckBox EnableParametersCheckBox;
        private System.Windows.Forms.TextBox StatusLabel;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        internal System.Windows.Forms.ToolStripMenuItem showLogWindowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
    }
}

