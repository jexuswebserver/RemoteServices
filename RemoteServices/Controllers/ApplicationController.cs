// Application controller.
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

using RemoteServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Http;

namespace RemoteServicesHost.Controllers
{
    [RoutePrefix("api/app")]
    public class ApplicationController : ApiController
    {
        [Route("{name}")]
        [HttpGet]
        public IEnumerable<string> Get(string name)
        {
            var folder = new DirectoryInfo(JexusServer.SiteFolder);
            var result = new List<string>();
            var siteFiles = folder.GetFiles().Select(file => file.Name).Where(file => file.StartsWith(string.Format("{0}_", name)));
            foreach (var site in siteFiles)
            {
                var settings = GetApplication(site);
                if (settings == null)
                {
                    continue;
                }

                result.Add(site);
            }

            return result;
        }

        [Route("get/{name}")]
        public IDictionary<string, List<string>> GetApplication(string name)
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

            string appName = ToPath(name);
            if (string.IsNullOrEmpty(appName))
            {
                return null;
            }

            if (!result["root"][0].StartsWith(string.Format("{0}/ ", appName)))
            {
                return null;
            }

            return result;
        }

        private static string ToPath(string name)
        {
            var appName = new StringBuilder();
            var sections = name.Split('_');
            for (int i = 1; i < sections.Length; i++)
            {
                appName.AppendFormat("/{0}", sections[i]);
            }

            return appName.ToString();
        }
    }
}