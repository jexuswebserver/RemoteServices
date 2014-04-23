// Server class.
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
using System.IO;

namespace RemoteServices
{
    public static class JexusServer
    {
        public static SortedDictionary<string, List<string>> ServerVariables;
        public static string SiteFolder;
        public static string RootFolder = AppDomain.CurrentDomain.BaseDirectory;
        internal static string Credentials;

        static JexusServer()
        {
            var lines = File.ReadAllLines(Path.Combine(RootFolder, "jws.conf"));
            ServerVariables = new SortedDictionary<string, List<string>>();
            foreach (var line in lines)
            {
                var index = line.IndexOf('#');
                var content = index == -1 ? line : line.Substring(0, index);
                var parts = content.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    continue;
                }

                var key = parts[0].Trim().ToLowerInvariant();
                var value = parts[1].Trim();
                if (ServerVariables.ContainsKey(key))
                {
                    ServerVariables[key].Add(value);
                }
                else
                {
                    ServerVariables.Add(key, new List<string> { value });
                }
            }

            var folder = LoadVariable(ServerVariables, new List<string> { "siteconf" }, "siteconfigdir")[0];
            SiteFolder = folder.StartsWith("/")
                ? folder
                : Path.Combine(RootFolder, folder);
        }

        private static List<string> LoadVariable(SortedDictionary<string, List<string>> variables, List<string> defaultValue, params string[] names)
        {
            foreach (var name in names)
            {
                if (variables.ContainsKey(name))
                {
                    var result = variables[name];
                    return result;
                }
            }

            return defaultValue;
        }

        internal static void Save()
        {
            var lines = new List<string>();
            foreach (var item in ServerVariables)
            {
                foreach (var line in item.Value)
                {
                    lines.Add(string.Format("{0}={1}", item.Key, line));
                }
            }

            var root = AppDomain.CurrentDomain.BaseDirectory;
            File.WriteAllLines(Path.Combine(root, "jws.conf"), lines);
        }

        internal static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }
    }
}