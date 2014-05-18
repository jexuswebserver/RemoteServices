// Entry.
// Copyright (C) 2014  Lex Li
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using Microsoft.Owin.Hosting;
using Mono.Unix.Native;
using RemObjects.Mono.Helpers;
using System;
using System.IO;
using System.Reflection;

namespace RemoteServicesHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var title = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute));
            Console.WriteLine("{0} version {1}", title.Title, assembly.GetName().Version);
            var copyright = (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute));
            Console.WriteLine(copyright.Copyright);
			Console.WriteLine("More information can be found at http://Jexus.codeplex.com");
            Console.WriteLine();

            if (JexusServer.IsRunningOnMono() && PlatformSupport.Platform != PlatformType.Windows)
            {
                if (Syscall.getuid() != 0)
                {
                    Console.WriteLine(@"Remote services must be run as root on Linux.");
                    return;
                }

                if (!File.Exists("jws"))
                {
                    Console.WriteLine(@"Remote services must be running in Jexus installation folder.");
                    return;
                }
            }

            var baseAddress = args.Length > 0 ? args[0] : "https://localhost:8088";
            JexusServer.Credentials = args.Length > 2 ? args[1] + "|" + args[2] : "jexus|lextudio.com";
            JexusServer.Timeout = args.Length > 3 ? double.Parse(args[3]) : 30D;

            using (WebApp.Start<Startup>(url: baseAddress))
            {
                Console.WriteLine("Remote services have started at {0}.", baseAddress);
                Console.WriteLine("Credentials is {0}", JexusServer.Credentials);
                Console.WriteLine("Press Enter to quit.");
                Console.ReadLine();
            }
        }
    }
}
