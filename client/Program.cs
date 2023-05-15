using System;
using System.Threading.Tasks;
using Greet;
using Grpc.Core;

namespace client
{
    internal class Program
    {
        const string Target = "127.0.0.1:50051";
        public static void Main(string[] args)
        {
            Channel channel = new Channel(Target, ChannelCredentials.Insecure);

            channel.ConnectAsync().ContinueWith((task) =>
            {
                if(task.Status == TaskStatus.RanToCompletion)
                {
                    Console.WriteLine("Client connected");
                }
            });

            var client = new GreetingService.GreetingServiceClient(channel);
            var request = new GreetingRequest()
            {
                Greeting = new Greeting() { FirstName = "John", LastName = "Smith" }
            };

            var response = client.Greet(request);
            Console.WriteLine(response.Result);

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }
    }
}