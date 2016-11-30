﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.Bot.Builder.Dialogs;

namespace DontPanicBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            if (activity != null && activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new EchoDialog());

                //////// calculate something for us to return
                //////int length = (activity.Text ?? string.Empty).Length;

                //////// return our reply to the user
                //////Activity reply = activity.CreateReply($"You sent \"{activity.Text}\" which was {length} characters long.");
                //////await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                Activity reply = HandleSystemMessage(activity);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
                if (message.Action == "add")
                {
                    return message.CreateReply($"Hello {message.From.Name} and welcome to your Don't Panic Adventures bot conversation!");
                }
                if (message.Action == "remove")
                {
                    return message.CreateReply($"{message.From.Name} has left the chat...");
                }
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
                return message.CreateReply($"{message.From.Name} is typing...");
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        [Serializable]
        public class EchoDialog : IDialog<object>
        {
            protected int count = 1;

            public async Task StartAsync(IDialogContext context)
            {
                context.Wait(MessageReceivedAsync);
            }

            public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
            {
                var message = await argument;

                if (message.Text == "reset")
                {
                    PromptDialog.Confirm(context,
                                            AfterResetAsync,
                                            "Are you sure you want to reset the count?",
                                            "Sorry, I didn't catch that!",
                                            promptStyle: PromptStyle.None);
                }
                else
                {
                    await context.PostAsync($"[{count++}] You said: {message.Text}");
                    context.Wait(MessageReceivedAsync);
                }
            }

            public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
            {
                var confirm = await argument;
                
                if (confirm)
                {
                    this.count = 1;
                    await context.PostAsync("Count has been reset.");
                }
                else
                {
                    await context.PostAsync("Count was not reset.");
                }
                context.Wait(MessageReceivedAsync);
            }


        }

        public class SampleDialog : IDialog<object>
        {
            protected string firstName;
            protected string lastName;


            public async Task StartAsync(IDialogContext context)
            {
                context.Wait(ReceivedAsyncMessage);
            }

            public async Task ReceivedAsyncMessage(IDialogContext context, IAwaitable<IMessageActivity> argument)
            {
                var message = await argument;


            }
        }







    }
}