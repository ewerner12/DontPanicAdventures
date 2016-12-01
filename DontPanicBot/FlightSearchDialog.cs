using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace DontPanicBot
{
    [Serializable]
    public class FlightSearchDialog : IDialog<object>
    {


        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(AskUserToStartSearch);
        }

        public async Task AskUserToStartSearch(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            List<string> greetings = new List<string> { "hello", "hi", "hey" };
            bool hasMatch = greetings.Contains(message.Text.ToLower());

            if (hasMatch)
            {
                await context.PostAsync("Would you like to start planning your next trip?");
                context.Wait(StartFlightSearch);
            }
            else
            {
                await context.PostAsync("I'm sorry, I didn't catch that. Welcome to Don't Panic Adventures!");
                context.Wait(AskUserToStartSearch);
            }
        }

        public async Task StartFlightSearch(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            List<string> affirmativeResponses = new List<string> { "yes", "yeah", "sure", "absolutely", "affirmative" };
            List<string> negativeResponses = new List<string> { "no", "nah", "not really" };
            bool affirmativeConfirmation = affirmativeResponses.Contains(message.Text.ToLower());
            bool negativeConfirmation = negativeResponses.Contains(message.Text.ToLower());


            if (affirmativeConfirmation)
            {
                //start form flow???
                await context.PostAsync("Congratulations, you decided to start your search. Good luck and have fun!");
            }
            else if (negativeConfirmation)
            {
                //then what would you like to do?
                await context.PostAsync("Not a problem. How may I assist you today?");
            }
            else
            {
                await context.PostAsync("I'm sorry, I didn't catch that. Would you like to start planning your next trip?");
                context.Wait(StartFlightSearch);
            }
        }









    }
}