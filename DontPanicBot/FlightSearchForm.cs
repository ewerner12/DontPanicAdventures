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

        [Prompt("Which city would you like to travel to? {||}")]
        [Pattern(@"[A-Z][a-z]+")]
        public string arrivalCity;

        [Prompt("What is the maximum amount you would like to spend? {||}")]
        [Pattern(@"\d+")]
        public string maxBudget;

        public static IForm<FlightSearchForm> BuildForm()
        {
            OnCompletionAsyncDelegate<FlightSearchForm> saveFlightSearchParams = async (context, form) =>
            {
                string searchingMessage = "Starting the search for your next trip!";
                await context.PostAsync(searchingMessage);
                context.PrivateConversationData.SetValue<bool>("SearchParamsComplete", true);
                context.PrivateConversationData.SetValue<string>("firstName", form.firstName);
                context.PrivateConversationData.SetValue<string>("lastName", form.lastName);
                context.PrivateConversationData.SetValue<string>("emailAddress", form.emailAddress);
                context.PrivateConversationData.SetValue<string>("departureCity", form.departureCity);
                context.PrivateConversationData.SetValue<string>("departureDate", form.departureDate);
                context.PrivateConversationData.SetValue<string>("arrivalCity", form.arrivalCity);
                context.PrivateConversationData.SetValue<string>("maxBudget", form.maxBudget);
                await context.PostAsync("Your search parameters are all set!");
            };

            return new FormBuilder<FlightSearchForm>()
                .Message("Great. Let's start planning your next trip!")
                .Field(nameof(firstName))
                .Field(nameof(lastName))
                .Field(nameof(emailAddress))
                .Field(nameof(departureCity))
                .Field(nameof(departureDate))
                .Field(nameof(arrivalCity))
                .Field(nameof(maxBudget))
                .OnCompletion(saveFlightSearchParams)
                .Build();
        }

    }


}