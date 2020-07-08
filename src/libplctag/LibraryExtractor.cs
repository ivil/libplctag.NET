﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace libplctag.NativeImport
{
    class LibraryExtractor
    {

        public static void Init(string extractDirectory = null)
        {

            if (extractDirectory == null)
            {
                extractDirectory = GetExecutingAssemblyDirectory();
            }

            if (!LibraryExists(extractDirectory))
            {
                ExtractAppropriateLibrary(extractDirectory);
            }

        }

        public static string GetExecutingAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        static void ExtractAppropriateLibrary(string folder)
        {
            var embeddedResourceName = GetResourceName();
            var embeddedResourceFileName = embeddedResourceName.Item2;

            var embeddedResource = GetEmbeddedResource(embeddedResourceName.Item1 + "." + embeddedResourceName.Item2);
            var newFileName = Path.Combine(folder, embeddedResourceFileName);

            File.WriteAllBytes(newFileName, embeddedResource);
        }

        public static bool LibraryExists(string folder)
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return File.Exists(Path.Combine(folder, "plctag.dll"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return File.Exists(Path.Combine(folder, "libplctag.so"));
            }
            else
            {
                throw new TypeLoadException("Platform not supported");
            }
        }

        static Tuple<string, string> GetResourceName()
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.X86)
            {
                return new Tuple<string, string>("libplctag.runtime.win_x86", "plctag.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                return new Tuple<string, string>("libplctag.runtime.win_x64", "plctag.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.ProcessArchitecture == Architecture.X86)
            {
                return new Tuple<string, string>("libplctag.runtime.linux_86", "libplctag.so");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && RuntimeInformation.ProcessArchitecture == Architecture.X64)
            {
                return new Tuple<string, string>("libplctag.runtime.linux_x64", "libplctag.so");
            }
            else
            {
                throw new TypeLoadException("This platform is not supported, could not load appropriate unmanaged library");
            }

        }

        static byte[] GetEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return null;
                byte[] resource = new byte[stream.Length];
                stream.Read(resource, 0, resource.Length);
                return resource;
            }
        }

    }
}
