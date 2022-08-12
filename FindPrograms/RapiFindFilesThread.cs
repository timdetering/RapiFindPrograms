// Program Name: FindPrograms.exe
//
// RapiFindFilesThread.cs - Creates a background 
// thread to retrieve file names from the device.
//
// Code from _Programming the .NET Compact Framework with C#_
// and _Programming the .NET Compact Framework with VB_
// (c) Copyright 2002-2004 Paul Yao and David Durant. 
// All rights reserved.

using System;
using System.Threading;
using System.Collections;
using System.Windows.Forms;
using System.Diagnostics;
using YaoDurant.Win32;

namespace FindPrograms
{
   // Reasons our thread invokes user-interface thread.
   public enum INVOKE_FINDFILES
   {
      FINDFILE_QUERYSTARTPATH,
      FINDFILE_NEWFILE,
      FINDFILE_COMPLETE,
      STATUS_MESSAGE
   }

   /// <summary>
   /// FindFilesThread wraps a thread that supports
   /// RAPI search of device directory.
   /// </summary>
   public class FindFilesThread
   {
      public string strBuffer;       // Inter-thread buffer
      public INVOKE_FINDFILES itReason;   // Inter-thread reason

      private Thread m_thrd = null;    // The contained thread
      private Control m_ctlInvokeTarget; // Inter-thread control
      private EventHandler m_deleCallback; // Inter-thread delegate
      private bool m_bContinue; // Continue flag.
      private bool m_bIncludeSubDirs = false; // Search sub-dirs
      private int m_cFiles = 0;   // Find-File counter.

      public bool bThreadContinue // Continue property.
      {
         get { return m_bContinue; }
         set { m_bContinue = value; } 
      }

      /// <summary>
      /// FindFilesThread - Constructor.
      /// </summary>
      /// <param name="ctl">Owner control</param>
      /// <param name="dele">Delegate to invoke</param>
      public FindFilesThread(Control ctl, EventHandler dele)
      {
         bThreadContinue = true;
         m_ctlInvokeTarget = ctl;  // Who to call.
         m_deleCallback = dele;    // How to call.
      }

      /// <summary>
      /// Run - Init function for find-files thread.
      /// </summary>
      /// <param name="bSubDirs"></param>
      /// <returns></returns>
      public bool Run(bool bSubDirs)
      {
         ThreadStart ts = null;
         ts = new ThreadStart(ThreadMainFindFiles);
         if (ts == null)
            return false;

         m_bIncludeSubDirs = bSubDirs;

         m_thrd = new Thread(ts);
         m_thrd.Start();
         return true;
      }

      /// <summary>
      /// ThreadMainFindFiles - Main thread for file find thread
      /// </summary>
      private void ThreadMainFindFiles()
      {
         int cTicks = Environment.TickCount;

         itReason = INVOKE_FINDFILES.FINDFILE_QUERYSTARTPATH;
         m_ctlInvokeTarget.Invoke(m_deleCallback);
         string strPath = strBuffer;
         AddProgramsInDirectory(strPath);

         int cSeconds = (Environment.TickCount - cTicks + 500) / 1000;
         if (bThreadContinue)
         {
            // Send message for search time.
            strBuffer = "Ready - " + m_cFiles +
               " programs found in " + cSeconds + " seconds.";
            itReason = INVOKE_FINDFILES.STATUS_MESSAGE;
            m_ctlInvokeTarget.Invoke(m_deleCallback);

            // Trigger that search is done.
            itReason = INVOKE_FINDFILES.FINDFILE_COMPLETE;
            m_ctlInvokeTarget.Invoke(m_deleCallback);
         }
      }

      /// <summary>
      /// AddProgramsInDirectory - Recursive function to search
      /// into directory tree.
      /// </summary>
      /// <param name="strDir">Starting directory</param>
      /// <returns></returns>
      private bool AddProgramsInDirectory(string strDir)
      {
         Trace.WriteLine("FindPrograms: " +
            "AddProgramsInDirectory (" + strDir + ")");

         // Update status bar through delegate function.
         strBuffer = "Searching in " + strDir + "...";
         itReason = INVOKE_FINDFILES.STATUS_MESSAGE;
         m_ctlInvokeTarget.Invoke(m_deleCallback);

         // As we add programs, store directory names.
         ArrayList alDirectories = new ArrayList();
         Rapi.CE_FIND_DATA fd = new Rapi.CE_FIND_DATA();

         // Start our search.
         string strSearch = strDir + "*.*";
         IntPtr hff = Rapi.CeFindFirstFile(strSearch, ref fd);
         if ((int)hff == -1)
         {
            FetchAndDisplayError();
         }
         else
         {
            do
            {
               string strFileName = fd.cFileName;
               int iFlag = (int)
                  Rapi.FILE_ATTRIBUTE.FILE_ATTRIBUTE_DIRECTORY;
               if ((fd.dwFileAttributes & iFlag) == iFlag)
               {
                  alDirectories.Add(strDir+fd.cFileName);
               }
               else
               {
                  if (strFileName.EndsWith(".EXE") || 
                     strFileName.EndsWith(".exe"))
                  {
                     m_cFiles++;

                     strBuffer = strDir + fd.cFileName;
                     itReason = INVOKE_FINDFILES.FINDFILE_NEWFILE;
                     m_ctlInvokeTarget.Invoke(m_deleCallback);
                  }
               }
            } while (bThreadContinue &&
               Rapi.CeFindNextFile(hff, ref fd) != 0);
            
            if (bThreadContinue && m_bIncludeSubDirs)
            {
               foreach (string str in alDirectories)
               {
                  AddProgramsInDirectory(str + "\\");
               }
            }
         }

         Rapi.CeFindClose(hff);

         return true;
      }
      
      /// <summary>
      /// FetchAndDisplayError - Display error in status bar
      /// </summary>
      public void FetchAndDisplayError()
      {
         strBuffer = string.Empty;

         // Is this a RAPI error?
         int err = Rapi.CeRapiGetError();
         if (err != Rapi.S_OK)
         {
            strBuffer = "RAPI Error (0x" + ((int)err).ToString("x") +")";
         }
         else
         {
            // Check for CE error.
            err = Rapi.CeGetLastError();
            if (err != (int)Rapi.RAPI_ERROR.ERROR_FILE_NOT_FOUND)
            {
               strBuffer = "CE Error (code = " +
                  err.ToString("x") + ")";
             }
         }
         if (strBuffer != string.Empty)
         {
            itReason = INVOKE_FINDFILES.STATUS_MESSAGE;
            m_ctlInvokeTarget.Invoke(m_deleCallback);
         }
         
         // Trigger that thread has ended.
         m_thrd = null;
      }
   } // class FindFilesThread
} // namespace FindPrograms
