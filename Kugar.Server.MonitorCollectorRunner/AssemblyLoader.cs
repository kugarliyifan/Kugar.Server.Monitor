using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Server.MonitorCollectorRunner
{
    internal class AssemblyLoader
    {
        private string _basePath = "";
        private AssemblyLoadContext context;

        public AssemblyLoader(string basePath)
        {
            _basePath=basePath;
        }

        public Type[] Load(string dllFileName)
        {
            context = new AssemblyLoadContext(dllFileName);
            context.Resolving += Context_Resolving;

            var path = Path.Combine(_basePath, dllFileName);
            
            if (File.Exists(path))
            {
                try
                {
                    using var stream=File.OpenRead(path);

                    var assembly = context.LoadFromStream(stream);

                    var types=assembly.GetTypes();
                          
                    return types;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }


            return null;
        }

        private Assembly Context_Resolving(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            string expectedPath = Path.Combine(_basePath, assemblyName.Name + ".dll");

            if (File.Exists(expectedPath))
            {
                try
                {
                    using (var stream=File.OpenRead(expectedPath))
                    {
                        return context.LoadFromStream(stream);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else
            {
                Console.WriteLine("依赖文件不存在");
            }

            return null;
        }
    }

}
