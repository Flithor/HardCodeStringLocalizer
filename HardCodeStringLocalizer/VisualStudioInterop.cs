using EnvDTE80;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Setup.Configuration;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;

namespace ConsoleLauncherApp
{
    /// <summary>
    /// VisualStudio 互操作库
    /// </summary>
    public static class VisualStudioInterop
    {
        /// <summary>
        /// 在VisualStudio中打开指定文件
        /// </summary>
        public static EnvDTE.TextSelection OpenFileInVisualStudio(string fileName)
        {
            DTE2 dte = null;
            TryFor(1000, () => dte = GetDteByName("VisualStudio.DTE"));
            if (dte == null) throw new Exception("尚未启动Visual Studio");
            dte.MainWindow.Activate();
            TryFor(1000, () => dte.ItemOperations.OpenFile(fileName));
            EnvDTE.TextSelection textSelection = null;
            TryFor(1000, () =>
            {
                textSelection = (EnvDTE.TextSelection)dte.ActiveDocument.Selection;
            });
            return textSelection;
        }

        private static void TryFor(int ms, Action action)
        {
            DateTime timeout = DateTime.Now.AddMilliseconds(ms);
            bool success = false;

            do
            {
                try
                {
                    action();
                    success = true;
                }
                catch (Exception ex)
                {
                    if (DateTime.Now > timeout) throw ex;
                }
            } while (!success);
        }

        static DTE2 GetDteByName(string name)
        {
            IntPtr numFetched = Marshal.AllocHGlobal(sizeof(int));

            IRunningObjectTable runningObjectTable;
            IEnumMoniker monikerEnumerator;
            IMoniker[] monikers = new IMoniker[1];

            IBindCtx bindCtx;
            Marshal.ThrowExceptionForHR(CreateBindCtx(reserved: 0, ppbc: out bindCtx));

            bindCtx.GetRunningObjectTable(out runningObjectTable);

            runningObjectTable.EnumRunning(out monikerEnumerator);
            monikerEnumerator.Reset();

            while (monikerEnumerator.Next(1, monikers, numFetched) == 0)
            {
                IBindCtx ctx;
                CreateBindCtx(0, out ctx);

                string runningObjectName;
                monikers[0].GetDisplayName(ctx, null, out runningObjectName);

                if (runningObjectName.Contains(name))
                {
                    object runningObjectVal;
                    runningObjectTable.GetObject(monikers[0], out runningObjectVal);

                    DTE2 dte = (DTE2)runningObjectVal;
                    return (dte);
                }
            }
            return null;
        }

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);
    }
}