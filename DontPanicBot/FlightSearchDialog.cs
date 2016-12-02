using Autofac.Core;
using Google.Apis.QPXExpress.v1;
using Google.Apis.QPXExpress.v1.Data;
using Google.Apis.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Services.Description;

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
        protected string returnDate;
        protected string maxBudget;
        protected List<string> affirmativeResponses = new List<string> { "yes", "yeah", "sure", "absolutely", "affirmative", "yarp" };
        protected List<string> negativeResponses = new List<string> { "no", "nah", "not really", "nope" };
        protected Regex iataRegex = new Regex(@"[A-Z]{3}");
        protected Regex nameRegex = new Regex(@"[A-Z][a-z]+");
        protected Regex emailRegex = new Regex(@"\w+[@]\w+[.]\w+");
        protected Regex dateRegex = new Regex(@"^201[6-7]-\d{2}-\d{2}");
        protected Regex budgetRegex = new Regex(@"^[0-9]{1,4}[.][0-9]{2}$");

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
                await context.PostAsync("You must enter a name in the correct format. Enter 'yes' to try again.");
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
                //await context.PostAsync("You must enter a name in the correct format. Enter 'yes' to try again.");
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
                    "Which city would you like to depart from? (Use IATA code please)",
                    "Please enter a departure city name in the valid format.");
            }
            else
            {
                //await context.PostAsync("You must enter an email address in the correct format. Enter 'yes' to try again.");
                await GetEmailAddress(context, argument);
            }
        }

        public async Task GetDepartureDate(IDialogContext context, IAwaitable<string> argument)
        {
            string message = await argument;
            Match hasDepartureCity = iataRegex.Match(message);

            if (hasDepartureCity.Success)
            {
                departureCity = message;

                PromptDialog.Text(
                    context,
                    GetArrivalCity,
                    "What date would you like to depart on? (YYYY-MM-DD)",
                    "Please enter a departure date in the correct format.");
            }
            else
            {
                //await context.PostAsync("You must enter a city name in the correct format. Enter 'yes' to try again.");
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
                    GetReturnDate,
                    "Which city would you like to travel to? (Use IATA code please)",
                    "Please enter an arrival city name in the valid format.");
            }
            else
            {
                //await context.PostAsync("You must enter a date in the correct format. Enter 'yes' to try again.");
                await GetDepartureDate(context, argument);
            }
        }

        public async Task GetReturnDate(IDialogContext context, IAwaitable<string> argument)
        {
            string message = await argument;
            Match hasArrivalCity = iataRegex.Match(message);

            if (hasArrivalCity.Success)
            {
                arrivalCity = message;

                PromptDialog.Text(
                    context,
                    GetMaxBudget,
                    "When would you like to return? (YYYY-MM-DD)",
                    "Sorry, please enter a return date in the correct format.");
            }
            else
            {
                //await context.PostAsync("You must enter a city name in the correct format. Enter 'yes' to try again.");
                await GetArrivalCity(context, argument);
            }
        }

        public async Task GetMaxBudget(IDialogContext context, IAwaitable<string> argument)
        {
            string message = await argument;
            Match hasReturnDate = dateRegex.Match(message);

            if (hasReturnDate.Success)
            {
                returnDate = message;

                PromptDialog.Text(
                    context,
                    ConfirmSearchFlightParameters,
                    "What is your maximum budget for this trip? (in 888.88) ",
                    "Sorry, please enter a valid numerical value.");
            }
            else
            {
                //await context.PostAsync("You must enter a date in the correct format. Enter 'yes' to try again.");
                await GetReturnDate(context, argument);
            }
        }

        public async Task ConfirmSearchFlightParameters(IDialogContext context, IAwaitable<string> argument)
        {
            string message = await argument;
            Match hasMaxBudget = budgetRegex.Match(message);

            if (hasMaxBudget.Success & Convert.ToDouble(message) > 0)
            {
                maxBudget = "USD" + message;

                PromptDialog.Confirm(
                    context,
                    SearchForFlightOptions,
                    $"Please review that the following information is correct >> " +
                        $"First Name: {firstName} || " +
                        $"Last Name: {lastName} || " +
                        $"Email Address: {emailAddress} || " +
                        $"Departure City: {departureCity} || " +
                        $"Departure Date: {departureDate} || " +
                        $"Arrival City: {arrivalCity} || " +
                        $"Return Date: {returnDate} || " +
                        $"Budget: {maxBudget} ",
                    "Sorry, some information is missing. Please provide all information.");
            }
            else
            {
                //await context.PostAsync("You must enter a budget amount in the correct format. Enter 'yes' to try again.");
                await GetMaxBudget(context, argument);
            }
        }

        public async Task SearchForFlightOptions(IDialogContext context, IAwaitable<bool> argument)
        {
            bool fieldsCompleted = await argument;

            if (fieldsCompleted)
            {
                await context.PostAsync("Great! Searching for flight options now...");
                await SearchForFlights(context);
            }
            else
            {
                await context.PostAsync("Not a problem. Please try again!");
                context.Wait(AskUserToStartSearch);
            }

        }

        //AIzaSyAuHEH2vQFNlS19Y7pBD95BC-4y4Lt8zsw
        public async Task SearchForFlights(IDialogContext context)
        {
            var airports = new List<string>();
            var trips = new List<string>();

            QPXExpressService service = new QPXExpressService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyAuHEH2vQFNlS19Y7pBD95BC-4y4Lt8zsw",
                ApplicationName = "DontPanicAdventures"
            });

            TripsSearchRequest qpxRequest = new TripsSearchRequest();

            qpxRequest.Request = new TripOptionsRequest();
            qpxRequest.Request.Passengers = new PassengerCounts { AdultCount = 1 };
            qpxRequest.Request.Slice = new List<SliceInput>();
            qpxRequest.Request.Slice.Add(new SliceInput() { Origin = departureCity , Destination = arrivalCity, Date = departureDate  });
            qpxRequest.Request.MaxPrice = maxBudget;
            qpxRequest.Request.Solutions = 5;

            var results = service.Trips.Search(qpxRequest).Execute();

            foreach (var airport in results.Trips.Data.Airport)
            {
                airports.Add(airport.Code + " || " + airport.Name + " || " + airport.City);
            }

            foreach (var trip in results.Trips.TripOption)
            {
                trips.Add("Flight No.: " + trip.Slice.FirstOrDefault().Segment.FirstOrDefault().Flight.Number +
                            " || Duration: " + trip.Slice.FirstOrDefault().Duration + 
                            " || Price: " + trip.Pricing.FirstOrDefault().BaseFareTotal.ToString());
            }

            var flights = airports.Zip(trips, (a, t) => a + " || " + t);

            foreach (var flight in flights)
            {
                await context.PostAsync(flight);
            }

            PromptDialog.Text(
                context,
                StartNewSearch,
                "Thanks for using our services! Would you like to start a new search?",
                "Sorry I didn't catch that. Did you want to start a new search?");
        }

        public async Task StartNewSearch(IDialogContext context, IAwaitable<string> argument)
        {
            string message = await argument;
            bool confirmRestart = affirmativeResponses.Contains(message);
            bool rejectRestart = negativeResponses.Contains(message);

            if (confirmRestart)
            {
                await context.PostAsync("Sounds good! Let's start over!");
                await GetDepartureCity(context, argument);
            }
            else if (rejectRestart)
            {
                await context.PostAsync("No worries! Thanks for using our services and have a great day!");
                await StartNewSearch(context, argument);
            }
            else
            {
                await context.PostAsync("I'm sorry, I didn't catch that. Would you like to plan another trip?");
                await StartNewSearch(context, argument);
            }
        }







    }
}