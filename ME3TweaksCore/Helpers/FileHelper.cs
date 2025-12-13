using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace ME3TweaksCore.Helpers
{
    public static partial class FileHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BY_HANDLE_FILE_INFORMATION
        {
            public uint FileAttributes;
            public FILETIME CreationTime;
            public FILETIME LastAccessTime;
            public FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }

        #region From SO - Do not change to LibraryImport
        // Source - https://stackoverflow.com/a/37760332
        // Posted by jdphenix, modified by community. See post 'Timeline' for change history
        // Retrieved 2025-12-13, License - CC BY-SA 3.0

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetFileInformationByHandle(
            IntPtr hFile,
            out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        // Source - https://stackoverflow.com/a/37760332
        // Posted by jdphenix, modified by community. See post 'Timeline' for change history
        // Retrieved 2025-12-13, License - CC BY-SA 3.0

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] FileAccess access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
            IntPtr templateFile);

        // Source - https://stackoverflow.com/a/37760332
        // Posted by jdphenix, modified by community. See post 'Timeline' for change history
        // Retrieved 2025-12-13, License - CC BY-SA 3.0

        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        #endregion

        // Source - https://stackoverflow.com/a/37760332
        // Posted by jdphenix, modified by community. See post 'Timeline' for change history
        // Retrieved 2025-12-13, License - CC BY-SA 3.0

        public static bool IsOnSameVolume(string src, string dest)
        {
            IntPtr srcHandle = IntPtr.Zero, destHandle = IntPtr.Zero;
            bool ensureDeletion = false;

            try
            {
                if (!File.Exists(dest) && Directory.Exists(dest))
                {
                    ensureDeletion = true;
                    File.Create(dest).Dispose();
                }

                srcHandle = CreateFile(src, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero,
                    FileMode.Open, (FileAttributes)0x02000000, IntPtr.Zero);
                destHandle = CreateFile(dest, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero,
                    FileMode.Open, (FileAttributes)0x02000000, IntPtr.Zero);
                var srcInfo = new BY_HANDLE_FILE_INFORMATION();
                var destInfo = new BY_HANDLE_FILE_INFORMATION();

                if (!GetFileInformationByHandle(srcHandle, out srcInfo))
                {
                    throw new Exception(Marshal.GetLastWin32Error().ToString());
                }

                if (!GetFileInformationByHandle(destHandle, out destInfo))
                {
                    throw new Exception(Marshal.GetLastWin32Error().ToString());
                }

                return srcInfo.VolumeSerialNumber == destInfo.VolumeSerialNumber;
            }
            finally
            {
                if (srcHandle != IntPtr.Zero)
                {
                    CloseHandle(srcHandle);
                }

                if (destHandle != IntPtr.Zero)
                {
                    CloseHandle(destHandle);
                }

                if (ensureDeletion)
                {
                    try
                    {
                        File.Delete(dest);
                    }
                    catch
                    {
                        // Ignore exceptions during cleanup
                    }
                }
            }
        }
    }
}
