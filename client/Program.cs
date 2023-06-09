﻿using System;
using System.Linq;
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
            UnaryWithDeadline(client);
            //await ServerStreaming(client);
            //await ClientStreaming(client);
            //await BiDirectionalStreaming(client);

            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }

        private static void Unary(GreetingService.GreetingServiceClient client)
        {
            try
            {
                var request = new GreetingRequest()
                {
                    Greeting = new Greeting() { FirstName = "John", LastName = "Smith" }
                };
                var response = client.Greet(request);
                Console.WriteLine(response.Result);
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Status.Detail);
            }
        }

        private static void UnaryWithDeadline(GreetingService.GreetingServiceClient client)
        {
            try
            {
                var request = new GreetingRequest()
                {
                    Greeting = new Greeting() { FirstName = "John", LastName = "Smith" }
                };
                var response = client.Greet(
                    request, 
                    deadline: DateTime.UtcNow.AddMilliseconds(50));

                Console.WriteLine(response.Result);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
            {
                Console.WriteLine(ex.Status.Detail);
            }
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

        private static async Task ClientStreaming(GreetingService.GreetingServiceClient client)
        {
            var request = new GreetingRequest()
            {
                Greeting = new Greeting() { FirstName = "John", LastName = "Smith" }
            };

            var stream = client.LongGreet();

            foreach (var i in Enumerable.Range(1, 10))
            {
                await stream.RequestStream.WriteAsync(request);
            }

            await stream.RequestStream.CompleteAsync();

            var response = await stream.ResponseAsync;

            Console.WriteLine(response.Result);
        }

        private static async Task BiDirectionalStreaming(GreetingService.GreetingServiceClient client)
        {
            var stream = client.GreetEveryone();

            var responseReaderTask = Task.Run(async () =>
            {
                while (await stream.ResponseStream.MoveNext())
                {
                    Console.WriteLine("Received " + stream.ResponseStream.Current.Result);
                }
            });

            Greeting[] greetings =
            {
                new Greeting() { FirstName = "John", LastName = "Smith" },
                new Greeting() { FirstName = "John", LastName = "Bean" }
            };

            foreach (var greeting in greetings)
            {
                Console.WriteLine("Sending " + greeting);
                await stream.RequestStream.WriteAsync(new GreetingRequest() { Greeting = greeting });
            }

            await stream.RequestStream.CompleteAsync();
            await responseReaderTask;
        }
    }
}