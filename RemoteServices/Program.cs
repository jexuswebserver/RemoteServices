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
using Mono.Security.Authenticode;
using Mono.Security.X509;
using Mono.Security.X509.Extensions;
using Mono.Unix.Native;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using RemObjects.Mono.Helpers;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

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
            Console.WriteLine("More information can be found at https://Jexus.codeplex.com");
            Console.WriteLine();

            var baseAddress = args.Length > 0 ? args[0] : "https://localhost:8088";
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

                var loc = baseAddress.LastIndexOf(':');
                var port = "443";
                if (loc != -1)
                {
                    port = baseAddress.Substring(loc + 1);
                }

                string dirname = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string path = Path.Combine(dirname, ".mono", "httplistener");
                if (false == Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string target_cert = Path.Combine(path, string.Format("{0}.cer", port));
                if (File.Exists(target_cert))
                {
                    Console.WriteLine("Use {0}", target_cert);
                }
                else
                {
                    Console.WriteLine("Generating a self-signed certificate for Jexus Manager");

                    // Generate certificate
                    string defaultIssuer = "CN=jexus.lextudio.com";
                    string defaultSubject = "CN=jexus.lextudio.com";
                    byte[] sn = Guid.NewGuid().ToByteArray();
                    string subject = defaultSubject;
                    string issuer = defaultIssuer;
                    DateTime notBefore = DateTime.Now;
                    DateTime notAfter = new DateTime(643445675990000000); // 12/31/2039 23:59:59Z

                    RSA issuerKey = new RSACryptoServiceProvider(2048);
                    RSA subjectKey = null;

                    bool selfSigned = true;
                    string hashName = "SHA1";

                    CspParameters subjectParams = new CspParameters();
                    CspParameters issuerParams = new CspParameters();
                    BasicConstraintsExtension bce = new BasicConstraintsExtension
                    {
                        PathLenConstraint = BasicConstraintsExtension.NoPathLengthConstraint,
                        CertificateAuthority = true
                    };
                    ExtendedKeyUsageExtension eku = null;
                    SubjectAltNameExtension alt = null;
                    string p12file = Path.Combine(path, "temp.pfx");
                    string p12pwd = "test";

                    // serial number MUST be positive
                    if ((sn[0] & 0x80) == 0x80)
                        sn[0] -= 0x80;

                    if (selfSigned)
                    {
                        if (subject != defaultSubject)
                        {
                            issuer = subject;
                            issuerKey = subjectKey;
                        }
                        else
                        {
                            subject = issuer;
                            subjectKey = issuerKey;
                        }
                    }

                    if (subject == null)
                        throw new Exception("Missing Subject Name");

                    X509CertificateBuilder cb = new X509CertificateBuilder(3);
                    cb.SerialNumber = sn;
                    cb.IssuerName = issuer;
                    cb.NotBefore = notBefore;
                    cb.NotAfter = notAfter;
                    cb.SubjectName = subject;
                    cb.SubjectPublicKey = subjectKey;
                    // extensions
                    if (bce != null)
                        cb.Extensions.Add(bce);
                    if (eku != null)
                        cb.Extensions.Add(eku);
                    if (alt != null)
                        cb.Extensions.Add(alt);

                    IDigest digest = new Sha1Digest();
                    byte[] resBuf = new byte[digest.GetDigestSize()];
                    var spki = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(DotNetUtilities.GetRsaPublicKey(issuerKey));                    
                    byte[] bytes = spki.PublicKeyData.GetBytes();
                    digest.BlockUpdate(bytes, 0, bytes.Length);
                    digest.DoFinal(resBuf, 0);

                    cb.Extensions.Add(new SubjectKeyIdentifierExtension { Identifier = resBuf });
                    cb.Extensions.Add(new AuthorityKeyIdentifierExtension { Identifier = resBuf });
                    // signature
                    cb.Hash = hashName;
                    byte[] rawcert = cb.Sign(issuerKey);

                    PKCS12 p12 = new PKCS12();
                    p12.Password = p12pwd;

                    ArrayList list = new ArrayList();
                    // we use a fixed array to avoid endianess issues 
                    // (in case some tools requires the ID to be 1).
                    list.Add(new byte[4] { 1, 0, 0, 0 });
                    Hashtable attributes = new Hashtable(1);
                    attributes.Add(PKCS9.localKeyId, list);

                    p12.AddCertificate(new Mono.Security.X509.X509Certificate(rawcert), attributes);
                    p12.AddPkcs8ShroudedKeyBag(subjectKey, attributes);
                    p12.SaveToFile(p12file);

                    var x509 = new System.Security.Cryptography.X509Certificates.X509Certificate2(p12file, p12pwd, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.Exportable);

                    // Install certificate
                    string target_pvk = Path.Combine(path, string.Format("{0}.pvk", port));

                    using (Stream cer = File.OpenWrite(target_cert))
                    {
                        byte[] raw = x509.RawData;
                        cer.Write(raw, 0, raw.Length);
                    }

                    PrivateKey pvk = new PrivateKey();
                    pvk.RSA = subjectKey;
                    pvk.Save(target_pvk);
                }
            }

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
