/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Parsing;
using System;
using System.IO;
using System.Reflection;

namespace MihaZupan.MarkdownValidator.Configuration
{
    public static class ExtensionLoader
    {
        public static int Load(Config configuration, string assemblyPath, out string error)
        {
            error = null;
            try
            {
                string fullPath = Path.GetFullPath(assemblyPath);
                if (!File.Exists(fullPath))
                {
                    fullPath = Path.GetFullPath(assemblyPath, AppDomain.CurrentDomain.BaseDirectory);
                }

                if (Path.GetFileName(fullPath).Equals("MarkdownValidator.dll", StringComparison.OrdinalIgnoreCase))
                {
                    error = "Can't load the core assembly as an extension";
                    return -1;
                }

                Assembly assembly = Assembly.LoadFile(fullPath);

                int parserCount = 0;
                foreach (Type type in assembly.ExportedTypes)
                {
                    if (!type.IsClass || type.IsAbstract)
                        continue;

                    if (typeof(IParser).IsAssignableFrom(type))
                    {
                        parserCount++;
                        var parser = Activator.CreateInstance(type) as IParser;
                        configuration.Parsing.AddParser(parser);
                    }
                    else if (typeof(ICodeBlockParser).IsAssignableFrom(type))
                    {
                        parserCount++;
                        var parser = Activator.CreateInstance(type) as ICodeBlockParser;
                        configuration.Parsing.CodeBlocks.AddParser(parser);
                    }
                }
                return parserCount;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                return -1;
            }
        }
    }
}
