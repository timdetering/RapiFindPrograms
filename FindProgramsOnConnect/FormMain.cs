// FormMain.cs - main user-interface for FindProgramsOnConnect
//
// Code from _Programming the .NET Compact Framework with C#_
// and _Programming the .NET Compact Framework with VB_
// (c) Copyright 2002-2004 Paul Yao and David Durant. 
// All rights reserved.

using System;
using System.Collections;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using YaoDurant.Win32;

namespace FindPrograms
{
   /// <summary>
   /// FormMain - program main window.
   /// </summary>
   public class FormMain : System.Windows.Forms.Form
   {
      private System.Windows.Forms.Button cmdConnect;
      private System.Windows.Forms.Button cmdDisconnect;
      private System.Windows.Forms.Button cmdFind;
      private System.Windows.Forms.Button cmdRun;
      private System.Windows.Forms.Button cmdAbout;

      // Startup thread definitions
      private StartupThread m_thrdStartup = null;
      private EventHandler m_deleStartup;
      private bool m_bRapiConnected = false;

      // Find Files thread definitions
      private FindFilesThread m_thrdFindFiles = null;
      private EventHandler m_deleFindFiles;

      // Connect detect definitions
      private RapiConnectDetect m_rapicd = null;
      private EventHandler m_deleConnect;

      /// <summary>
      /// Required designer variable.
      /// </summary>
      private System.ComponentModel.Container components = null;
      private System.Windows.Forms.StatusBar sbarMain;
      private System.Windows.Forms.Label label1;
      private System.Windows.Forms.TextBox txtStartDir;
      private System.Windows.Forms.CheckBox chkSubs;
      private System.Windows.Forms.ListBox lboxPrograms;
      private const string m_strAppName = "FindProgramsOnConnect";

      public FormMain()
      {
         //
         // Required for Windows Form Designer support
         //
         InitializeComponent();

      }

      /// <summary>
      /// Clean up any resources being used.
      /// </summary>
      protected override void Dispose( bool disposing )
      {
         if( disposing )
         {
            if (components != null) 
            {
               components.Dispose();
            }
         }
         base.Dispose( disposing );
      }

