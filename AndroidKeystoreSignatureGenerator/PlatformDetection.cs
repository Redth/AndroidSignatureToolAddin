using System;
using System.IO;

namespace AndroidKeystoreSignatureGenerator
{
	public static class PlatformDetection
	{
		public static bool IsMac
        {
            get
            {
                if (IsWindows)
                    return false;

                return MacDetector.IsRunningOnMac();
            }
        }

        public static bool IsLinux
        {
            get
            {
                if (IsWindows)
                    return false;

                return !IsMac;
            }
        }

		public static bool IsWindows
        {
            get
            {
                var p = Environment.OSVersion.Platform;

                return p != PlatformID.Unix && p != PlatformID.MacOSX;
            }
        }
	}

    static class MacDetector
    {
        static MacDetector()
        {
            System.Diagnostics.Debug.WriteLine("MacDetector ctor");

        }

        public static bool IsRunningOnMac()
        {
            IntPtr buf = IntPtr.Zero;
            try
            {
                buf = System.Runtime.InteropServices.Marshal.AllocHGlobal(8192);
                // This is a hacktastic way of getting sysname from uname ()
                if (uname(buf) == 0)
                {
                    string os = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(buf);
                    if (os == "Darwin")
                        return true;
                }
            }
            catch
            {
            }
            finally
            {
                if (buf != IntPtr.Zero)
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(buf);
            }
            return false;
        }

        [System.Runtime.InteropServices.DllImport("libc")]
        static extern int uname(IntPtr buf);
    }
}

