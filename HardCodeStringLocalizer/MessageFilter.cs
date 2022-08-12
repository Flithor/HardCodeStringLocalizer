using System;
using System.Runtime.InteropServices;

namespace HardCodeStringLocalizer
{
    /// <summary>
    /// 解决因线程竞争而导致操作被拒绝的问题<br/>
    /// Code From: https://docs.microsoft.com/en-us/previous-versions/ms228772(v=vs.140)?redirectedfrom=MSDN
    /// </summary>
    internal class MessageFilter : MessageFilter.IOleMessageFilter
    {
        private class MessageFilterDisposer : IDisposable
        {
            public MessageFilterDisposer()
            {
                // Register the IOleMessageFilter to handle any threading errors.
                Register();
            }
            public void Dispose()
            {
                // and turn off the IOleMessageFilter.
                Revoke();
            }
        }

        [ComImport(), Guid("00000016-0000-0000-C000-000000000046"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleMessageFilter
        {
            [PreserveSig]
            int HandleInComingCall(
                int dwCallType,
                IntPtr hTaskCaller,
                int dwTickCount,
                IntPtr lpInterfaceInfo);

            [PreserveSig]
            int RetryRejectedCall(
                IntPtr hTaskCallee,
                int dwTickCount,
                int dwRejectType);

            [PreserveSig]
            int MessagePending(
                IntPtr hTaskCallee,
                int dwTickCount,
                int dwPendingType);
        }
        public static IDisposable Using()
        {
            return new MessageFilterDisposer();
        }

        //
        // Class containing the IOleMessageFilter
        // thread error-handling functions.

        // Start the filter.
        public static void Register()
        {
            IOleMessageFilter newFilter = new MessageFilter();
            CoRegisterMessageFilter(newFilter, out _);
        }

        // Done with the filter, close it.
        public static void Revoke()
        {
            CoRegisterMessageFilter(null, out _);
        }

        //
        // IOleMessageFilter functions.
        // Handle incoming thread requests.
        int IOleMessageFilter.HandleInComingCall(int dwCallType,
          System.IntPtr hTaskCaller, int dwTickCount, System.IntPtr
          lpInterfaceInfo)
        {
            //Return the flag SERVERCALL_ISHANDLED.
            return 0;
        }

        // Thread call was rejected, so try again.
        int IOleMessageFilter.RetryRejectedCall(System.IntPtr
          hTaskCallee, int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == 2)
            // flag = SERVERCALL_RETRYLATER.
            {
                // Retry the thread call immediately if return >=0 & 
                // <100.
                return 99;
            }
            // Too busy; cancel call.
            return -1;
        }

        int IOleMessageFilter.MessagePending(System.IntPtr hTaskCallee,
          int dwTickCount, int dwPendingType)
        {
            //Return the flag PENDINGMSG_WAITDEFPROCESS.
            return 2;
        }

        // Implement the IOleMessageFilter interface.
        [DllImport("Ole32.dll")]
        private static extern int
          CoRegisterMessageFilter(IOleMessageFilter newFilter, out
          IOleMessageFilter oldFilter);
    }
}
