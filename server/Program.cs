using System;
using System.IO;
using Greet;
using Grpc.Core;

namespace server
{
    internal class Program
    {
        const int Port = 50051;
        public static void Main(string[] args)
        {
            Server server = null;

            try
            {
                server = new Server
                {
                    Services = { GreetingService.BindService(new GreetingServiceImpl()) },
                    Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
                };

                server.Start();
                Console.WriteLine($"The server is listening port {Port}");
                Console.ReadKey();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
            finally
            {
                if (server != null)
                {
                    server.ShutdownAsync().Wait();
                }
            }
        }
    }
}