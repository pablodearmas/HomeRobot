
namespace HomeRobot.Test
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
            this.components = new System.ComponentModel.Container();
            this.mnuMain = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpenConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuNewSimulation = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpenSimulation = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSaveSimulation = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRun = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStart = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStep = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSettings = new System.Windows.Forms.ToolStripComboBox();
            this.mnuStrategy = new System.Windows.Forms.ToolStripComboBox();
            this.dlgConfig = new System.Windows.Forms.OpenFileDialog();
            this.pnlMain = new System.Windows.Forms.Panel();
            this.pbcEnv = new System.Windows.Forms.PictureBox();
            this.stbMain = new System.Windows.Forms.StatusStrip();
            this.lblDirtyPercent = new System.Windows.Forms.ToolStripStatusLabel();
            this.pbrDirtyPercent = new System.Windows.Forms.ToolStripProgressBar();
            this.lblDirtyPercentVal = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStepCounter = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStepCounterVal = new System.Windows.Forms.ToolStripStatusLabel();
            this.tmrSimulation = new System.Windows.Forms.Timer(this.components);
            this.mnuMain.SuspendLayout();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbcEnv)).BeginInit();
            this.stbMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuMain
            // 
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuRun,
            this.mnuSettings,
            this.mnuStrategy});
            this.mnuMain.Location = new System.Drawing.Point(0, 0);
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Size = new System.Drawing.Size(800, 27);
            this.mnuMain.TabIndex = 0;
            this.mnuMain.Text = "menuStrip1";
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuOpenConfig,
            this.toolStripMenuItem1,
            this.mnuNewSimulation,
            this.mnuOpenSimulation,
            this.mnuSaveSimulation,
            this.toolStripMenuItem2,
            this.mnuExit});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(37, 23);
            this.mnuFile.Text = "&File";
            // 
            // mnuOpenConfig
            // 
            this.mnuOpenConfig.Name = "mnuOpenConfig";
            this.mnuOpenConfig.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.mnuOpenConfig.Size = new System.Drawing.Size(215, 22);
            this.mnuOpenConfig.Text = "Open &Config";
            this.mnuOpenConfig.Click += new System.EventHandler(this.CmOpenConfig);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(212, 6);
            // 
            // mnuNewSimulation
            // 
            this.mnuNewSimulation.Name = "mnuNewSimulation";
            this.mnuNewSimulation.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.mnuNewSimulation.Size = new System.Drawing.Size(215, 22);
            this.mnuNewSimulation.Text = "&New Simulation";
            this.mnuNewSimulation.Click += new System.EventHandler(this.CmNewSimulation);
            // 
            // mnuOpenSimulation
            // 
            this.mnuOpenSimulation.Name = "mnuOpenSimulation";
            this.mnuOpenSimulation.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.mnuOpenSimulation.Size = new System.Drawing.Size(215, 22);
            this.mnuOpenSimulation.Text = "&Open Simulation...";
            // 
            // mnuSaveSimulation
            // 
            this.mnuSaveSimulation.Name = "mnuSaveSimulation";
            this.mnuSaveSimulation.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.mnuSaveSimulation.Size = new System.Drawing.Size(215, 22);
            this.mnuSaveSimulation.Text = "&Save Simulation...";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(212, 6);
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.mnuExit.Size = new System.Drawing.Size(215, 22);
            this.mnuExit.Text = "E&xit";
            this.mnuExit.Click += new System.EventHandler(this.CmExit);
            // 
            // mnuRun
            // 
            this.mnuRun.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuStart,
            this.mnuStop,
            this.mnuStep});
            this.mnuRun.Name = "mnuRun";
            this.mnuRun.Size = new System.Drawing.Size(40, 23);
            this.mnuRun.Text = "&Run";
            // 
            // mnuStart
            // 
            this.mnuStart.Name = "mnuStart";
            this.mnuStart.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.mnuStart.Size = new System.Drawing.Size(149, 22);
            this.mnuStart.Text = "&Start";
            this.mnuStart.Click += new System.EventHandler(this.CmStartStop);
            // 
            // mnuStop
            // 
            this.mnuStop.Name = "mnuStop";
            this.mnuStop.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F5)));
            this.mnuStop.Size = new System.Drawing.Size(149, 22);
            this.mnuStop.Text = "Sto&p";
            this.mnuStop.Click += new System.EventHandler(this.CmStartStop);
            // 
            // mnuStep
            // 
            this.mnuStep.Name = "mnuStep";
            this.mnuStep.ShortcutKeys = System.Windows.Forms.Keys.F10;
            this.mnuStep.Size = new System.Drawing.Size(149, 22);
            this.mnuStep.Text = "S&tep";
            this.mnuStep.Click += new System.EventHandler(this.CmStep);
            // 
            // mnuSettings
            // 
            this.mnuSettings.AutoSize = false;
            this.mnuSettings.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mnuSettings.Name = "mnuSettings";
            this.mnuSettings.Size = new System.Drawing.Size(300, 23);
            this.mnuSettings.SelectedIndexChanged += new System.EventHandler(this.mnuSettings_SelectedIndexChanged);
            // 
            // mnuStrategy
            // 
            this.mnuStrategy.AutoSize = false;
            this.mnuStrategy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.mnuStrategy.Name = "mnuStrategy";
            this.mnuStrategy.Size = new System.Drawing.Size(300, 23);
            // 
            // dlgConfig
            // 
            this.dlgConfig.Filter = "Config files|*.json";
            this.dlgConfig.Title = "Open Configuration";
            // 
            // pnlMain
            // 
            this.pnlMain.AutoScroll = true;
            this.pnlMain.Controls.Add(this.pbcEnv);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 27);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(800, 423);
            this.pnlMain.TabIndex = 1;
            // 
            // pbcEnv
            // 
            this.pbcEnv.Location = new System.Drawing.Point(0, 3);
            this.pbcEnv.Name = "pbcEnv";
            this.pbcEnv.Size = new System.Drawing.Size(100, 50);
            this.pbcEnv.TabIndex = 2;
            this.pbcEnv.TabStop = false;
            this.pbcEnv.Paint += new System.Windows.Forms.PaintEventHandler(this.pbcEnvPaint);
            // 
            // stbMain
            // 
            this.stbMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblDirtyPercent,
            this.pbrDirtyPercent,
            this.lblDirtyPercentVal,
            this.lblStepCounter,
            this.lblStepCounterVal});
            this.stbMain.Location = new System.Drawing.Point(0, 428);
            this.stbMain.Name = "stbMain";
            this.stbMain.Size = new System.Drawing.Size(800, 22);
            this.stbMain.TabIndex = 2;
            this.stbMain.Text = "statusStrip1";
            // 
            // lblDirtyPercent
            // 
            this.lblDirtyPercent.Name = "lblDirtyPercent";
            this.lblDirtyPercent.Size = new System.Drawing.Size(78, 17);
            this.lblDirtyPercent.Text = "Dirty Percent:";
            // 
            // pbrDirtyPercent
            // 
            this.pbrDirtyPercent.AutoToolTip = true;
            this.pbrDirtyPercent.Name = "pbrDirtyPercent";
            this.pbrDirtyPercent.Size = new System.Drawing.Size(100, 16);
            // 
            // lblDirtyPercentVal
            // 
            this.lblDirtyPercentVal.Name = "lblDirtyPercentVal";
            this.lblDirtyPercentVal.Size = new System.Drawing.Size(0, 17);
            // 
            // lblStepCounter
            // 
            this.lblStepCounter.Name = "lblStepCounter";
            this.lblStepCounter.Size = new System.Drawing.Size(33, 17);
            this.lblStepCounter.Text = "Step:";
            // 
            // lblStepCounterVal
            // 
            this.lblStepCounterVal.Name = "lblStepCounterVal";
            this.lblStepCounterVal.Size = new System.Drawing.Size(0, 17);
            // 
            // tmrSimulation
            // 
            this.tmrSimulation.Interval = 500;
            this.tmrSimulation.Tick += new System.EventHandler(this.tmrSimulation_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.stbMain);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.mnuMain);
            this.MainMenuStrip = this.mnuMain;
            this.Name = "MainForm";
            this.Text = "Home Robot Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.mnuMain.ResumeLayout(false);
            this.mnuMain.PerformLayout();
            this.pnlMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbcEnv)).EndInit();
            this.stbMain.ResumeLayout(false);
            this.stbMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip mnuMain;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuRun;
        private System.Windows.Forms.ToolStripMenuItem mnuStart;
        private System.Windows.Forms.ToolStripMenuItem mnuStop;
        private System.Windows.Forms.ToolStripMenuItem mnuStep;
        private System.Windows.Forms.ToolStripMenuItem mnuOpenConfig;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuOpenSimulation;
        private System.Windows.Forms.ToolStripMenuItem mnuSaveSimulation;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
        private System.Windows.Forms.ToolStripMenuItem mnuNewSimulation;
        private System.Windows.Forms.ToolStripComboBox mnuSettings;
        private System.Windows.Forms.OpenFileDialog dlgConfig;
        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.PictureBox pbcEnv;
        private System.Windows.Forms.StatusStrip stbMain;
        private System.Windows.Forms.ToolStripStatusLabel lblDirtyPercent;
        private System.Windows.Forms.ToolStripProgressBar pbrDirtyPercent;
        private System.Windows.Forms.ToolStripStatusLabel lblDirtyPercentVal;
        private System.Windows.Forms.Timer tmrSimulation;
        private System.Windows.Forms.ToolStripStatusLabel lblStepCounter;
        private System.Windows.Forms.ToolStripStatusLabel lblStepCounterVal;
        private System.Windows.Forms.ToolStripComboBox mnuStrategy;
    }
}

