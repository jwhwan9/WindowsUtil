using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace qSSLVersion
{
    class Program
    {        
        static void Main(string[] args)
        {            
            string host = "google.com";

            if (args.Length >= 1)
            {
                host = args[0];
            }

            string sslVersion = "";
            bool Ssl2Mark = false, Ssl3Mark = false, TlsMark = false, Tls11Mark = false, Tls12Mark = false;
            string Ssl2Info = "", Ssl3Info = "", TlsInfo = "", Tls11Info = "", Tls12Info = "";

            Ssl2Mark = IsSSLv2(host, out Ssl2Info);
            Ssl3Mark = IsSSLv3(host, out Ssl3Info);
            TlsMark = IsTLS(host, out TlsInfo);
            Tls11Mark = IsTLS11(host, out Tls11Info);
            Tls12Mark = IsTLS12(host, out Tls12Info);

            #region 將各版本 SSL 支援清單列出來
            if (Ssl2Mark) sslVersion += " ,SSLv2=" + Ssl2Info;
            if (Ssl3Mark) sslVersion += " ,SSLv3="+Ssl3Info;
            //if (TlsMark) sslVersion += " ,[TLS]=" + TlsInfo;
            if (Tls11Mark) sslVersion += " ,TLSv1.1="+Tls11Info;
            if (Tls12Mark) sslVersion += " ,TLSv1.2="+Tls12Info;
            #endregion

            Console.WriteLine(host + ":" + sslVersion);

            //Console.ReadKey();
        }

        #region 建立 SSL Socket Stream
        static SslStream createSSL(string host)
        {
            TcpClient client = new TcpClient(host, 443);
            int timeout = 3000;

            // Create an SSL stream that will close the client's stream.
            var ss = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate)
                );

            //var ss = new SslStream(poolItem.SecureConnection, false, remoteCertificateValidationCallback);

            ss.ReadTimeout = ss.WriteTimeout = timeout;
            return ss;
        }
        #endregion

        #region Check 是否支援 TLS v1.2
        static bool IsTLS12(string host, out string cryptInfo)
        {
            
            bool SupportMark = true;
            SslStream ss = createSSL(host);
            cryptInfo ="";

            try
            {
                ss.AuthenticateAsClient(host, new X509CertificateCollection(), SslProtocols.Tls12, false);
                cryptInfo = "[Cipher]:" + ss.CipherAlgorithm + "/" + ss.CipherStrength + " bits|[Hash]:" + ss.HashAlgorithm + "/" + ss.HashStrength + " bits";
                SupportMark = true;
            }
            catch (Exception ex)
            {
                SupportMark = false;
            }
            return SupportMark;
        }
        #endregion

        #region Check 是否支援 TLS v1.1
        static bool IsTLS11(string host, out string cryptInfo)
        {
            bool SupportMark = true;
            SslStream ss = createSSL(host);
            cryptInfo = "";

            try
            {
                ss.AuthenticateAsClient(host, new X509CertificateCollection(), SslProtocols.Tls11, false);
                cryptInfo = "[Cipher]:" + ss.CipherAlgorithm + "/" + ss.CipherStrength + " bits)|[Hash]:" + ss.HashAlgorithm + "/" + ss.HashStrength + " bits";

                SupportMark = true;
            }
            catch (Exception ex)
            {
                SupportMark = false;
            }
            return SupportMark;
        }
        #endregion

        #region Check 是否支援 TLS
        static bool IsTLS(string host, out string cryptInfo)
        {
            bool SupportMark = true;
            SslStream ss = createSSL(host);
            cryptInfo = "";

            try
            {
                ss.AuthenticateAsClient(host, new X509CertificateCollection(), SslProtocols.Tls, false);
                cryptInfo = "[Cipher]:" + ss.CipherAlgorithm + "/" + ss.CipherStrength + " bits)|[Hash]:" + ss.HashAlgorithm + "/" + ss.HashStrength + " bits";

                SupportMark = true;
            }
            catch (Exception ex)
            {
                SupportMark = false;
            }
            return SupportMark;
        }
        #endregion

        #region Check 是否支援 SSL v3
        static bool IsSSLv3(string host, out string cryptInfo)
        {
            bool SupportMark = true;
            SslStream ss = createSSL(host);
            cryptInfo = "";

            try
            {
                ss.AuthenticateAsClient(host, new X509CertificateCollection(), SslProtocols.Ssl3, false);
                cryptInfo = "[Cipher]:" + ss.CipherAlgorithm + "/" + ss.CipherStrength + " bits)|[Hash]:" + ss.HashAlgorithm + "/" + ss.HashStrength + " bits";

                SupportMark = true;
            }
            catch (Exception ex)
            {
                SupportMark = false;
            }
            return SupportMark;
        }
        #endregion

        #region Check 是否支援 SSL v2
        static bool IsSSLv2(string host, out string cryptInfo)
        {
            bool SupportMark = true;
            SslStream ss = createSSL(host);
            cryptInfo = "";

            try
            {
                ss.AuthenticateAsClient(host, new X509CertificateCollection(), SslProtocols.Ssl2, false);
                cryptInfo = "[Cipher]:" + ss.CipherAlgorithm + "/" + ss.CipherStrength + " bits)|[Hash]:" + ss.HashAlgorithm + "/" + ss.HashStrength + " bits";

                SupportMark = true;
            }
            catch (Exception ex)
            {
                SupportMark = false;
            }
            return SupportMark;
        }
        #endregion

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        static string ReadMessage(SslStream sslStream)
        {
            // Read the  message sent by the client.
            // The client signals the end of the message using the
            // "<EOF>" marker.
            byte[] buffer = new byte[2048];
            StringBuilder messageData = new StringBuilder();
            int bytes = -1;
            do
            {
                // Read the client's test message.
                bytes = sslStream.Read(buffer, 0, buffer.Length);

                // Use Decoder class to convert from bytes to UTF8
                // in case a character spans two buffers.
                Decoder decoder = Encoding.UTF8.GetDecoder();
                char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                // Check for EOF or an empty message.
                if (messageData.ToString().IndexOf("<EOF>") != -1)
                {
                    break;
                }
            } while (bytes != 0);

            return messageData.ToString();
        }
        static void DisplaySecurityLevel(SslStream stream)
        {
            Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            Console.WriteLine("Protocol: {0}", stream.SslProtocol);
        }
        static void DisplaySecurityServices(SslStream stream)
        {
            Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            Console.WriteLine("IsSigned: {0}", stream.IsSigned);
            Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
        }
        static void DisplayStreamProperties(SslStream stream)
        {
            Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
            Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
        }
        static void DisplayCertificateInformation(SslStream stream)
        {
            Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            X509Certificate localCertificate = stream.LocalCertificate;
            if (stream.LocalCertificate != null)
            {
                Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Local certificate is null.");
            }
            // Display the properties of the client's certificate.
            X509Certificate remoteCertificate = stream.RemoteCertificate;
            if (stream.RemoteCertificate != null)
            {
                Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                Console.WriteLine("Remote certificate is null.");
            }
        }
         
    }
}
