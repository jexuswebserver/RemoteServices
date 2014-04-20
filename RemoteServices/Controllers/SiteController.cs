// Site controller.
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

using RemObjects.Mono.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Http;

namespace RemoteServices.Controllers
{
    [RoutePrefix("api/site")]
    public class SiteController : ApiController
    {
        [Route("")]
        public IEnumerable<string> Get()
        {
            var folder = new DirectoryInfo(JexusServer.SiteFolder);
            var result = new List<string>();
            var siteFiles = folder.GetFiles().Select(file => file.Name);
            foreach (var site in siteFiles)
            {
                var settings = Get(site);
                if (settings == null)
                {
                    continue;
                }

                result.Add(site);
            }

            return result;
        }

        [Route("{name}")]
        [HttpGet]
        public IDictionary<string, List<string>> Get(string name)
        {
            var rows = File.ReadAllLines(Path.Combine(JexusServer.SiteFolder, name));
            var result = new SortedDictionary<string, List<string>>();
            foreach (var line in rows)
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
                if (result.ContainsKey(key))
                {
                    result[key].Add(value);
                    continue;
                }

                result.Add(key, new List<string> { value });
            }

            if (result["root"][0].StartsWith("/ "))
            {
                return result;
            }

            return null;
        }

        [Route("{name}")]
        [HttpPut]
        public void Put(string name, SortedDictionary<string, List<string>> variables)
        {
            var rows = new List<string>();
            foreach (var item in variables)
            {
                foreach (var line in item.Value)
                {
                    rows.Add(string.Format("{0}={1}", item.Key, line));
                }
            }

            var fileName = Path.Combine(JexusServer.SiteFolder, name);
            File.WriteAllLines(fileName, rows);
        }

        [Route("{name}")]
        [HttpDelete]
        public void Delete(string name)
        {
            var fileName = Path.Combine(JexusServer.SiteFolder, name);
            File.Delete(fileName);
        }

        [Route("start/{name}")]
        [HttpGet]
        public bool GetStart(string name)
        {
            var onLinux = JexusServer.IsRunningOnMono() && PlatformSupport.Platform != PlatformType.Windows;
            var process = Process.Start(onLinux ? "jws" : "jws.exe", string.Format("start {0}", name));
            process.WaitForExit();
            return true;
        }

        [Route("stop/{name}")]
        [HttpGet]
        public bool GetStop(string name)
        {
            var onLinux = JexusServer.IsRunningOnMono() && PlatformSupport.Platform != PlatformType.Windows;
            var process = Process.Start(onLinux ? "jws" : "jws.exe", string.Format("stop {0}", name));
            process.WaitForExit();
            return true;
        }

        [Route("state/{name}")]
        [HttpGet]
        public bool GetState(string name)
        {
            var onLinux = JexusServer.IsRunningOnMono() && PlatformSupport.Platform != PlatformType.Windows;
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = onLinux ? "jws" : "jws.exe",
                    Arguments = string.Format("status {0}", name),
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            process.WaitForExit();
            var content = process.StandardOutput.ReadToEnd();
            return content.Contains("running");
        }

        [Route("list/{name}")]
        [HttpGet]
        public IEnumerable<string> GetFiles(string name)
        {
            var variables = Get(name);
            var root = variables["root"][0];
            var split = root.IndexOf(' ');
            if (split == -1 || split == 0)
            {
                return null;
            }

            var path = root.Substring(split + 1);
            var winPath = Path.Combine(JexusServer.RootFolder, path.Replace('/', '\\').TrimStart('\\'));
            var result = Directory.GetDirectories(JexusServer.IsRunningOnMono() ? path : winPath, "*", SearchOption.AllDirectories)
                .Select(folder => JexusServer.IsRunningOnMono() ? folder.Substring(path.Length) : folder.Substring(winPath.Length).Replace('\\', '/'));
            return result;
        }

        [Route("verify")]
        [HttpPost]
        public bool VerifyPath([FromBody] string path)
        {
            return Directory.Exists(path);
        }
    }
}
