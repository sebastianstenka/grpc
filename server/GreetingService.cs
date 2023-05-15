using Greet;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Greet.GreetingService;

namespace server
{
    public class GreetingServiceImpl : GreetingServiceBase
    {
        public override Task<GreetingResponse> Greet(GreetingRequest request, ServerCallContext context)
        {
            var result = $"hello {request.Greeting.FirstName} {request.Greeting.LastName}";

            return Task.FromResult(new GreetingResponse() { Result = result }); 
        }

        public override async Task GreetManyTimes(GreetingRequest request, IServerStreamWriter<GreetingResponse> responseStream, ServerCallContext context)
        {
            Console.WriteLine("The server received the request");
            Console.WriteLine(request.ToString());

            var result = $"{request.Greeting.FirstName} {request.Greeting.LastName}";

            foreach (var i in Enumerable.Range(1,10))
            {
                await responseStream.WriteAsync(new GreetingResponse() { Result = result + $" {i}" });
            }
        }
    }
}
