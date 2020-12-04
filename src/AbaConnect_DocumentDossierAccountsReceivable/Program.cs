using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advice.MilaTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TestDebtorDocument();

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        static void TestDebtorDocument()
        {
            using var client = new DebtorDocumentServiceOrchestrator();
            try
            {
                client.Ping();

                client.Login();

                client.ReadData();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
                client.Logout();
            }


        }

        #region unused

        static void TestLoginLogout_Abacus_BestPractices()
        {
            using var client = new AddressServiceOrchestrator();

            Debug.WriteLine("Read session 1");
            client.Login();
            client.ReadData();
            client.Logout();

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));

            Debug.WriteLine("Read session 2");
            client.Login();
            client.ReadData();
            client.Logout();
        }

        static void TestLogin_ReadingData_TwoTimes_Without_Logout_InBetween()
        {
            using var client = new AddressServiceOrchestrator();

            Debug.WriteLine("Read session 1");
            client.Login();
            client.ReadData();


            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(2));

            Debug.WriteLine("Read session 2");
            client.Login();
            client.ReadData();
        }

        static void Ping()
        {
            using var client = new AddressServiceOrchestrator();
            client.Ping();
        }

        #endregion

    }
}
