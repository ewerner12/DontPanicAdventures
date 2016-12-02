using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace DontPanicBot
{
    [Serializable]
    public class FlightSearchDialog : IDialog<object>
    {
        //////internal static IDialog<FlightSearchForm> MakeFlightSearch()
        //////{
        //////    return Chain.From(() => FormDialog.FromForm(FlightSearchForm.BuildForm));
        //////}
        protected string firstName;
        protected string lastName;
        protected string emailAddress;
        protected string departureCity;
        protected string departureDate;
        protected string arrivalCity;
        protected string arrivalDate; //do not need to prompt for this
        protected string maxBudget;
        protected Regex nameRegex = new Regex(@"[A-Z][a-z]+");
        protected Regex emailRegex = new Regex(@"\w+[@]\w+[.]\w+");
        protected Regex dateRegex = new Regex(@"\d{1,2}\/\d{1,2}\/201[6-7]$");
        protected Regex budgetRegex = new Regex(@"[0-9]+");

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(AskUserToStartSearch);
        }

        public async Task AskUserToStartSearch(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            List<string> greetings = new List<string> { "hello", "hi", "hey", "yo" };
            bool hasMatch = greetings.Contains(message.Text.ToLower());

            if (hasMatch)
            {
                await context.PostAsync("Hello and welcome to Don't Panic Adventures! Would you like to start planning your next trip?");
                context.Wait(StartFlightSearch);
            }
            else
            {
                await context.PostAsync("I'm sorry, I don't understand what you just said, but welcome to Don't Panic Adventures!");
                context.Wait(AskUserToStartSearch);
            }
        }

        public async Task StartFlightSearch(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            //////var activity = await context; //need to create an Activity
            var message = await argument;
            List<string> affirmativeResponses = new List<string> { "yes", "yeah", "sure", "absolutely", "affirmative", "yarp" };
            List<string> negativeResponses = new List<string> { "no", "nah", "not really", "nope" };
            bool affirmativeConfirmation = affirmativeResponses.Contains(message.Text.ToLower());
            bool negativeConfirmation = negativeResponses.Contains(message.Text.ToLower());

            if (affirmativeConfirmation)
            {
                await context.PostAsync("Great! Let's get started!");
                await GetFirstName(context, argument);
            }
            else if (negativeConfirmation)
            {
                await context.PostAsync("Not a problem. How may I assist you today?");
                context.Wait(StartFlightSearch);
            }
            else
            {
                await context.PostAsync("I'm sorry, I didn't catch that. Would you like to start planning your next trip?");
                context.Wait(StartFlightSearch);
            }
        }

        public async Task GetFirstName(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            PromptDialog.Text(
                context,
                GetLastName,
                "What is your first name?",
                "Please enter a first name in the correct format.");
        }

        public async Task GetLastName(IDialogContext context, IAwaitable<string> argument)
        {
            string message = await argument;
            Match hasFirstName = nameRegex.Match(message);

            if (hasFirstName.Success)
            {
                firstName = message;

                PromptDialog.Text(
                    context,
                    GetEmailAddress,
                    "What is your last name?",
                    "Please enter a last name in the correct format.");
            }
            else
            {
                context.Wait(GetFirstName);
            }
        }

        public async Task GetEmailAddress(IDialogContext context, IAwaitable<string> argument)
        {
            string message = await argument;
            Match hasLastName = nameRegex.Match(message);

            if (hasLastName.Success)
            {
                lastName = message;

                PromptDialog.Text(
                    context,
                    GetDepartureCity,
                    "What is the best email address to contact you at?",
                    "Please enter a valid email address.");
            }
            else
            {
                await GetLastName(context, argument);
            }
        }

        public async Task GetDepartureCity(IDialogContext context, IAwaitable<string> argument)
        {
            string message = await argument;
            Match hasEmail = emailRegex.Match(message);

            if (hasEmail.Success)
            {
                emailAddress = message;

                PromptDialog.Text(
                    context,
                    GetDepartureDate,
                    "Which city would you like to depart from?",
                    "Please enter a departure city name in the valid format.");
            }
            else
            {
                await GetEmailAddress(context, argument);
            }
        }

        public async Task GetDepartureDate(IDialogContext context, IAwaitable<string> argument)
        {
            string message = await argument;
            Match hasDepartureCity = nameRegex.Match(message);

            if (hasDepartureCity.Success)
            {
                departureCity = message;

                PromptDialog.Text(
                    context,
                    GetArrivalCity,
                    "What date would you like to depart on?",
                    "Please enter a departure date in the correct format.");
            }
            else
            {
                await GetDepartureCity(context, argument);
            }
        }

        public async Task GetArrivalCity(IDialogContext context, IAwaitable<string> argument)
        {
            string message = await argument;
            Match hasDepartureDate = dateRegex.Match(message);

            if (hasDepartureDate.Success)
            {
                departureDate = message;

                PromptDialog.Text(
                    context,
                    GetMaxBudget,
                    "Which city would you like to travel to?",
                    "Please enter an arrival city name in the valid format.");
            }
            else
            {
                await GetDepartureDate(context, argument);
            }
        }

        public async Task GetMaxBudget(IDialogContext context, IAwaitable<string> argument)
        {
            string message = await argument;
            Match hasArrivalCity = nameRegex.Match(message);

            if (hasArrivalCity.Success)
            {
                arrivalCity = message;

                PromptDialog.Text(
                    context,
                    ConfirmSearchFlightParameters,
                    "What is your maximum budget for this trip?",
                    "Sorry, please enter a valid numerical value.");
            }
            else
            {
                await GetArrivalCity(context, argument);
            }
        }

        public async Task ConfirmSearchFlightParameters(IDialogContext context, IAwaitable<string> argument)
        {
            string message = await argument;
            Match hasMaxBudget = budgetRegex.Match(message);

            if (hasMaxBudget.Success & Convert.ToInt32(message) > 0)
            {
                maxBudget = message;

                PromptDialog.Confirm(
                    context,
                    SearchForFlightOptions,
                    $"Please review that the following information is correct:\r\nFirst Name: {firstName}" +
                        Environment.NewLine + 
                        $"First Name: {firstName}" +
                        Environment.NewLine +
                        $"Last Name: {lastName}" +
                        Environment.NewLine +
                        $"Email Address: {emailAddress}" +
                        Environment.NewLine +
                        $"Departure City: {departureCity}" +
                        Environment.NewLine +
                        $"Departure Date: {departureDate}" +
                        Environment.NewLine +
                        $"Arrival City: {arrivalCity}" +
                        Environment.NewLine +
                        $"Budget: ${maxBudget}",
                    "Sorry, some information is missing. Please provide all information.");
            }
            else
            {
                await GetMaxBudget(context, argument);
            }
        }

        public async Task SearchForFlightOptions(IDialogContext context, IAwaitable<bool> argument)
        {
            bool fieldsCompleted = await argument;

            if (fieldsCompleted)
            {
                await context.PostAsync("Thanks! Searching for flight options now...");
            }
            else
            {
                await context.PostAsync("Not a problem. Please try again!");
            }

        }

        //////public async Task GetArrivalDate(IDialogContext context, IAwaitable<string> argument)
        //////{
        //////    string message = await argument;
        //////    Match hasFirstName = nameRegex.Match(message);

        //////    if (hasFirstName.Success)
        //////    {
        //////        PromptDialog.Text(
        //////            context,
        //////            GetEmailAddress,
        //////            "Please enter your last name: ",
        //////            "Sorry, please enter a name in the correct format.");
        //////    }
        //////    else
        //////    {
        //////        context.Wait(GetFirstName);
        //////    }
        //////}








    }
}