      #region Windows Form Designer generated code
      /// <summary>
      /// Required method for Designer support - do not modify
      /// the contents of this method with the code editor.
      /// </summary>
      private void InitializeComponent()
      {
         this.cmdConnect = new System.Windows.Forms.Button();
         this.cmdDisconnect = new System.Windows.Forms.Button();
         this.cmdFind = new System.Windows.Forms.Button();
         this.cmdRun = new System.Windows.Forms.Button();
         this.lboxPrograms = new System.Windows.Forms.ListBox();
         this.cmdAbout = new System.Windows.Forms.Button();
         this.sbarMain = new System.Windows.Forms.StatusBar();
         this.label1 = new System.Windows.Forms.Label();
         this.txtStartDir = new System.Windows.Forms.TextBox();
         this.chkSubs = new System.Windows.Forms.CheckBox();
         this.SuspendLayout();
         // 
         // cmdConnect
         // 
         this.cmdConnect.Location = new System.Drawing.Point(448, 32);
         this.cmdConnect.Name = "cmdConnect";
         this.cmdConnect.Size = new System.Drawing.Size(96, 23);
         this.cmdConnect.TabIndex = 0;
         this.cmdConnect.Text = "Connect";
         this.cmdConnect.Click += new System.EventHandler(this.cmdConnect_Click);
         // 
         // cmdDisconnect
         // 
         this.cmdDisconnect.Enabled = false;
         this.cmdDisconnect.Location = new System.Drawing.Point(448, 72);
         this.cmdDisconnect.Name = "cmdDisconnect";
         this.cmdDisconnect.Size = new System.Drawing.Size(96, 23);
         this.cmdDisconnect.TabIndex = 1;
         this.cmdDisconnect.Text = "Disconnect";
         this.cmdDisconnect.Click += new System.EventHandler(this.cmdDisconnect_Click);
         // 
         // cmdFind
         // 
         this.cmdFind.Enabled = false;
         this.cmdFind.Location = new System.Drawing.Point(448, 112);
         this.cmdFind.Name = "cmdFind";
         this.cmdFind.Size = new System.Drawing.Size(96, 23);
         this.cmdFind.TabIndex = 2;
         this.cmdFind.Text = "Find Programs";
         this.cmdFind.Click += new System.EventHandler(this.cmdFind_Click);
         // 
         // cmdRun
         // 
         this.cmdRun.Enabled = false;
         this.cmdRun.Location = new System.Drawing.Point(448, 152);
         this.cmdRun.Name = "cmdRun";
         this.cmdRun.Size = new System.Drawing.Size(96, 23);
         this.cmdRun.TabIndex = 3;
         this.cmdRun.Text = "Run";
         this.cmdRun.Click += new System.EventHandler(this.cmdRun_Click);
         // 
         // lboxPrograms
         // 
         this.lboxPrograms.Location = new System.Drawing.Point(8, 40);
         this.lboxPrograms.Name = "lboxPrograms";
         this.lboxPrograms.Size = new System.Drawing.Size(416, 173);
         this.lboxPrograms.TabIndex = 4;
         // 
         // cmdAbout
         // 
         this.cmdAbout.Location = new System.Drawing.Point(448, 192);
         this.cmdAbout.Name = "cmdAbout";
         this.cmdAbout.Size = new System.Drawing.Size(96, 23);
         this.cmdAbout.TabIndex = 5;
         this.cmdAbout.Text = "About";
         this.cmdAbout.Click += new System.EventHandler(this.cmdAbout_Click);
         // 
         // sbarMain
         // 
         this.sbarMain.Location = new System.Drawing.Point(0, 229);
         this.sbarMain.Name = "sbarMain";
         this.sbarMain.Size = new System.Drawing.Size(560, 22);
         this.sbarMain.TabIndex = 6;
         // 
         // label1
         // 
         this.label1.Location = new System.Drawing.Point(8, 8);
         this.label1.Name = "label1";
         this.label1.Size = new System.Drawing.Size(100, 16);
         this.label1.TabIndex = 7;
         this.label1.Text = "Starting Directory:";
         // 
         // txtStartDir
         // 
         this.txtStartDir.Location = new System.Drawing.Point(112, 8);
         this.txtStartDir.Name = "txtStartDir";
         this.txtStartDir.Size = new System.Drawing.Size(312, 20);
         this.txtStartDir.TabIndex = 8;
         this.txtStartDir.Text = "";
         // 
         // chkSubs
         // 
         this.chkSubs.Location = new System.Drawing.Point(440, 8);
         this.chkSubs.Name = "chkSubs";
         this.chkSubs.Size = new System.Drawing.Size(104, 16);
         this.chkSubs.TabIndex = 9;
         this.chkSubs.Text = "Sub-directories";
         // 
         // FormMain
         // 
         this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
         this.ClientSize = new System.Drawing.Size(560, 251);
         this.Controls.Add(this.chkSubs);
         this.Controls.Add(this.txtStartDir);
         this.Controls.Add(this.label1);
         this.Controls.Add(this.sbarMain);
         this.Controls.Add(this.cmdAbout);
         this.Controls.Add(this.lboxPrograms);
         this.Controls.Add(this.cmdRun);
         this.Controls.Add(this.cmdFind);
         this.Controls.Add(this.cmdDisconnect);
         this.Controls.Add(this.cmdConnect);
         this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
         this.Name = "FormMain";
         this.Text = "RAPI - FindProgramsOnConnect";
         this.Load += new System.EventHandler(this.FormMain_Load);
         this.Closed += new System.EventHandler(this.FormMain_Closed);
         this.ResumeLayout(false);

      }
      #endregion

      /// <summary>
      /// The main entry point for the application.
      /// </summary>
      [STAThread]
      static void Main() 
      {
         Application.Run(new FormMain());
      }

      private void FormMain_Load(object sender, System.EventArgs e)
      {
         sbarMain.Text = "Ready";
         Trace.WriteLine("FindProgramsOnConnect: Starting...\r\n");
         
         // Setup inter-thread delegates.
         m_deleStartup = new EventHandler(this.StartupCallback);
         m_deleFindFiles = new EventHandler(this.FindFilesCallback);
         m_deleConnect = new EventHandler(this.DetectCallback);

         // Create detect connect object.
         m_rapicd = new RapiConnectDetect(this, this.m_deleConnect);
         m_rapicd.Init();
         m_rapicd.Enable(true);
      }

      private void FormMain_Closed(object sender, System.EventArgs e)
      {
         // If threads are running, trigger shutdown.
         if (this.m_thrdStartup != null)
            this.m_thrdStartup.bThreadContinue = false;
         if (this.m_thrdFindFiles != null)
            this.m_thrdFindFiles.bThreadContinue = false;

         if (m_bRapiConnected)
         {
            Rapi.CeRapiUninit();
            m_bRapiConnected = false;
         }
      }

      private void 
      cmdAbout_Click(object sender, System.EventArgs e)
      {
         MessageBox.Show("(c) Copyright 2002-2004 " +
            "Paul Yao and David Durant\n\n" +
            "FindProgramsOnConnect - RAPI Sample for \n" +
            "Programming the .NET Compact Framework with C#,\n"+
            "& Programming the .NET Compact Framework with VB."
            , m_strAppName);
      }

      private void 
      cmdConnect_Click(object sender, System.EventArgs e)
      {
         // Update UI.
         sbarMain.Text = "Connecting...";
         this.cmdConnect.Enabled = false;

         // Create thread to connect to RAPI.
         m_thrdStartup = new StartupThread(this, m_deleStartup);
         if (!m_thrdStartup.Run())
            m_thrdStartup = null;

         // Clear out prvevious contents of file listbox.
         lboxPrograms.Items.Clear();
      }

