using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;

namespace DontPanicBot
{
    [Serializable]
    public class FlightSearchForm
    {
        [Prompt("Please enter your first name: {||}")]
        [Pattern(@"[A-Z][a-z]+")]
        public string firstName;

        [Prompt("Please enter your last name: {||}")]
        [Pattern(@"[A-Z][a-z]+")]
        public string lastName;

        [Prompt("Please enter your email address: {||}")]
        [Pattern(@"\w+[@]\w+[.]\w+")]
        public string emailAddress;

        [Prompt("Which city would you like to depart from? {||}")]
        [Pattern(@"[A-Z][a-z]+")]
        public string departureCity;

        [Prompt("Which date would you like to depart on? {||}")]
        [Pattern(@"\d{1,2}\/\d{1,2}\/2016$")]
        public string departureDate;

        [Prompt("What is the maximum amount you would like to spend? {||}")]
        [Pattern(@"\d+")]
        public string maxBudget;

        public static IForm<FlightSearchForm> BuildForm()
        {
            OnCompletionAsyncDelegate<FlightSearchForm> beginFlightSearch = async (context, state) =>
            {
                string searchingMessage = "Starting the search for your next trip!";
                await context.PostAsync(searchingMessage);
            };

            return new FormBuilder<FlightSearchForm>()
                .Message("Hello. Let's start planning your next trip!")
                .Field(nameof(firstName))
                .Field(nameof(lastName))
                .Field(nameof(emailAddress))
                .Field(nameof(departureCity))
                .Field(nameof(departureDate))
                .Field(nameof(maxBudget))
                .OnCompletion(beginFlightSearch)
                .Build();
        }

    }


}