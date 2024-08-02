using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        /// <param name="overwrite"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static void CopyDirectory(string fromPath, string toPath, bool copySubDirectory, bool overwrite)
        {
            var directoryInfo = new DirectoryInfo(fromPath);
            if (directoryInfo.Exists is false)
                throw new DirectoryNotFoundException(fromPath);

            if (Directory.Exists(toPath) is false)
            {
                Directory.CreateDirectory(toPath);
            }

            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                var toFilePath = Path.Combine(toPath, fileInfo.Name);
                if (overwrite || File.Exists(toFilePath) is false)
                    fileInfo.CopyTo(toFilePath, overwrite);
            }

            if (copySubDirectory is false)
                return;

            foreach (var directory in directoryInfo.GetDirectories())
            {
                var toDirectoryPath = Path.Combine(toPath, directory.Name);
                CopyDirectory(directory.FullName, toDirectoryPath, copySubDirectory, overwrite);
            }
        }
        
        public static async Task CopyDirectoryAsync(string fromPath, string toPath, bool copySubDirectory, bool overwrite)
        {
            var directoryInfo = new DirectoryInfo(fromPath);
            if (directoryInfo.Exists is false)
                throw new DirectoryNotFoundException(fromPath);

            if (Directory.Exists(toPath) is false)
            {
                Directory.CreateDirectory(toPath);
            }
            
            foreach (var fileInfo in directoryInfo.GetFiles())
            {
                var toFilePath = Path.Combine(toPath, fileInfo.Name);
                if (overwrite || File.Exists(toFilePath) is false)
                {
                    await CopyFile(fileInfo.FullName, toFilePath);
                }
            }
            
            if (copySubDirectory is false)
                return;
            
            foreach (var directory in directoryInfo.GetDirectories())
            {
                var toDirectoryPath = Path.Combine(toPath, directory.Name);
                await CopyDirectoryAsync(directory.FullName, toDirectoryPath, copySubDirectory, overwrite);
            }
            
            return;

            static async Task CopyFile(string fromPath, string toPath)
            {
                using var fromStream = new FileStream(fromPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, true);
                using var toStream = new FileStream(toPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                await fromStream.CopyToAsync(toStream);
            }
        }

        public static int GetIndex<T>(IEnumerable<T> from, Func<T, bool> equalKey)
        {
            return from
                .Select((o, i) => (value: o, index: i))
                .Single(o => equalKey(o.value)).index;
        }
    }
}