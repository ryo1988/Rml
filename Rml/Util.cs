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
    }
}