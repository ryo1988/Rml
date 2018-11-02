using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Rml.RedirectLoadAssemblyFolder
{
    public class RedirectLoadAssemblyFolder
    {
        public static void Redirect(string folderPath, AppDomain appDomain)
        {
            AttachHandler(folderPath, appDomain);
        }

        public static void Redirect(string folderPath)
        {
            AttachHandler(folderPath, AppDomain.CurrentDomain);
        }

        public static void RedirectExecutingAssemblyFolder(AppDomain appDomain)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var folderPath = Path.GetDirectoryName(assembly.Location);
            AttachHandler(folderPath, appDomain);
        }

        public static void RedirectExecutingAssemblyFolder()
        {
            RedirectExecutingAssemblyFolder(AppDomain.CurrentDomain);
        }

        private static void AttachHandler(string folderPath, AppDomain appDomain)
        {
            var files = Directory.GetFiles(folderPath, "*.dll", SearchOption.AllDirectories);
            var haveAssembly = new HashSet<string>(files);

            appDomain.AssemblyResolve += (s, e) =>
            {
                var requestingAssemblyFolderPath = Path.GetDirectoryName(e.RequestingAssembly.Location);
                if (folderPath != requestingAssemblyFolderPath)
                {
                    return null;
                }

                var assemblyName = new AssemblyName(e.Name);
                var path = Path.Combine(folderPath, assemblyName.Name + ".dll");
                if (haveAssembly.Contains(path) == false)
                {
                    return null;
                }

                try
                {
                    var assembly = Assembly.LoadFile(path);
                    return assembly;
                }
                catch
                {
                    // ignored
                }

                return null;
            };
        }
    }
}