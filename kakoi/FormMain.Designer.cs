namespace omochat
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle7 = new DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle5 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle6 = new DataGridViewCellStyle();
            buttonStart = new Button();
            buttonStop = new Button();
            buttonSetting = new Button();
            checkBoxPostBar = new CheckBox();
            buttonRelayList = new Button();
            labelRelays = new Label();
            toolTipRelays = new ToolTip(components);
            dataGridViewNotes = new DataGridView();
            notifyIcon = new NotifyIcon(components);
            contextMenuStrip = new ContextMenuStrip(components);
            settingToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            quitToolStripMenuItem = new ToolStripMenuItem();
            time = new DataGridViewTextBoxColumn();
            name = new DataGridViewTextBoxColumn();
            hash = new DataGridViewTextBoxColumn();
            note = new DataGridViewTextBoxColumn();
            geohash = new DataGridViewTextBoxColumn();
            id = new DataGridViewTextBoxColumn();
            pubkey = new DataGridViewTextBoxColumn();
            kind = new DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)dataGridViewNotes).BeginInit();
            contextMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // buttonStart
            // 
            buttonStart.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonStart.Image = Properties.Resources.icons8_start_16;
            buttonStart.Location = new Point(291, 326);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(23, 23);
            buttonStart.TabIndex = 5;
            buttonStart.TabStop = false;
            buttonStart.UseVisualStyleBackColor = true;
            buttonStart.Click += ButtonStart_Click;
            // 
            // buttonStop
            // 
            buttonStop.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonStop.Enabled = false;
            buttonStop.Image = Properties.Resources.icons8_stop_16;
            buttonStop.Location = new Point(320, 326);
            buttonStop.Name = "buttonStop";
            buttonStop.Size = new Size(23, 23);
            buttonStop.TabIndex = 6;
            buttonStop.TabStop = false;
            buttonStop.UseVisualStyleBackColor = true;
            buttonStop.Click += ButtonStop_Click;
            // 
            // buttonSetting
            // 
            buttonSetting.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonSetting.Image = Properties.Resources.icons8_setting_16;
            buttonSetting.Location = new Point(349, 326);
            buttonSetting.Name = "buttonSetting";
            buttonSetting.Size = new Size(23, 23);
            buttonSetting.TabIndex = 7;
            buttonSetting.TabStop = false;
            buttonSetting.UseVisualStyleBackColor = true;
            buttonSetting.Click += ButtonSetting_Click;
            // 
            // checkBoxPostBar
            // 
            checkBoxPostBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            checkBoxPostBar.AutoSize = true;
            checkBoxPostBar.Location = new Point(12, 331);
            checkBoxPostBar.Name = "checkBoxPostBar";
            checkBoxPostBar.Size = new Size(15, 14);
            checkBoxPostBar.TabIndex = 2;
            checkBoxPostBar.TabStop = false;
            toolTipRelays.SetToolTip(checkBoxPostBar, "Toggle post bar");
            checkBoxPostBar.UseVisualStyleBackColor = true;
            checkBoxPostBar.CheckedChanged += CheckBoxPostBar_CheckedChanged;
            // 
            // buttonRelayList
            // 
            buttonRelayList.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonRelayList.Image = Properties.Resources.icons8_list_16;
            buttonRelayList.Location = new Point(262, 326);
            buttonRelayList.Name = "buttonRelayList";
            buttonRelayList.Size = new Size(23, 23);
            buttonRelayList.TabIndex = 4;
            buttonRelayList.TabStop = false;
            buttonRelayList.UseVisualStyleBackColor = true;
            buttonRelayList.Click += ButtonRelayList_Click;
            // 
            // labelRelays
            // 
            labelRelays.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelRelays.AutoEllipsis = true;
            labelRelays.ForeColor = SystemColors.GrayText;
            labelRelays.Location = new Point(87, 331);
            labelRelays.Name = "labelRelays";
            labelRelays.Size = new Size(169, 15);
            labelRelays.TabIndex = 0;
            labelRelays.Text = "Relay info";
            labelRelays.TextAlign = ContentAlignment.TopRight;
            labelRelays.MouseClick += FormMain_MouseClick;
            labelRelays.MouseDoubleClick += FormMain_MouseDoubleClick;
            // 
            // dataGridViewNotes
            // 
            dataGridViewNotes.AllowUserToAddRows = false;
            dataGridViewNotes.AllowUserToDeleteRows = false;
            dataGridViewNotes.AllowUserToOrderColumns = true;
            dataGridViewNotes.AllowUserToResizeRows = false;
            dataGridViewNotes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewNotes.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridViewNotes.BackgroundColor = Color.White;
            dataGridViewNotes.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Yu Gothic UI", 9F);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dataGridViewNotes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dataGridViewNotes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewNotes.ColumnHeadersVisible = false;
            dataGridViewNotes.Columns.AddRange(new DataGridViewColumn[] { time, name, hash, note, geohash, id, pubkey, kind });
            dataGridViewCellStyle7.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle7.BackColor = SystemColors.Window;
            dataGridViewCellStyle7.Font = new Font("Yu Gothic UI", 9F);
            dataGridViewCellStyle7.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle7.SelectionBackColor = Color.DeepPink;
            dataGridViewCellStyle7.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle7.WrapMode = DataGridViewTriState.False;
            dataGridViewNotes.DefaultCellStyle = dataGridViewCellStyle7;
            dataGridViewNotes.GridColor = Color.DeepPink;
            dataGridViewNotes.Location = new Point(12, 12);
            dataGridViewNotes.MultiSelect = false;
            dataGridViewNotes.Name = "dataGridViewNotes";
            dataGridViewNotes.ReadOnly = true;
            dataGridViewNotes.RowHeadersVisible = false;
            dataGridViewNotes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewNotes.Size = new Size(360, 308);
            dataGridViewNotes.StandardTab = true;
            dataGridViewNotes.TabIndex = 1;
            dataGridViewNotes.CellDoubleClick += DataGridViewNotes_CellDoubleClick;
            dataGridViewNotes.CellMouseClick += DataGridViewNotes_CellMouseClick;
            dataGridViewNotes.KeyDown += DataGridViewNotes_KeyDown;
            dataGridViewNotes.MouseEnter += Control_MouseEnter;
            dataGridViewNotes.MouseLeave += Control_MouseLeave;
            // 
            // notifyIcon
            // 
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "omochat";
            notifyIcon.Visible = true;
            notifyIcon.Click += NotifyIcon_Click;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.Items.AddRange(new ToolStripItem[] { settingToolStripMenuItem, toolStripMenuItem1, quitToolStripMenuItem });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new Size(112, 54);
            // 
            // settingToolStripMenuItem
            // 
            settingToolStripMenuItem.Name = "settingToolStripMenuItem";
            settingToolStripMenuItem.Size = new Size(111, 22);
            settingToolStripMenuItem.Text = "Setting";
            settingToolStripMenuItem.Click += SettingToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(108, 6);
            // 
            // quitToolStripMenuItem
            // 
            quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            quitToolStripMenuItem.Size = new Size(111, 22);
            quitToolStripMenuItem.Text = "Quit";
            quitToolStripMenuItem.Click += QuitToolStripMenuItem_Click;
            // 
            // time
            // 
            time.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.TopRight;
            dataGridViewCellStyle2.Font = new Font("メイリオ", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            dataGridViewCellStyle2.Format = "t";
            dataGridViewCellStyle2.NullValue = null;
            time.DefaultCellStyle = dataGridViewCellStyle2;
            time.HeaderText = "time";
            time.Name = "time";
            time.ReadOnly = true;
            time.SortMode = DataGridViewColumnSortMode.NotSortable;
            time.Width = 5;
            // 
            // name
            // 
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle3.Font = new Font("メイリオ", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            name.DefaultCellStyle = dataGridViewCellStyle3;
            name.HeaderText = "name";
            name.Name = "name";
            name.ReadOnly = true;
            name.SortMode = DataGridViewColumnSortMode.NotSortable;
            name.Width = 70;
            // 
            // hash
            // 
            hash.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle4.Font = new Font("メイリオ", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            hash.DefaultCellStyle = dataGridViewCellStyle4;
            hash.HeaderText = "hash";
            hash.Name = "hash";
            hash.ReadOnly = true;
            hash.SortMode = DataGridViewColumnSortMode.NotSortable;
            hash.Width = 5;
            // 
            // note
            // 
            note.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle5.Alignment = DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle5.Font = new Font("メイリオ", 9F, FontStyle.Regular, GraphicsUnit.Point, 128);
            dataGridViewCellStyle5.WrapMode = DataGridViewTriState.True;
            note.DefaultCellStyle = dataGridViewCellStyle5;
            note.HeaderText = "note";
            note.Name = "note";
            note.ReadOnly = true;
            note.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // geohash
            // 
            dataGridViewCellStyle6.Alignment = DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle6.Font = new Font("メイリオ", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            geohash.DefaultCellStyle = dataGridViewCellStyle6;
            geohash.HeaderText = "geohash";
            geohash.Name = "geohash";
            geohash.ReadOnly = true;
            geohash.Resizable = DataGridViewTriState.True;
            geohash.SortMode = DataGridViewColumnSortMode.NotSortable;
            geohash.Width = 30;
            // 
            // id
            // 
            id.HeaderText = "id";
            id.Name = "id";
            id.ReadOnly = true;
            id.SortMode = DataGridViewColumnSortMode.NotSortable;
            id.Visible = false;
            // 
            // pubkey
            // 
            pubkey.HeaderText = "pubkey";
            pubkey.Name = "pubkey";
            pubkey.ReadOnly = true;
            pubkey.SortMode = DataGridViewColumnSortMode.NotSortable;
            pubkey.Visible = false;
            // 
            // kind
            // 
            kind.HeaderText = "kind";
            kind.Name = "kind";
            kind.ReadOnly = true;
            kind.SortMode = DataGridViewColumnSortMode.NotSortable;
            kind.Visible = false;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(384, 361);
            Controls.Add(dataGridViewNotes);
            Controls.Add(labelRelays);
            Controls.Add(buttonRelayList);
            Controls.Add(checkBoxPostBar);
            Controls.Add(buttonSetting);
            Controls.Add(buttonStop);
            Controls.Add(buttonStart);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MinimumSize = new Size(320, 320);
            Name = "FormMain";
            SizeGripStyle = SizeGripStyle.Show;
            StartPosition = FormStartPosition.Manual;
            Text = "omochat";
            TopMost = true;
            FormClosing += FormMain_FormClosing;
            Load += FormMain_Load;
            Shown += FormMain_Shown;
            SizeChanged += FormMain_SizeChanged;
            KeyDown += FormMain_KeyDown;
            MouseClick += FormMain_MouseClick;
            MouseDoubleClick += FormMain_MouseDoubleClick;
            ((System.ComponentModel.ISupportInitialize)dataGridViewNotes).EndInit();
            contextMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button buttonStart;
        private Button buttonStop;
        private Button buttonSetting;
        internal CheckBox checkBoxPostBar;
        private Button buttonRelayList;
        private Label labelRelays;
        private ToolTip toolTipRelays;
        internal DataGridView dataGridViewNotes;
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenuStrip;
        private ToolStripMenuItem settingToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
        private ToolStripMenuItem quitToolStripMenuItem;
        private DataGridViewTextBoxColumn time;
        private DataGridViewTextBoxColumn name;
        private DataGridViewTextBoxColumn hash;
        private DataGridViewTextBoxColumn note;
        private DataGridViewTextBoxColumn geohash;
        private DataGridViewTextBoxColumn id;
        private DataGridViewTextBoxColumn pubkey;
        private DataGridViewTextBoxColumn kind;
    }
}