      private void 
      cmdDisconnect_Click(object sender, System.EventArgs e)
      {
         sbarMain.Text = "Disconnecting...";

         // Trigger thread to stop running.
         if (m_thrdFindFiles != null)
            m_thrdFindFiles.bThreadContinue = false;

         // Disconnect from RAPI.
         Rapi.CeRapiUninit();

         ResetUI();

         // Clear out previous contents of file listbox.
         lboxPrograms.Items.Clear();
         
         m_bRapiConnected = false;
      }

      private void 
      cmdRun_Click(object sender, System.EventArgs e)
      {
         sbarMain.Text = "Attempting to run program...";

         int iItem = lboxPrograms.SelectedIndex;
         string strProg = lboxPrograms.Items[iItem].ToString();
            
         if (strProg.Length > 0)
         {
            Rapi.PROCESS_INFORMATION pi = 
               new Rapi.PROCESS_INFORMATION();
            Rapi.CeCreateProcess(strProg, 0, 0, 0, 0,  
               0, 0, 0, 0, ref pi);

            Rapi.CeCloseHandle(pi.hProcess);
            Rapi.CeCloseHandle(pi.hThread);
         }

         sbarMain.Text = "Ready";
      }

      private void 
      cmdFind_Click(object sender, System.EventArgs e)
      {
         // Disable Find button.
         this.cmdFind.Enabled = false;

         // Clear out previous contents of file listbox.
         lboxPrograms.Items.Clear();
         
         m_thrdFindFiles = new FindFilesThread(this, m_deleFindFiles);
         if (!m_thrdFindFiles.Run(this.chkSubs.Checked))
            m_thrdFindFiles = null;
      }

      /// <summary>
      /// StartupCallback - Interthread delegate.
      /// </summary>
      /// <param name="sender">unused</param>
      /// <param name="e">unused</param>
      private void 
      StartupCallback(object sender, System.EventArgs e)
      {
         INVOKE_STARTUP it = this.m_thrdStartup.itReason;
         switch(it)
         {
            case INVOKE_STARTUP.STARTUP_SUCCESS:
               m_bRapiConnected = true;
               EnableUI();
               break;
            case INVOKE_STARTUP.STARTUP_FAILED:
               ResetUI();
               break;
            case INVOKE_STARTUP.STATUS_MESSAGE:
               sbarMain.Text = m_thrdStartup.strBuffer;
               break;
         }
      }

      /// <summary>
      /// FindFilesCallback - Interthread delegate.
      /// </summary>
      /// <param name="sender">unused</param>
      /// <param name="e">unused</param>
      private void 
      FindFilesCallback(object sender, System.EventArgs e)
      {
         INVOKE_FINDFILES it = this.m_thrdFindFiles.itReason;
         switch(it)
         {
            case INVOKE_FINDFILES.FINDFILE_QUERYSTARTPATH:
               string strStart = txtStartDir.Text;
               if (strStart.EndsWith("\\") == false)
                  strStart = strStart + "\\";
               m_thrdFindFiles.strBuffer = strStart;
               break;
            case INVOKE_FINDFILES.FINDFILE_NEWFILE:
               lboxPrograms.Items.Add(m_thrdFindFiles.strBuffer);
               break;
            case INVOKE_FINDFILES.FINDFILE_COMPLETE:
               this.cmdFind.Enabled = true;
               break;
            case INVOKE_FINDFILES.STATUS_MESSAGE:
               sbarMain.Text = m_thrdFindFiles.strBuffer;
               break;
         }
      }

      /// <summary>
      /// DetectCallback - Called to notify start and end of
      /// RAPI connections.
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      private void 
      DetectCallback(object sender, System.EventArgs e)
      {
         INVOKE_CONNECT it = this.m_rapicd.itReason;
         switch(it)
         {
            case INVOKE_CONNECT.START:
               this.cmdConnect_Click(this, new EventArgs());
               this.txtStartDir.Text = @"\";
               this.chkSubs.Checked = true;
               cmdFind.Enabled = true;
               this.cmdFind_Click(this, new EventArgs());
               break;
            case INVOKE_CONNECT.STOP:
               this.cmdDisconnect_Click(this, new EventArgs());
               break;
         }
      }

      /// <summary>
      /// EnableUI - Connection established.
      /// </summary>
      private void EnableUI()
      {
         this.cmdDisconnect.Enabled = true;
         this.cmdFind.Enabled = true;
         this.cmdRun.Enabled = true;
         this.cmdConnect.Enabled = false;
         sbarMain.Text = "Connected";
      }

      /// <summary>
      /// ResetUI - Connection terminated.
      /// </summary>
      private void ResetUI()
      {
         this.cmdDisconnect.Enabled = false;
         this.cmdFind.Enabled = false;
         this.cmdRun.Enabled = false;
         this.cmdConnect.Enabled = true;
         sbarMain.Text = "Ready";
      }

   } // class FormMain
} // namespace FindPrograms
