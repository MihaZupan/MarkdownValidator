/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Parsing;
using MihaZupan.MarkdownValidator.Parsing.ExternalUrls;
using System;
using System.IO;
using System.Reflection;

namespace MihaZupan.MarkdownValidator.Configuration
{
    public static class ExtensionLoader
    {
        public static bool TryLoad(Config configuration, string assemblyPath)
        {
            try
            {
                string fullPath = Path.GetFullPath(assemblyPath);
                if (!File.Exists(fullPath))
                {
                    fullPath = Path.GetFullPath(assemblyPath, AppDomain.CurrentDomain.BaseDirectory);
                }

                if (fullPath.EndsWith("MarkdownValidator.dll", StringComparison.OrdinalIgnoreCase))
                    return false;

                Assembly assembly = Assembly.LoadFile(fullPath);

                foreach (Type type in assembly.ExportedTypes)
                {
                    if (!type.IsClass || type.IsAbstract)
                        continue;

                    if (typeof(IParser).IsAssignableFrom(type))
                    {
                        var parser = Activator.CreateInstance(type) as IParser;
                        configuration.Parsing.AddParser(parser);
                    }
                    if (typeof(ICodeBlockParser).IsAssignableFrom(type))
                    {
                        var parser = Activator.CreateInstance(type) as ICodeBlockParser;
                        configuration.Parsing.CodeBlocks.AddParser(parser);
                    }
                    if (typeof(IUrlPostProcessor).IsAssignableFrom(type))
                    {
                        var processor = Activator.CreateInstance(type) as IUrlPostProcessor;
                        configuration.WebIO.UrlProcessor.AddUrlPostProcessor(processor);
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
