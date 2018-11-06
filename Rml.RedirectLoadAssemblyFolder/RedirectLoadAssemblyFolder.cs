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

        private static Assembly LoadAssembly(HashSet<string> haveAssembly, string folderPath, AppDomain appDomain, string location, string name)
        {
            var requestingAssemblyFolderPath = Path.GetDirectoryName(location);
            if (folderPath != requestingAssemblyFolderPath && folderPath != appDomain.BaseDirectory)
            {
                return null;
            }

            var assemblyName = new AssemblyName(name);
            var path = Path.Combine(folderPath, assemblyName.Name + ".dll");
            if (haveAssembly.Contains(path) == false)
            {
                return null;
            }

            try
            {
                foreach(var assembly in appDomain.GetAssemblies())
                {
                    if (assembly.FullName == name)
                    {
                        return assembly;
                    }
                }
                {
                    var assembly = Assembly.LoadFile(path);
                    return assembly;
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

        private static void AttachHandler(string folderPath, AppDomain appDomain)
        {
            var files = Directory.GetFiles(folderPath, "*.dll", SearchOption.AllDirectories);
            var haveAssembly = new HashSet<string>(files);

            appDomain.AssemblyResolve += (s, e) => LoadAssembly(haveAssembly, folderPath, appDomain, e.RequestingAssembly?.Location ?? Assembly.GetCallingAssembly().Location, e.Name);
        }
    }
}