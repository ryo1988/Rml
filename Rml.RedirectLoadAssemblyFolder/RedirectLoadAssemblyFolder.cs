using System;
using System.IO;
using System.Reflection;

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
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                var info = e.Name.Split(',');
                var path = Path.Combine(folderPath, info[0] + ".dll");

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