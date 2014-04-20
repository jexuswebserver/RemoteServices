// Jexus Windows simulator.
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace jws
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("wrong");
                return;
            }

            if (args[0] == "stop")
            {
                if (args.Length == 1)
                {
                    if (!File.Exists(".running"))
                    {
                        return;
                    }

                    Console.Write("Stopping ... ");
                    Thread.Sleep(10000);
                    File.Delete(".running");
                    Console.Write("OK.");
                    return;
                }

                var site = args[1];
                if (File.Exists(site))
                {
                    Console.Write("Stop site:{0} ... ", site);
                    Thread.Sleep(10000);
                    File.Delete(site);
                    Console.Write("OK.");
                    return;
                }

                return;
            }

            if (args[0] == "start")
            {
                if (args.Length == 1)
                {
                    if (File.Exists(".running"))
                    {
                        return;
                    }

                    Console.Write("Starting ... ");
                    Thread.Sleep(10000);
                    File.WriteAllText(".running", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
                    Console.Write("OK.");
                    return;
                }

                var site = args[1];
                if (!File.Exists(site))
                {
                    Console.Write("Start site:{0} ... ", site);
                    Thread.Sleep(10000);
                    File.WriteAllText(site, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
                    Console.Write("OK.");
                    return;
                }

                return;
            }

            if (args[0] == "restart")
            {
                if (args.Length == 1)
                {
                    Console.Write("Starting ... ");
                    Thread.Sleep(10000);
                    File.WriteAllText(".running", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
                    Console.Write("OK.");
                    return;
                }

                var site = args[1];
                Console.Write("Start site:{0} ... ", site);
                Thread.Sleep(10000);
                File.WriteAllText(site, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
                Console.Write("OK.");
                return;
            }

            if (args[0] == "status")
            {
                if (args.Length == 1)
                {
                    if (File.Exists(".running"))
                    {
                        Console.Write("Jexus is running.");
                        return;
                    }

                    Console.Write("Jexus has stopped.");
                    return;
                }

                var site = args[1];
                if (File.Exists(site))
                {
                    Console.Write("site:{0} is running.", site);
                    return;
                }

                Console.Write("site:{0} has stopped.", site);
                return;
            }

            Console.WriteLine("wrong");
            return;
        }
    }
}