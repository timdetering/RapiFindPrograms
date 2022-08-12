// RapiStartupThread.cs - Creates a background thread
// for the purpose of starting RAPI.
//
// Code from _Programming the .NET Compact Framework with C#_
// and _Programming the .NET Compact Framework with VB_
// (c) Copyright 2002-2004 Paul Yao and David Durant. 
// All rights reserved.

using System;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using YaoDurant.Win32;

namespace FindPrograms
{
   // Table of reasons that WorkerThread calls into the
   // user-interface thread.
   public enum INVOKE_STARTUP
   {
      STARTUP_SUCCESS,
      STARTUP_FAILED,
      STATUS_MESSAGE
   }

   /// <summary>
   /// StartupThread - Wrapper class that spins a thread
   /// to initialize RAPI. Calls a delegate to report status.
   /// </summary>
   public class StartupThread
   {
      public string strBuffer;       // Inter-thread buffer
      public INVOKE_STARTUP itReason;   // Inter-thread reason

      private Thread m_thrd = null;    // The contained thread
      private Control m_ctlInvokeTarget; // Inter-thread control
      private EventHandler m_deleCallback; // Inter-thread delegate
      private bool m_bContinue; // Continue flag.

      public bool bThreadContinue // Continue property.
      {
         get { return m_bContinue; }
         set { m_bContinue = value; }
      }

      /// <summary>
      /// StartupThread - Constructor.
      /// </summary>
      /// <param name="ctl">Owner control</param>
      /// <param name="dele">Delegate to invoke</param>
      public StartupThread(Control ctl, EventHandler dele)
      {
         bThreadContinue = true;
         m_ctlInvokeTarget = ctl;  // Who to call.
         m_deleCallback = dele;    // How to call.
      }

      /// <summary>
      /// Run - Init function for startup thread.
      /// </summary>
      /// <returns></returns>
      public bool Run()
      {
         ThreadStart ts = null;
         ts = new ThreadStart(ThreadMainStartup);
         if (ts == null)
            return false;

         m_thrd = new Thread(ts);
         m_thrd.Start();
         return true;
      }

      /// <summary>
      /// ThreadMainStartup - Start RAPI connection.
      /// </summary>
      private void ThreadMainStartup()
      {
         // Allocate structure for call to CeRapiInitEx
         Rapi.RAPIINIT ri = new Rapi.RAPIINIT();
         ri.cbSize = Marshal.SizeOf(ri);

         // Call init function
         int hr = Rapi.CeRapiInitEx(ref ri);

         // Wrap event handle in corresponding .NET object
         ManualResetEvent mrev = new ManualResetEvent(false);
         mrev.Handle = ri.heRapiInit;

         // Wait five seconds, then fail.
         if (mrev.WaitOne(5000, false) && ri.hrRapiInit == Rapi.S_OK)
         {
            // Notify caller that connection established.
            itReason = INVOKE_STARTUP.STARTUP_SUCCESS;
            m_ctlInvokeTarget.Invoke(m_deleCallback);
         }
         else
         {
            // On failure, disconnect from RAPI.
            Rapi.CeRapiUninit();

            strBuffer = "Timeout - no device present.";
            itReason = INVOKE_STARTUP.STATUS_MESSAGE;
            m_ctlInvokeTarget.Invoke(m_deleCallback);

            // Notify caller that connection failed.
            itReason = INVOKE_STARTUP.STARTUP_FAILED;
            m_ctlInvokeTarget.Invoke(m_deleCallback);
         }
      }
   } // class StartupThread
} // namespace FindPrograms
