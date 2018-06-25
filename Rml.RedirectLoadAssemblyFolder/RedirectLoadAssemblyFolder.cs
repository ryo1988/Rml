using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Rml.RedirectLoadAssemblyFolder
{
    public class RedirectLoadAssemblyFolder
    {
        public static void Redirect(string folderPath)
        {
            AttachHandler(folderPath);
        }

        public static void RedirectExecutingAssemblyFolder()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var folderPath = Path.GetDirectoryName(assembly.Location);
            AttachHandler(folderPath);
        }

        private static void AttachHandler(string folderPath)
        {
            var files = Directory.GetFiles(folderPath, "*.dll", SearchOption.AllDirectories);
            var haveAssembly = new HashSet<string>(files);

            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
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