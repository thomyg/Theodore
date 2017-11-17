using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;


namespace Theodore.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string ShowMyTeamsOption = "Show my Teams";
        private const string CreateTeamOption = "Create Team";

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            PromptDialog.Choice(
                context,
                this.AfterChoiceSelected,
                new[] { ShowMyTeamsOption, CreateTeamOption },
                "Hi I'm Theodor your Team automation but. How can I help?",
                "I am sorry but I didn't understand that. I need you to select one of the options below",
                attempts: 2);
        }

        private async Task AfterChoiceSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                var selection = await result;

                switch (selection)
                {
                    case ShowMyTeamsOption:
                        await context.PostAsync("This functionality is not yet implemented!");
                        await this.StartAsync(context);
                        break;

                    case CreateTeamOption:
                        context.Call(new CreateTeamDialog(), this.AfterCreateTeam);
                        break;
                }
            }
            catch (TooManyAttemptsException)
            {
                await this.StartAsync(context);
            }
        }

        private async Task AfterCreateTeam(IDialogContext context, IAwaitable<bool> result)
        {
            var success = await result;

            if (success)
            {
                await context.PostAsync("Your team was created successfully, my pleasure.");
            }
            else
            {
                await context.PostAsync("Sorry I was unable to create your team. ;(");
            }

            await this.StartAsync(context);
        }
    }
}

