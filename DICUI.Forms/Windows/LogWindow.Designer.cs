namespace DICUI.Forms.Windows
{
    partial class LogWindow
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
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.output = new System.Windows.Forms.RichTextBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.Verbose = new System.Windows.Forms.CheckBox();
            this.OpenAtStartupCheckBox = new System.Windows.Forms.CheckBox();
            this.ClearButton = new System.Windows.Forms.Button();
            this.HideButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.progressBar, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.output, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(560, 287);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progressBar.Location = new System.Drawing.Point(3, 3);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(554, 34);
            this.progressBar.TabIndex = 0;
            // 
            // output
            // 
            this.output.BackColor = System.Drawing.Color.DimGray;
            this.output.Dock = System.Windows.Forms.DockStyle.Fill;
            this.output.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.output.Location = new System.Drawing.Point(3, 43);
            this.output.Name = "output";
            this.output.ReadOnly = true;
            this.output.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.output.Size = new System.Drawing.Size(554, 201);
            this.output.TabIndex = 1;
            this.output.Text = "";
            this.output.TextChanged += new System.EventHandler(this.OnTextChanged);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel2.Controls.Add(this.Verbose, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.OpenAtStartupCheckBox, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.ClearButton, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.HideButton, 4, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 250);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(554, 34);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // Verbose
            // 
            this.Verbose.AutoSize = true;
            this.Verbose.Dock = System.Windows.Forms.DockStyle.Left;
            this.Verbose.Location = new System.Drawing.Point(3, 3);
            this.Verbose.Name = "Verbose";
            this.Verbose.Size = new System.Drawing.Size(65, 28);
            this.Verbose.TabIndex = 1;
            this.Verbose.Text = "Verbose";
            this.Verbose.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Verbose.UseVisualStyleBackColor = true;
            // 
            // OpenAtStartupCheckBox
            // 
            this.OpenAtStartupCheckBox.AutoSize = true;
            this.OpenAtStartupCheckBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OpenAtStartupCheckBox.Location = new System.Drawing.Point(86, 3);
            this.OpenAtStartupCheckBox.Name = "OpenAtStartupCheckBox";
            this.OpenAtStartupCheckBox.Size = new System.Drawing.Size(104, 28);
            this.OpenAtStartupCheckBox.TabIndex = 2;
            this.OpenAtStartupCheckBox.Text = "Open at startup";
            this.OpenAtStartupCheckBox.UseVisualStyleBackColor = true;
            // 
            // ClearButton
            // 
            this.ClearButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ClearButton.Location = new System.Drawing.Point(389, 3);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(77, 28);
            this.ClearButton.TabIndex = 3;
            this.ClearButton.Text = "Clear";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.OnClearButton);
            // 
            // HideButton
            // 
            this.HideButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.HideButton.Location = new System.Drawing.Point(472, 3);
            this.HideButton.Name = "HideButton";
            this.HideButton.Size = new System.Drawing.Size(79, 28);
            this.HideButton.TabIndex = 4;
            this.HideButton.Text = "Hide";
            this.HideButton.UseVisualStyleBackColor = true;
            this.HideButton.SizeChanged += new System.EventHandler(this.ScrollViewer_SizeChanged);
            this.HideButton.Click += new System.EventHandler(this.OnHideButton);
            // 
            // LogWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 311);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "LogWindow";
            this.ShowIcon = false;
            this.Text = "Log Window";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnWindowClosed);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.RichTextBox output;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.CheckBox Verbose;
        private System.Windows.Forms.CheckBox OpenAtStartupCheckBox;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.Button HideButton;
    }
}