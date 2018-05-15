using System;
using System.Collections.Generic;
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
            SkillResponse response = null;
            try
            {
                switch (input.Request)
                {
                    case LaunchRequest r:
                        response = ResponseBuilder.Ask(
                            speechResponse: responses.Launch,
                            reprompt: new Reprompt()
                            {
                                OutputSpeech = new PlainTextOutputSpeech() { Text = responses.Reprompt }
                            });
                        break;
                    case IntentRequest r when r.Intent.Name == "IndroductionIntent":
                        response = HandleIntroductionIntent();
                        break;
                    case IntentRequest r when r.Intent.Name == "AgendaIntent":
                        response = HandleAgendaIntent();
                        break;
                    case IntentRequest r when r.Intent.Name == "VoiceIntent":
                        response = HandleVoiceIntent();
                        break;
                    case IntentRequest r when r.Intent.Name == "TemperatureIntent":
                        response = HandleTemperatureIntent(r);
                        break;
                    case IntentRequest r when r.Intent.Name == "AudienceIntent":
                        response = HandleAudienceIntent(input);
                        break;
                    case IntentRequest r when r.Intent.Name == "SetAudienceCountIntent":
                        response = HandleSetAudienceCountIntent(input, r);
                        break;
                    case IntentRequest r when r.Intent.Name == "InvalidIntent":
                        response = HandleInvalidIntent(r);
                        break;
                    case IntentRequest r when r.Intent.Name == BuiltInIntent.Cancel:
                        response = ResponseBuilder.Tell("You've requested 'Cancel'");
                        break;
                    case IntentRequest r when r.Intent.Name == BuiltInIntent.Stop:
                        response = ResponseBuilder.Tell("You've requested 'Cancel'");
                        break;
                    case IntentRequest r when r.Intent.Name == BuiltInIntent.Help:
                        response = ResponseBuilder.Tell("You've requested 'Help'");
                        break;
                    default:
                        response = ResponseBuilder.Tell("I don't know what you mean, I can help you if you ask me.");
                        break;
                }
            }
            catch (Exception e)
            {
                log.LogLine(e.ToString());
                throw;
            }

            response.Response.ShouldEndSession = false;
            return response;
        }

        private SkillResponse HandleInvalidIntent(IntentRequest r) => ResponseBuilder.Tell(new SsmlOutputSpeech()
        {
            Ssml = "<speak>Maybe you should read the documentation?<speak>"
        });

        private SkillResponse HandleAudienceIntent(SkillRequest input)
        {
            if (input.Session.Attributes != null && 
                input.Session.Attributes.TryGetValue("AudienceCount", out var count))
            {
                input.Session.Attributes.Clear();
                return ResponseBuilder.Tell($"There are around {count} people here.", input.Session);
            }
            else
            {
                return ResponseBuilder.Ask("How the hell I'm supposed to know? I don't have camera dummy!", new Reprompt()
                {
                    OutputSpeech = new PlainTextOutputSpeech()
                    {
                        Text = responses.Reprompt
                    }
                },
                input.Session);
            }
        }

        private SkillResponse HandleSetAudienceCountIntent(SkillRequest input, IntentRequest r)
        {
            var count = r.Intent.Slots["count"].Value;
            var countInt = int.Parse(count);

            if (input.Session.Attributes == null)
                input.Session.Attributes = new Dictionary<string, object>();
            else
                input.Session.Attributes.Clear();

            input.Session.Attributes.Add("AudienceCount", countInt);

            return ResponseBuilder.Tell($"Ok I'll try to remember there are around {countInt} people here.", input.Session);
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
            "Everyone could use a short <break time='1s'/>break from time to time",
            "<prosody rate='x-slow'>I can talk very slow.</prosody><prosody rate='x-fast'>Or very fast</prosody>",
            "I can speak about some number as telephone number like <say-as interpret-as='telephone'>0 555 555 555</say-as>. I can also speak single digits<say-as interpret-as='digits'>555555555</say-as>. And tell it like ordinal number as well <say-as interpret-as='ordinal'>555555555</say-as>.",
            "I can also stress something <emphasis level='strong'>important</emphasis>"
        };

        public string VoiceSSML => $"<speak>{GetRandomResponse(voiceSSML)}</speak>";

        private string GetRandomResponse(string[] responseCollection)
        {
            if (responseCollection == null)
                throw new ArgumentNullException(nameof(responseCollection));

            return responseCollection[random.Next(0, responseCollection.Length)];
        }
    }
}
