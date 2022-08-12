// RapiConnectDetect.cs - Creates an RAPI COM
// object of type IDccMan and an Advise Sink of type IDccManSink
// to detect the start and end of RAPI connections.
//
// Code from _Programming the .NET Compact Framework with C#_
// and _Programming the .NET Compact Framework with VB_
// (c) Copyright 2002-2004 Paul Yao and David Durant. 
// All rights reserved.

using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace FindPrograms
{
   // Reason we are calling.
   public enum INVOKE_CONNECT
   {
      START,
      STOP,
   }

   /// <summary>
   /// Provides notifications of RAPI connection
   /// </summary>
   public class RapiConnectDetect : IDccManSink
   {
      public INVOKE_CONNECT itReason;   // Inter-thread reason

      private EventHandler m_deleCallback; // Callback delegate
      private Control m_ctlTarget; // Target for Invoke calls

      private IDccMan m_pDccMan = null;  // RAPI-provided object
      private bool m_Connected = false;   // Connection state
      private int m_iAdviseSinkContext = 0; // Advise sink context

      [DllImport("Ole32.dll"), PreserveSig]
      private static extern int 
         CoCreateInstance(
         ref Guid clsid,
         int Res, // punkOuter - set to zero
         int context,
         ref Guid iid,
         ref IDccMan pDccMan);

      public const int CLSCTX_INPROC_SERVER = 1;

      /// <summary>
      /// RapiConnectDetect -- class constructor
      /// </summary>
      /// <param name="dele"></param>
      public RapiConnectDetect(Control ctl, EventHandler dele)
      {
         m_deleCallback = dele; // Callback for notifications
         m_ctlTarget = ctl;     // Control to notify.
      }
      
      public bool Init()
      {
         System.Guid IID_IDccMan = new
            System.Guid("A7B88841-A812-11CF-8011-00A0C90A8F78");
         System.Guid CLSID_DccMan = new
            System.Guid("499C0C20-A766-11CF-8011-00A0C90A8F78");

         int iErr = CoCreateInstance(ref CLSID_DccMan, 0,
            CLSCTX_INPROC_SERVER, ref IID_IDccMan, ref m_pDccMan);

         if (m_pDccMan == null)
            return false;

         return true;
      }

      /// <summary>
      /// Enable - Toggle advise synch operation.
      /// </summary>
      /// <param name="bEnable"></param>
      public void Enable(bool bEnable)
      {
         if(bEnable && m_iAdviseSinkContext == 0)
         {
            IDccManSink idcc = (IDccManSink)this;
            m_pDccMan.Advise(idcc, ref m_iAdviseSinkContext);
         }
         
         if (!bEnable && m_iAdviseSinkContext != 0)
         {
            m_pDccMan.Unadvise(m_iAdviseSinkContext);
            m_iAdviseSinkContext = 0;
         }
      }

      //
      // IDccManSink interface functions.
      //

      public int OnLogAnswered()  { return 0; } // Line detected
      public int OnLogActive()    { return 0; } // Line active

      /// <summary>
      /// OnLogIpAddr - First event when it makes sense to
      /// try to do any real work with target device.
      /// </summary>
      /// <param name="dwIpAddr"></param>
      /// <returns></returns>
      public int OnLogIpAddr(int dwIpAddr) // Link established.
      { 
         if (!m_Connected) // Notify only if not connected.
         {
            this.itReason = INVOKE_CONNECT.START;
            this.m_ctlTarget.Invoke(this.m_deleCallback);
            m_Connected = true;
         }
         return 0;
      }


      /// <summary>
      /// OnLogDisconnection - Connection ended.
      /// </summary>
      /// <returns></returns>
      public int OnLogDisconnection()  
      {
         if (m_Connected) // Notify only if connected.
         {
            this.itReason = INVOKE_CONNECT.STOP;
            this.m_ctlTarget.Invoke(this.m_deleCallback);
            m_Connected = false;
         }

         return 0; 
      }

      public int OnLogListen()      { return 0; }
      public int OnLogTerminated()  { return 0; }
      public int OnLogInactive()    { return 0; }
      public int OnLogError()  { return 0; }

   } // class RapiConnectDetect


   #region IDccMan Definitions   
   /// <summary>
   /// IDccMan - interface of COM component provided by RAPI.
   /// </summary>
   [Guid("a7b88841-a812-11cf-8011-00a0c90a8f78"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] 
   interface IDccMan   // RAPI notification interface
   {
      void Advise (IDccManSink pDccSink, ref int pdwContext);
      void Unadvise(int dwContext);
      void ShowCommSettings ();
      void AutoconnectEnable ();
      void AutoconnectDisable ();
      void ConnectNow ();
      void DisconnectNow ();
      void SetIconDataTransferring ();
      void SetIconNoDataTransferring ();
      void SetIconError ();
   }

   /// <summary>
   /// IDccManSink - interface we implement to grab notifications
   /// </summary>
   [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
   interface IDccManSink
   {
      [PreserveSig] int OnLogIpAddr (int dwIpAddr);
      [PreserveSig] int OnLogTerminated ();
      [PreserveSig] int OnLogActive ();
      [PreserveSig] int OnLogInactive ();
      [PreserveSig] int OnLogAnswered ();
      [PreserveSig] int OnLogListen ();
      [PreserveSig] int OnLogDisconnection ();
      [PreserveSig] int OnLogError ();
   }

#endregion

} // namespace FindPrograms
