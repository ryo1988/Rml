using System;
using System.Diagnostics;
using System.IO;

namespace Rml
{
    public static class Util
    {
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