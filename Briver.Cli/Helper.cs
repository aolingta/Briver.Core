using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Briver.Extension;

namespace Briver
{
    internal static partial class Helper
    {

        [DllImport("shell32.dll", SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW(
            [MarshalAs(UnmanagedType.LPWStr)]string lpCmdLine,
            out int pNumArgs);

        public static string[] ParseCommandArgs(this string commandLine)
        {
            var argv = CommandLineToArgvW(commandLine, out int argc);
            if (argv == IntPtr.Zero)
            {
                throw new System.ComponentModel.Win32Exception();
            }

            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }

        }


    }
}
