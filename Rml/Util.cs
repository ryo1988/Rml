using System;
using System.Diagnostics;
using System.IO;

namespace Rml
{
    /// <summary>
    /// 
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// .NetCore向けにBasePathを取得できるように
        /// </summary>
        /// <returns></returns>
        public static string GetBasePath()
        {
            var mainModuleFileName = Process.GetCurrentProcess().MainModule?.FileName;
            if (mainModuleFileName == @"c:\program files\dotnet\dotnet.exe")
            {
                return Directory.GetCurrentDirectory();
            }

            return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName ??
                                         throw new InvalidOperationException()) ??
                   throw new InvalidOperationException();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="fromPath"></param>
        /// <param name="toPath"></param>
        /// <param name="copySubDirectory"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static void CopyDirectory(string fromPath, string toPath, bool copySubDirectory)
        {
            var directoryInfo = new DirectoryInfo(fromPath);
            if (directoryInfo.Exists is false)
                throw new DirectoryNotFoundException(fromPath);

            Directory.CreateDirectory(toPath);

            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                var toFilePath = Path.Combine(toPath, fileInfo.Name);
                fileInfo.CopyTo(toFilePath);
            }

            if (copySubDirectory is false)
                return;

            foreach (var directory in directoryInfo.GetDirectories())
            {
                var toDirectoryPath = Path.Combine(toPath, directory.Name);
                CopyDirectory(directory.FullName, toDirectoryPath, copySubDirectory);
            }
        }
    }
}