using System.Data.SqlClient;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Polly;

namespace SqlServerHealthModule
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Loader;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Client;
    using Microsoft.Azure.Devices.Client.Transport.Mqtt;
    
    class Program
    {
        static void Main(string[] args)
        {
            Init().Wait();

            // Wait until the app unloads or is cancelled
            var cts = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += (ctx) => cts.Cancel();
            Console.CancelKeyPress += (sender, cpe) => cts.Cancel();
            WhenCancelled(cts.Token).Wait();
        }

        /// <summary>
        /// Handles cleanup operations when app is cancelled or unloads
        /// </summary>
        public static Task WhenCancelled(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).SetResult(true), tcs);
            return tcs.Task;
        }

        /// <summary>
        /// Initializes the ModuleClient and sets up the callback to receive
        /// messages containing temperature information
        /// </summary>
        static async Task Init()
        {
            MqttTransportSettings mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            ITransportSettings[] settings = { mqttSetting };

            // Open a connection to the Edge runtime
            ModuleClient ioTHubModuleClient = await ModuleClient.CreateFromEnvironmentAsync(settings);
            await ioTHubModuleClient.OpenAsync();
            Console.WriteLine("IoT Hub module client initialized.");

            await SendSqlServerHealthDataAsync(
                ioTHubModuleClient,
                new SqlHealthService("Server=sqlserver;Database=master;User Id=sa;Password=9@uD4kH@;"),
                TimeSpan.FromSeconds(5),
                new MessageFactory());
        }

        private static async Task SendSqlServerHealthDataAsync(
            ModuleClient ioTHubModuleClient, 
            SqlHealthService sqlHealthService,
            TimeSpan interval, 
            MessageFactory messageFactory)
        {
            while (true)
            {
                try
                {
                    await sqlHealthService.DoHealthCheckAsync();
                    var message = messageFactory.CreateMessage(HealthStatus.Healthy);
                    await ioTHubModuleClient.SendEventAsync("sqlServerHealthCheckOutput", message);
                    Console.WriteLine($"\t{DateTime.UtcNow.ToLongTimeString()}> SQL Server is healthy.");
                    await Task.Delay(interval);
                }
                catch (Exception)
                {
                    var message = messageFactory.CreateMessage(HealthStatus.Unhealthy);
                    await ioTHubModuleClient.SendEventAsync("sqlServerHealthCheckOutput", message);
                    Console.WriteLine($"\t{DateTime.UtcNow.ToLongTimeString()}> SQL Server is UNHEALTHY.");
                }
            }
        }

    }
}
