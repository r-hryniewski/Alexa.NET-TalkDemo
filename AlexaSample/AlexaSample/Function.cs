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
        private readonly Responses responses;
        public Function()
        {
            responses = new Responses(new Random());
        }
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
                        return ResponseBuilder.Ask(
                            speechResponse: responses.Launch,
                            reprompt: new Reprompt()
                            {
                                OutputSpeech = new PlainTextOutputSpeech() { Text = responses.Reprompt }
                            });
                    case IntentRequest r when r.Intent.Name == "IndroductionIntent":
                        return HandleIntroductionIntent();
                    case IntentRequest r when r.Intent.Name == "AgendaIntent":
                        return HandleAgendaIntent();
                    case IntentRequest r when r.Intent.Name == "VoiceIntent":
                        return HandleVoiceIntent();
                    case IntentRequest r when r.Intent.Name == "TemperatureIntent":
                        return HandleTemperatureIntent(r);
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

        private SkillResponse HandleIntroductionIntent() => ResponseBuilder.Tell(new SsmlOutputSpeech() { Ssml = "<speak><s>Hey there!</s><s> My name is Alexa and I'm an virtual assistant.</s> <s>Most of the time I'm sitting on desk in this dude's basement.</s> <s>I could tell you thing or two about him, if any of you would like to grab a beer after his talk.</s> <break time='3s'/> <s>I know some embarassing stuff.</s><s><amazon:effect name='whispered'>A lot of embarassing stuff.</amazon:effect></s></speak>" });

        private SkillResponse HandleAgendaIntent() => ResponseBuilder.Tell(new SsmlOutputSpeech()
        {
            Ssml =
            @"<speak>
                <prosody volume='loud'>
                    <p>
                        So, we've just introduced ourselves.
                    </p>
                    <p>
                        Now we're going to talk a little bit about history of voice recognition. When we're reach present time. We'll move to reason it's me who's here and not,<break time='0.25s'/> for example<break time='0.25s'/>, Cortana.
                    </p>
                    <p>
                        After this short introduction we're going to do fast one-oh-one on how to design skills for me <break time='0.125s'/>  and how to work with my voice.
                    </p>
                    <p>
                        Then we'll talk a while about some more advanced features like sessions and dialogs.
                    </p>
                    <p>
                        At the end we'll tell you where's money in my skills<break time='0.125s'/> and how to attach any skill you'll design<break time='0.125s'/> to your exisiting solutions.
                    </p>
                    <p>
                        <say-as interpret-as='interjection'>Well well!</say-as> It's going to be fun!
                    </p>
                </prosody>
            </speak>"
        });

        private SkillResponse HandleVoiceIntent()
        {
            return ResponseBuilder.Tell(new SsmlOutputSpeech()
            {
                Ssml = responses.VoiceSSML
            });
        }

        private SkillResponse HandleTemperatureIntent(IntentRequest r)
        {
            var amount = r.Intent.Slots["amount"].Value;
            var temperatureUnit = r.Intent.Slots["temperatureunit"].Value;

            return ResponseBuilder.Tell(
                $"It's {amount} degrees {temperatureUnit}. But you're supposed to ask me about things like that and not telling me them.");
        }
    }

    public class Responses
    {
        private readonly Random random;

        public Responses(Random random)
        {
            this.random = random;
        }

        private readonly string[] launch = new string[]
        {
            "What can I do for you?",
            "What now?",
            "How can I help you?"
        };
        public string Launch => GetRandomResponse(launch);

        private readonly string[] reprompt = new string[]
        {
            "Hey! They're waiting!",
            "Wakey, wakey",
            "Seriously? Did you just fell asleep on your own talk?"
        };
        public string Reprompt => GetRandomResponse(reprompt);

        public string Introduction =>
            "Hey there! My name is Alexa and I'm an virtual assistant. Most of the time I'm sitting on desk in this dude's basement. I could tell you thing or two about him, if any of you would like to grab a beer after his talk.";
        
        private readonly string[] voiceSSML = new string[]
        {
            "TODO"
        };

        public string VoiceSSML => GetRandomResponse(voiceSSML);

    private string GetRandomResponse(string[] responseCollection)
        {
            if (responseCollection == null)
                throw new ArgumentNullException(nameof(responseCollection));

            return responseCollection[random.Next(0, responseCollection.Length)];
        }
    }
}
