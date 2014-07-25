namespace GoToWindow
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.goToWindowButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.windowsDataGrid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.windowsDataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // goToWindowButton
            // 
            this.goToWindowButton.Location = new System.Drawing.Point(539, 295);
            this.goToWindowButton.Name = "goToWindowButton";
            this.goToWindowButton.Size = new System.Drawing.Size(75, 23);
            this.goToWindowButton.TabIndex = 3;
            this.goToWindowButton.Text = "Go";
            this.goToWindowButton.UseVisualStyleBackColor = true;
            this.goToWindowButton.Click += new System.EventHandler(this.goToWindowButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(458, 295);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // searchTextBox
            // 
            this.searchTextBox.Location = new System.Drawing.Point(12, 12);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(602, 20);
            this.searchTextBox.TabIndex = 1;
            this.searchTextBox.TextChanged += new System.EventHandler(this.searchTextBox_TextChanged);
            this.searchTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchTextBox_KeyDown);
            // 
            // windowsDataGrid
            // 
            this.windowsDataGrid.AllowUserToAddRows = false;
            this.windowsDataGrid.AllowUserToDeleteRows = false;
            this.windowsDataGrid.AllowUserToResizeColumns = false;
            this.windowsDataGrid.AllowUserToResizeRows = false;
            this.windowsDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.windowsDataGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.windowsDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.windowsDataGrid.ColumnHeadersVisible = false;
            this.windowsDataGrid.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.windowsDataGrid.Location = new System.Drawing.Point(12, 38);
            this.windowsDataGrid.MultiSelect = false;
            this.windowsDataGrid.Name = "windowsDataGrid";
            this.windowsDataGrid.ReadOnly = true;
            this.windowsDataGrid.RowHeadersVisible = false;
            this.windowsDataGrid.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.windowsDataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.windowsDataGrid.ShowEditingIcon = false;
            this.windowsDataGrid.Size = new System.Drawing.Size(602, 251);
            this.windowsDataGrid.TabIndex = 2;
            this.windowsDataGrid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.windowsDataGrid_CellDoubleClick);
            this.windowsDataGrid.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.windowsDataGrid_CellValueNeeded);
            this.windowsDataGrid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.windowsDataGrid_KeyDown);
            // 
            // MainForm
            // 
            this.AcceptButton = this.goToWindowButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(626, 330);
            this.ControlBox = false;
            this.Controls.Add(this.windowsDataGrid);
            this.Controls.Add(this.searchTextBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.goToWindowButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Go To Window";
            this.Deactivate += new System.EventHandler(this.MainForm_Deactivate);
            ((System.ComponentModel.ISupportInitialize)(this.windowsDataGrid)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button goToWindowButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.DataGridView windowsDataGrid;
    }
}

