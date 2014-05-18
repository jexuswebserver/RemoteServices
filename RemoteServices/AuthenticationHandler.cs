// Authentication handler.
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
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteServicesHost
{
    public class AuthenticationHandler : DelegatingHandler
    {
        const string _header = "X-HTTP-Authorization";
        const string _time = "Time";

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.Contains(_header))
            {
                var credentials = request.Headers.GetValues(_header).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(credentials) && credentials == JexusServer.Credentials)
                {
                    return base.SendAsync(request, cancellationToken);
                }

                var time = request.Headers.GetValues(_time).FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(time))
                {
                    var data = DateTime.Parse(time, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                    var span = DateTime.UtcNow - data;
                    if (span.TotalSeconds > 0 && span.TotalSeconds <= JexusServer.Timeout)
                    {
                        return base.SendAsync(request, cancellationToken);
                    }
                }
            }

            // Create the response.
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("You are not authorized for this server.")
            };

            // Note: TaskCompletionSource creates a task that does not contain a delegate.
            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);   // Also sets the task state to "RanToCompletion"
            return tsc.Task;
        }
    }
}