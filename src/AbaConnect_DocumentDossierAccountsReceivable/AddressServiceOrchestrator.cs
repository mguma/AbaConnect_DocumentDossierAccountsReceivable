using System;
using System.Diagnostics;
using System.ServiceModel;

namespace Advice.MilaTest
{
    public class AddressServiceOrchestrator : IDisposable
    {
        Advice.MilaTest.AbaConnect.addressPortClient client;
        string loginToken;

        public AddressServiceOrchestrator()
        {
            var endpoint = new EndpointAddress(Config.AbaConnect_Address_URL);

            BasicHttpBinding binding = new BasicHttpBinding();

                // Set up an own connection so we do not need the app.config settings to run program
                //            binding.Security.Mode = BasicHttpSecurityMode.None;
            binding.Security.Mode = BasicHttpSecurityMode.Transport;
            binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            binding.MaxReceivedMessageSize = Config.MaximumReceivedMessageSize;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            binding.ReaderQuotas.MaxStringContentLength = Config.MaxStringContentLength;
            binding.ReaderQuotas.MaxNameTableCharCount = 200000;

            // This configuration is required for some interfaces - most work with the .NET default settings, but not all
            binding.ReaderQuotas.MaxDepth = 56;
            binding.ReaderQuotas.MaxBytesPerRead = 4096;
            binding.ReaderQuotas.MaxArrayLength = 200000;  // Default is normally 16384;

            // Temp Info : testing from large size interfaces e.g ORDE
            // Ref: http://social.msdn.microsoft.com/Forums/en-US/wcf/thread/e1cd5de9-a1fd-4671-93d6-8d6eae9a89b6/
            binding.MaxBufferSize = (int)Config.MaximumReceivedMessageSize;

            client = new AbaConnect.addressPortClient(binding, endpoint);
        }

        public void Ping()
        {
            var result = client.ping(new AbaConnect.PingType { Echo = "Hello!" });
            Console.WriteLine($"PING OK: {result.Echo}");
        }

        public void Login()
        {
            var loginResponse = client.login(new AbaConnect.LoginType
            {
                Item = new AbaConnect.UserLoginType
                {
                    Mandant = Config.Client,
                    UserName = Config.UserName,
                    Password = Config.Password
                }
            });

            loginToken = loginResponse.LoginToken;

            Debug.WriteLine($"Login: {loginResponse.Message}, Token: {loginToken}");
        }

        public void ReadData()
        {
            Debug.WriteLine("Simulates reading data...");
        }

        public void Logout()
        {
            var logoutResponse = client.logout(new AbaConnect.LoginType { Item = loginToken });
            Debug.WriteLine($"Logout: ok, Token: {logoutResponse.LoginToken}");
        }

        public void Dispose()
        {
            client.Close();   
        }
    }
}
