using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace Theodore.Dialogs
{
    [Serializable]
    public class CreateTeamDialog : IDialog<bool>
    {
        private const string EmailAliasRegexPattern = @"^\w+([-+.']\w+)*";
        private const string TeamDisplayNameRegexPattern = @"^[a-zA-Z]+$";
        private const string TEAMS_FUNCTION_URI = "%YOUR_AZURE_FUNCTION_ENDPOINT";
        private const string NEW_TEAM_CMD = "&cmd=NewTeam";
        private string alias;
        private string displayname;
        public async Task StartAsync(IDialogContext context)
        {
            var promptAliasDialog = new PromptStringRegex(
                "Please enter the alias for the new Team:",
                EmailAliasRegexPattern,
                "The value entered is not a valid email alias. Please try again.",
                "You have tried to create a new new team too many times. Please try again later.",
                attempts: 2);

            context.Call(promptAliasDialog, this.ResumeAfterAliasEntered);
        }

        private async Task ResumeAfterAliasEntered(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.alias = await result;

                if (this.alias != null)
                {
                    await context.PostAsync($"The alias you provided is: {this.alias}");

                    var promtpDisplayName = new PromptStringRegex(
                        "Please enter the display name of the team:",
                        TeamDisplayNameRegexPattern,
                        "The value you entered is not a valid display name. Only characters a-z allowed. Try again.",
                        "You have tried to enter a display nam too many times. Please try again later.",
                        attempts: 2);

                    context.Call(promtpDisplayName, this.ResumeAfterDisplayNameEntered);
                }
                else
                {
                    context.Done(false);
                }
            }
            catch (TooManyAttemptsException)
            {
                context.Done(false);
            }
        }

        private async Task ResumeAfterDisplayNameEntered(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                this.displayname = await result;

                // calling Azure Function for creating new team

                using (var client = new HttpClient())
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                        dictionary.Add("displayName", this.displayname);
                        dictionary.Add("alias", this.alias);

                        string json = JsonConvert.SerializeObject(dictionary);                    
                        var requestData = new StringContent(json, Encoding.UTF8, "application/json");                        
                        
                        var response = await client.PostAsync(String.Format(TEAMS_FUNCTION_URI+NEW_TEAM_CMD), requestData);
                        var teams_result = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode.ToString().Equals("OK"))
                    {
                        context.Done(true);
                    }
                    else
                    {
                        context.Done(false);
                    }
                }
                
            }
            catch (TooManyAttemptsException)
            {
                context.Done(false);
            }
        }
    }
}