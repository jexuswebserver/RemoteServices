/*
 * Created by SharpDevelop.
 * User: Lex
 * Date: 10/1/2014
 * Time: 11:22 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Net.Http;

namespace RemoteServicesHost
{
    /// <summary>
    /// Extension methods for <seealso cref="HttpRequestMessage" />.
    /// </summary>
    /// <remarks>http://www.strathweb.com/2013/01/adding-request-islocal-to-asp-net-web-api/</remarks>
    public static class HttpRequestMessageExtensions
    {
       public static bool IsLocal(this HttpRequestMessage request)
       {
          var localFlag = request.Properties["MS_IsLocal"] as Lazy<bool>;
          return localFlag != null && localFlag.Value;
       }
    }
}