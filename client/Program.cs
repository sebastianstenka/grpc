using System;
using System.Threading.Tasks;
using Greet;
using Grpc.Core;

namespace client
{
    internal class Program
    {
        const string Target = "127.0.0.1:50051";
        public static async Task Main(string[] args)
        {
            Channel channel = new Channel(Target, ChannelCredentials.Insecure);

            await channel.ConnectAsync().ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    Console.WriteLine("Client connected");
                }
            });

            var client = new GreetingService.GreetingServiceClient(channel);


            //Unary(client);
            await ServerStreaming(client);

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }

        private static void Unary(GreetingService.GreetingServiceClient client)
        {
            var request = new GreetingRequest()
            {
                Greeting = new Greeting() { FirstName = "John", LastName = "Smith" }
            };
            var response = client.Greet(request);
            Console.WriteLine(response.Result);
        }


        private static async Task ServerStreaming(GreetingService.GreetingServiceClient client)
        {
            var request = new GreetingRequest()
            {
                Greeting = new Greeting() { FirstName = "John", LastName = "Smith" }
            };

            var response = client.GreetManyTimes(request);

            while (await response.ResponseStream.MoveNext())
            {
                Console.WriteLine(response.ResponseStream.Current.Result);
                await Task.Delay(200);
            }
        }
    }
}