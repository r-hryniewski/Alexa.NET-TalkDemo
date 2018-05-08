using System;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AlexaSample
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            var log = context.Logger;
            try
            {
                switch (input.Request)
                {
                    case LaunchRequest r:
                        return ResponseBuilder.Tell("You've launched your skill");
                    case IntentRequest r when r.Intent.Name == "MyIntent":
                        return ResponseBuilder.Tell("You've requested 'MyIntent'");
                    case IntentRequest r when r.Intent.Name == BuiltInIntent.Cancel:
                        return ResponseBuilder.Tell("You've requested 'Cancel'");
                    case IntentRequest r when r.Intent.Name == BuiltInIntent.Stop:
                        return ResponseBuilder.Tell("You've requested 'Cancel'");
                    case IntentRequest r when r.Intent.Name == BuiltInIntent.Help:
                        return ResponseBuilder.Tell("You've requested 'Help'");
                    default:
                        return ResponseBuilder.Tell("I don't know what you mean, I can help you if you ask me.");
                }
            }
            catch (Exception e)
            {
                log.LogLine(e.ToString());
                throw;
            }
        }
    }
}
