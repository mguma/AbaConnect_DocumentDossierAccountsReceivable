using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;

using Advice.MilaTest.AbaConnect.Debtors;

namespace Advice.MilaTest
{
    public class DebtorDocumentServiceOrchestrator : IDisposable
    {
        Advice.MilaTest.AbaConnect.Debtors.DocumentDossierAccountsReceivablePortClient client;
        string loginToken;

        public DebtorDocumentServiceOrchestrator()
        {
            var endpoint = new EndpointAddress(Config.AbaConnect_DebtorDocument_URL);

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

            client = new (binding, endpoint);
        }

        public void Ping()
        {
            var result = client.ping(new PingType { Echo = "Hello!" });
            Debug.WriteLine($"PING OK: {result.Echo}");
        }

        public void Login()
        {
            var loginResponse = client.login(new LoginType
            {
                Item = new UserLoginType
                {
                    Mandant = Config.Client,
                    UserName = Config.UserName,
                    Password = Config.Password
                }
            });

            loginToken = loginResponse.LoginToken;

            Debug.WriteLine($"Login: ok, Token: {loginToken}");
        }

        public void ReadData()
        {
            var documentNumber = 100_115_690;

            var response = client.find(new RequestType 
            {
                AbaConnectParam = new RequestParameterType { Login = new LoginType { Item = loginToken }, Additional = new ObjectDataType {  BooleanData = new[] { new BooleanDataType { Name = "ACGAPDossiersInclude", Value = true } } } },
                FindParam = new FindType { Operation = OperationType.EQUAL, Index = 1, IndexSpecified = true, KeyFields = new ObjectDataType { IntData = new[] { new IntDataType { Name = "Number", Value = documentNumber } } } }            
            });


            while (!response.ResponseMessage.IsFinished)
            {
                System.Threading.Thread.Sleep(250);


                response = client.isFinished(new RequestFinishedType { RequestID = response.ResponseMessage.RequestID });
            }

            PrintResponse(response);
        }

        private void PrintResponse(ResponseType response)
        {
            if (response.ResponseMessage != null)
            {
                Debug.WriteLine($"Response Message {response.ResponseMessage.RequestID}, Bookmark: {response.ResponseMessage.Bookmark}, IsFinished: {response.ResponseMessage.IsFinished}");

                if (response.ResponseMessage.Messages != null)
                {
                    var messages = response.ResponseMessage?.Messages?.Select(x => $"{x.Code} - {x.Level.ToString()}: {x.Text}");
                    var msg = string.Join(Environment.NewLine, messages);

                    Debug.WriteLine(msg);
                }

            }

            if (response.DataContainer == null)
            {
                Debug.WriteLine("No data found!");
                return;
            }

            if (response.DataContainer[0].Attachment == null)
            {
                Debug.WriteLine("No attachments found!");
            }
            else
            {
                foreach (var attachment in response.DataContainer[0].Attachment)
                {
                    Debug.WriteLine($"Attachment found {attachment.Name}, binary data :{attachment.BinData?.Value}");
                }
            }

            Debug.WriteLine($"Document data read successfull.");
        }

        public void Logout()
        {
            var logoutResponse = client.logout(new LoginType { Item = loginToken });
            Debug.WriteLine($"Logout: ok, Token: {logoutResponse.LoginToken}");


        }

        public void Dispose()
        {
            client.Close();   
        }
    }
}
