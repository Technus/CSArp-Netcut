using System;
using System.Reflection;
using System.Windows.Forms;

namespace CSArp.View
{
    partial class ScannerForm
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
                ExitGracefully().AsTask().Wait();
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
            components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(ScannerForm));
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItemSaveSettings = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            toolStripMenuItemMinimize = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItemChooseInterface = new ToolStripMenuItem();
            toolStripComboBoxDevicelist = new ToolStripComboBox();
            toolStripSeparator3 = new ToolStripSeparator();
            ClientNametoolStripMenuItem = new ToolStripMenuItem();
            toolStripTextBoxClientName = new ToolStripTextBox();
            toolStripSeparator1 = new ToolStripSeparator();
            cutoffToolStripMenuItem = new ToolStripMenuItem();
            reconnectToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripMenuItemRefreshClients = new ToolStripMenuItem();
            stopNetworkScanToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            aboutCSArpToolStripMenuItem = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            toolStripStatus = new ToolStripStatusLabel();
            toolStripStatusLabelSpringer = new ToolStripStatusLabel();
            toolStripStatusScan = new ToolStripStatusLabel();
            toolStripProgressBarScan = new ToolStripProgressBar();
            toolStripSplitButton1 = new ToolStripSplitButton();
            clearStripMenuItem = new ToolStripMenuItem();
            saveStripMenuItem = new ToolStripMenuItem();
            showLogToolStripMenuItem = new ToolStripMenuItem();
            clientListView = new ListView();
            columnHeaderIP = new ColumnHeader();
            columnHeaderMAC = new ColumnHeader();
            columnHeaderStatus = new ColumnHeader();
            columnHeaderSpoofing = new ColumnHeader();
            columnHeaderClientname = new ColumnHeader();
            columnHeaderTimestamp = new ColumnHeader();
            columnHeaderSN = new ColumnHeader();
            notifyIcon1 = new NotifyIcon(components);
            richTextBoxLog = new RichTextBox();
            saveFileDialog1 = new SaveFileDialog();
            menuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Padding = new Padding(7, 2, 0, 2);
            menuStrip1.Size = new System.Drawing.Size(665, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemSaveSettings, toolStripSeparator4, toolStripMenuItemMinimize, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // toolStripMenuItemSaveSettings
            // 
            toolStripMenuItemSaveSettings.Name = "toolStripMenuItemSaveSettings";
            toolStripMenuItemSaveSettings.ShortcutKeys = Keys.Control | Keys.S;
            toolStripMenuItemSaveSettings.Size = new System.Drawing.Size(163, 22);
            toolStripMenuItemSaveSettings.Text = "Save";
            toolStripMenuItemSaveSettings.ToolTipText = "Save current settings";
            toolStripMenuItemSaveSettings.Click += toolStripMenuItemSaveSettings_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new System.Drawing.Size(160, 6);
            // 
            // toolStripMenuItemMinimize
            // 
            toolStripMenuItemMinimize.Name = "toolStripMenuItemMinimize";
            toolStripMenuItemMinimize.ShortcutKeys = Keys.Control | Keys.T;
            toolStripMenuItemMinimize.Size = new System.Drawing.Size(163, 22);
            toolStripMenuItemMinimize.Text = "Minimize";
            toolStripMenuItemMinimize.ToolTipText = "Minimize to tray";
            toolStripMenuItemMinimize.Click += toolStripMenuItemMinimize_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Q;
            exitToolStripMenuItem.Size = new System.Drawing.Size(163, 22);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripMenuItemChooseInterface, toolStripSeparator3, ClientNametoolStripMenuItem, toolStripSeparator1, cutoffToolStripMenuItem, reconnectToolStripMenuItem, toolStripSeparator2, toolStripMenuItemRefreshClients, stopNetworkScanToolStripMenuItem });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            editToolStripMenuItem.Text = "Edit";
            // 
            // toolStripMenuItemChooseInterface
            // 
            toolStripMenuItemChooseInterface.DropDownItems.AddRange(new ToolStripItem[] { toolStripComboBoxDevicelist });
            toolStripMenuItemChooseInterface.Name = "toolStripMenuItemChooseInterface";
            toolStripMenuItemChooseInterface.Size = new System.Drawing.Size(193, 22);
            toolStripMenuItemChooseInterface.Text = "Choose interface";
            toolStripMenuItemChooseInterface.ToolTipText = "Select the network card to operate on";
            // 
            // toolStripComboBoxDevicelist
            // 
            toolStripComboBoxDevicelist.Name = "toolStripComboBoxDevicelist";
            toolStripComboBoxDevicelist.Size = new System.Drawing.Size(121, 23);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(190, 6);
            // 
            // ClientNametoolStripMenuItem
            // 
            ClientNametoolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { toolStripTextBoxClientName });
            ClientNametoolStripMenuItem.Name = "ClientNametoolStripMenuItem";
            ClientNametoolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            ClientNametoolStripMenuItem.Text = "Enter Client Name";
            ClientNametoolStripMenuItem.ToolTipText = "Enter a name for the selected client";
            // 
            // toolStripTextBoxClientName
            // 
            toolStripTextBoxClientName.Name = "toolStripTextBoxClientName";
            toolStripTextBoxClientName.Size = new System.Drawing.Size(100, 23);
            toolStripTextBoxClientName.KeyUp += toolStripTextBoxClientName_KeyUp;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(190, 6);
            // 
            // cutoffToolStripMenuItem
            // 
            cutoffToolStripMenuItem.Name = "cutoffToolStripMenuItem";
            cutoffToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            cutoffToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            cutoffToolStripMenuItem.Text = "Start Spoofing";
            cutoffToolStripMenuItem.ToolTipText = "Disconnect selected clients";
            cutoffToolStripMenuItem.Click += cutoffToolStripMenuItem_Click;
            // 
            // reconnectToolStripMenuItem
            // 
            reconnectToolStripMenuItem.Name = "reconnectToolStripMenuItem";
            reconnectToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.R;
            reconnectToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            reconnectToolStripMenuItem.Text = "Stop Spoofing";
            reconnectToolStripMenuItem.ToolTipText = "Stop arp spoofing";
            reconnectToolStripMenuItem.Click += reconnectToolStripMenuItem_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(190, 6);
            // 
            // toolStripMenuItemRefreshClients
            // 
            toolStripMenuItemRefreshClients.Name = "toolStripMenuItemRefreshClients";
            toolStripMenuItemRefreshClients.ShortcutKeys = Keys.F5;
            toolStripMenuItemRefreshClients.Size = new System.Drawing.Size(193, 22);
            toolStripMenuItemRefreshClients.Text = "Start Network Scan";
            toolStripMenuItemRefreshClients.ToolTipText = "Refresh active client list";
            toolStripMenuItemRefreshClients.Click += toolStripMenuItemRefreshClients_Click;
            // 
            // stopNetworkScanToolStripMenuItem
            // 
            stopNetworkScanToolStripMenuItem.Name = "stopNetworkScanToolStripMenuItem";
            stopNetworkScanToolStripMenuItem.ShortcutKeys = Keys.F6;
            stopNetworkScanToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
            stopNetworkScanToolStripMenuItem.Text = "Stop Network Scan";
            stopNetworkScanToolStripMenuItem.ToolTipText = "Stop scanning";
            stopNetworkScanToolStripMenuItem.Click += stopNetworkScanToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aboutCSArpToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            // 
            // aboutCSArpToolStripMenuItem
            // 
            aboutCSArpToolStripMenuItem.Name = "aboutCSArpToolStripMenuItem";
            aboutCSArpToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            aboutCSArpToolStripMenuItem.Text = "About CSArp";
            aboutCSArpToolStripMenuItem.Click += aboutCSArpToolStripMenuItem_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatus, toolStripStatusLabelSpringer, toolStripStatusScan, toolStripProgressBarScan, toolStripSplitButton1 });
            statusStrip1.Location = new System.Drawing.Point(0, 364);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Padding = new Padding(1, 0, 16, 0);
            statusStrip1.Size = new System.Drawing.Size(665, 27);
            statusStrip1.TabIndex = 1;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatus
            // 
            toolStripStatus.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Right;
            toolStripStatus.Margin = new Padding(11, 3, 0, 2);
            toolStripStatus.Name = "toolStripStatus";
            toolStripStatus.Size = new System.Drawing.Size(43, 22);
            toolStripStatus.Text = "Ready";
            // 
            // toolStripStatusLabelSpringer
            // 
            toolStripStatusLabelSpringer.Name = "toolStripStatusLabelSpringer";
            toolStripStatusLabelSpringer.Size = new System.Drawing.Size(337, 22);
            toolStripStatusLabelSpringer.Spring = true;
            // 
            // toolStripStatusScan
            // 
            toolStripStatusScan.BorderSides = ToolStripStatusLabelBorderSides.Left | ToolStripStatusLabelBorderSides.Right;
            toolStripStatusScan.Name = "toolStripStatusScan";
            toolStripStatusScan.Size = new System.Drawing.Size(95, 22);
            toolStripStatusScan.Text = "Refresh for scan";
            // 
            // toolStripProgressBarScan
            // 
            toolStripProgressBarScan.Name = "toolStripProgressBarScan";
            toolStripProgressBarScan.Size = new System.Drawing.Size(117, 21);
            // 
            // toolStripSplitButton1
            // 
            toolStripSplitButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripSplitButton1.DropDownItems.AddRange(new ToolStripItem[] { clearStripMenuItem, saveStripMenuItem, showLogToolStripMenuItem });
            toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripSplitButton1.Name = "toolStripSplitButton1";
            toolStripSplitButton1.Size = new System.Drawing.Size(43, 25);
            toolStripSplitButton1.Text = "Log";
            // 
            // clearStripMenuItem
            // 
            clearStripMenuItem.Name = "clearStripMenuItem";
            clearStripMenuItem.Size = new System.Drawing.Size(123, 22);
            clearStripMenuItem.Text = "Clear";
            clearStripMenuItem.Click += clearStripMenuItem_Click;
            // 
            // saveStripMenuItem
            // 
            saveStripMenuItem.Name = "saveStripMenuItem";
            saveStripMenuItem.Size = new System.Drawing.Size(123, 22);
            saveStripMenuItem.Text = "Save";
            saveStripMenuItem.Click += saveStripMenuItem_Click;
            // 
            // showLogToolStripMenuItem
            // 
            showLogToolStripMenuItem.CheckOnClick = true;
            showLogToolStripMenuItem.Name = "showLogToolStripMenuItem";
            showLogToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            showLogToolStripMenuItem.Text = "Show log";
            showLogToolStripMenuItem.CheckStateChanged += showLogToolStripMenuItem_CheckStateChanged;
            // 
            // clientListView
            // 
            clientListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            clientListView.Columns.AddRange(new ColumnHeader[] { columnHeaderIP, columnHeaderMAC, columnHeaderStatus, columnHeaderSpoofing, columnHeaderClientname, columnHeaderTimestamp });
            clientListView.FullRowSelect = true;
            clientListView.GridLines = true;
            clientListView.Location = new System.Drawing.Point(14, 31);
            clientListView.Margin = new Padding(4, 3, 4, 3);
            clientListView.Name = "clientListView";
            clientListView.ShowItemToolTips = true;
            clientListView.Size = new System.Drawing.Size(636, 328);
            clientListView.TabIndex = 2;
            clientListView.UseCompatibleStateImageBehavior = false;
            clientListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderIP
            // 
            columnHeaderIP.Text = "IP Address";
            columnHeaderIP.Width = 100;
            // 
            // columnHeaderMAC
            // 
            columnHeaderMAC.Text = "MAC Address";
            columnHeaderMAC.Width = 125;
            // 
            // columnHeaderStatus
            // 
            columnHeaderStatus.Text = "Status";
            columnHeaderStatus.Width = 45;
            // 
            // columnHeaderSpoofing
            // 
            columnHeaderSpoofing.Text = "Spoof";
            // 
            // columnHeaderClientname
            // 
            columnHeaderClientname.Text = "Client Name";
            columnHeaderClientname.Width = 151;
            // 
            // columnHeaderTimestamp
            // 
            columnHeaderTimestamp.Text = "Timestamp";
            columnHeaderTimestamp.Width = 140;
            // 
            // columnHeaderSN
            // 
            columnHeaderSN.Text = "SN";
            columnHeaderSN.Width = 50;
            // 
            // notifyIcon1
            // 
            notifyIcon1.Text = "CSArp";
            notifyIcon1.Visible = true;
            notifyIcon1.MouseClick += notifyIcon1_OnMouseClick;
            // 
            // richTextBoxLog
            // 
            richTextBoxLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBoxLog.Location = new System.Drawing.Point(14, 262);
            richTextBoxLog.Margin = new Padding(4, 3, 4, 3);
            richTextBoxLog.Name = "richTextBoxLog";
            richTextBoxLog.ScrollBars = RichTextBoxScrollBars.Vertical;
            richTextBoxLog.Size = new System.Drawing.Size(638, 100);
            richTextBoxLog.TabIndex = 3;
            richTextBoxLog.Text = "";
            richTextBoxLog.Visible = false;
            // 
            // ScannerForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(665, 391);
            Controls.Add(richTextBoxLog);
            Controls.Add(clientListView);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Margin = new Padding(4, 3, 4, 3);
            Name = "ScannerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CSArp";
            Load += Form1_Load;
            Resize += Form1_Resize;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutCSArpToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatus;
        private System.Windows.Forms.ListView clientListView;
        private System.Windows.Forms.ColumnHeader columnHeaderSN;
        private System.Windows.Forms.ColumnHeader columnHeaderIP;
        private System.Windows.Forms.ColumnHeader columnHeaderMAC;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutoffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reconnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ColumnHeader columnHeaderClientname;
        private System.Windows.Forms.ToolStripMenuItem ClientNametoolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBoxClientName;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemSaveSettings;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemChooseInterface;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxDevicelist;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemMinimize;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRefreshClients;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSpringer;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusScan;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBarScan;
        private System.Windows.Forms.ToolStripSplitButton toolStripSplitButton1;
        private System.Windows.Forms.ToolStripMenuItem showLogToolStripMenuItem;
        private System.Windows.Forms.RichTextBox richTextBoxLog;
        private System.Windows.Forms.ToolStripMenuItem saveStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private ToolStripMenuItem stopNetworkScanToolStripMenuItem;
        private ColumnHeader columnHeaderTimestamp;
        private ColumnHeader columnHeaderSpoofing;
    }
}

