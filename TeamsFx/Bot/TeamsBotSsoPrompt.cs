﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Bot.Schema;
using System.Text.RegularExpressions;
using System.Net;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Connector;
using Microsoft.TeamsFx.Helper;
using Microsoft.Identity.Client;
using Microsoft.TeamsFx.Model;

namespace Microsoft.TeamsFx.Bot;

/// <summary>
/// Creates a new prompt that leverage Teams Single Sign On (SSO) support for bot to automatically sign in user and
/// help receive oauth token, asks the user to consent if needed.
/// </summary>
/// <remarks>
/// The prompt will attempt to retrieve the user's current token of the desired scopes.
/// User will be automatically signed in leveraging Teams support of Bot Single Sign On(SSO):
/// https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/authentication/auth-aad-sso-bots
/// </remarks>
/// 
/// <example>
/// ## Prompt Usage
///
/// When used with your bot's <see cref="DialogSet"/> you can simply add a new instance of the prompt as a named
/// dialog using <see cref="DialogSet.Add(Dialog)"/>. You can then start the prompt from a waterfall step using either
/// <see cref="DialogContext.BeginDialogAsync(string, object, CancellationToken)"/> or
/// <see cref="DialogContext.PromptAsync(string, PromptOptions, CancellationToken)"/>. The user
/// will be prompted to signin as needed and their access token will be passed as an argument to
/// the caller's next waterfall step.
/// 
/// <code>
/// var convoState = new ConversationState(new MemoryStorage());
/// var dialogState = convoState.CreateProperty&lt;DialogState&gt;("dialogState");
/// var dialogs = new DialogSet(dialogState); 
/// var botAuthOptions = new BotAuthenticationOptions { 
///     ClientId = "{client_id_guid_value}", 
///     ClientSecret = "{client_secret_value}", 
///     TenantId = "{tenant_id_guid_value}", 
///     ApplicationIdUri = "{application_id_uri_value}", 
///     OAuthAuthority = "https://login.microsoftonline.com/{tenant_id_guid_value}", 
///     LoginStartPageEndpoint = "https://{bot_web_app_domain}/bot-auth-start" 
///     };
///     
/// var scopes = new string[] { "User.Read" };
/// var teamsBotSsoPromptSettings = new TeamsBotSsoPromptSettings(botAuthOptions, scopes);
/// 
/// dialogs.Add(new TeamsBotSsoPrompt("{unique_id_for_the_prompt}", teamsBotSsoPromptSettings)); 
/// dialogs.Add(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[] 
/// { 
///     async(WaterfallStepContext stepContext, CancellationToken cancellationToken) => {
///         return await stepContext.BeginDialogAsync(nameof(TeamsBotSsoPrompt), null, cancellationToken);
///     }, 
///     async(WaterfallStepContext stepContext, CancellationToken cancellationToken) => { 
///         var tokenResponse = (TeamsBotSsoPromptTokenResponse)stepContext.Result; 
///         if (tokenResponse?.Token != null) 
///         { 
///             // ... continue with task needing access token ... 
///         } 
///         else
///         {
///             await stepContext.Context.SendActivityAsync(MessageFactory.Text("Login was not successful please try again."), cancellationToken);
///         }
///         return await stepContext.EndDialogAsync(cancellationToken: cancellationToken); 
///     } 
/// }));
/// </code>
/// 
/// </example>
public class TeamsBotSsoPrompt : Dialog
{
    private readonly TeamsBotSsoPromptSettings _settings;
    private const string PersistedExpires = "expires";
    internal IIdentityClientAdapter _identityClientAdapter { private get; set; }
    internal ITeamsInfo _teamsInfo { private get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="TeamsBotSsoPrompt"/> class.
    /// </summary>
    /// <param name="dialogId">The ID to assign to this prompt.</param>
    /// <param name="settings">Additional OAuth settings to use with this instance of the prompt.</param>
    /// <remarks>The value of <paramref name="dialogId"/> must be unique within the
    /// <see cref="DialogSet"/> or <see cref="ComponentDialog"/> to which the prompt is added.</remarks>
    /// <exception cref="ExceptionCode.InvalidParameter">When input parameters is null.</exception>
    public TeamsBotSsoPrompt(string dialogId, TeamsBotSsoPromptSettings settings): base(dialogId)
    {
        if (string.IsNullOrWhiteSpace(dialogId))
        {
            throw new ExceptionWithCode($"Parameter {nameof(dialogId)} is null or empty.", ExceptionCode.InvalidParameter);
        }
        _settings = settings ?? throw new ExceptionWithCode($"Parameter {nameof(settings)} is null or empty.", ExceptionCode.InvalidParameter);

        var confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(_settings.BotAuthOptions.ClientId)
            .WithClientSecret(_settings.BotAuthOptions.ClientSecret)
            .WithAuthority(_settings.BotAuthOptions.OAuthAuthority)
            .Build();
        _identityClientAdapter = new IdentityClientAdapter(confidentialClientApplication);
        _teamsInfo = new TeamsInfoWrapper();
    }

    /// <summary>
    /// Called when the dialog is started and pushed onto the dialog stack.
    /// </summary>
    /// <param name="dialogContext">The Microsoft.Bot.Builder.Dialogs.DialogContext for the current turn of conversation.</param>
    /// <param name="options">Optional, initial information to pass to the dialog.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    /// <returns> A System.Threading.Tasks.Task representing the asynchronous operation.</returns>
    /// <exception cref="ExceptionCode.InvalidParameter">if dialog context argument is null</exception>
    public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dialogContext, object options = null, CancellationToken cancellationToken = default)
    {
        if (dialogContext == null)
        {
            throw new ExceptionWithCode($"Parameter {nameof(dialogContext)} is null or empty.", ExceptionCode.InvalidParameter);
        }

        EnsureMsTeamsChannel(dialogContext);

        var state = dialogContext.ActiveDialog?.State;
        state[PersistedExpires] = DateTime.UtcNow.AddMilliseconds(_settings.Timeout);

        // Send OAuthCard that tells Teams to obtain an authentication token for the bot application.
        await SendOAuthCardToObtainTokenAsync(dialogContext.Context, cancellationToken).ConfigureAwait(false);
        return EndOfTurn;
    }

    /// <summary>
    /// Called when a prompt dialog is the active dialog and the user replied with a new activity.
    /// </summary>
    /// <param name="dc">The <see cref="DialogContext"/> for the current turn of conversation.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects
    /// or threads to receive notice of cancellation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>If the task is successful, the result indicates whether the dialog is still
    /// active after the turn has been processed by the dialog.
    /// <para>The prompt generally ends on invalid message from user's reply.</para></remarks>
    /// <exception cref="ExceptionCode.InternalError">When failed to login with unknown error.</exception>
    /// <exception cref="ExceptionCode.ServiceError">When failed to get access token from identity server(AAD).</exception>
    public override async Task<DialogTurnResult> ContinueDialogAsync(DialogContext dc, CancellationToken cancellationToken = default(CancellationToken))
    {
        EnsureMsTeamsChannel(dc);

        // Check for timeout
        var state = dc.ActiveDialog?.State;
        bool isMessage = (dc.Context.Activity.Type == ActivityTypes.Message);
        bool isTimeoutActivityType =
          isMessage ||
          IsTeamsVerificationInvoke(dc.Context) ||
          IsTokenExchangeRequestInvoke(dc.Context);

        // If the incoming Activity is a message, or an Activity Type normally handled by TeamsBotSsoPrompt,
        // check to see if this TeamsBotSsoPrompt Expiration has elapsed, and end the dialog if so.
        bool hasTimedOut = isTimeoutActivityType && DateTime.Compare(DateTime.UtcNow, (DateTime)state[PersistedExpires]) > 0;
        if (hasTimedOut)
        {
            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        else
        {
            if (IsTeamsVerificationInvoke(dc.Context) || IsTokenExchangeRequestInvoke(dc.Context))
            {
                // Recognize token
                PromptRecognizerResult<TeamsBotSsoPromptTokenResponse> recognized = await RecognizeTokenAsync(dc, cancellationToken).ConfigureAwait(false);

                if (recognized.Succeeded)
                {
                    return await dc.EndDialogAsync(recognized.Value, cancellationToken).ConfigureAwait(false);
                }
            }
            else if (isMessage)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            return EndOfTurn;
        }
    }

    /// <summary>
    /// This is intended for internal use.
    /// </summary>
    /// <param name="dc">DialogContext.</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>PromptRecognizerResult.</returns>
    /// <exception cref="ExceptionCode.InternalError">When failed to login with unknown error.</exception>
    /// <exception cref="ExceptionCode.ServiceError">When failed to get access token from identity server(AAD).</exception>
    private async Task<PromptRecognizerResult<TeamsBotSsoPromptTokenResponse>> RecognizeTokenAsync(DialogContext dc, CancellationToken cancellationToken)
    {

        ITurnContext context = dc.Context;
        var result = new PromptRecognizerResult<TeamsBotSsoPromptTokenResponse>();
        TeamsBotSsoPromptTokenResponse tokenResponse = null;

        if (IsTokenExchangeRequestInvoke(context))
        {
            var tokenResponseObject = context.Activity.Value as JObject;
            string ssoToken = tokenResponseObject?.ToObject<TokenExchangeInvokeRequest>().Token;
            // Received activity is not a token exchange request
            if (String.IsNullOrEmpty(ssoToken))
            {
                var warningMsg =
                  "The bot received an InvokeActivity that is missing a TokenExchangeInvokeRequest value. This is required to be sent with the InvokeActivity.";
                await SendInvokeResponseAsync(context, HttpStatusCode.BadRequest, warningMsg, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                try
                {
                    var exchangedToken = await GetToken(ssoToken, _settings.Scopes).ConfigureAwait(false);

                    var ssoTokenObj = Utils.ParseJwt(ssoToken);
                    var ssoExpiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(ssoTokenObj.Payload["exp"].ToString()));
                    tokenResponse = new TeamsBotSsoPromptTokenResponse {
                        SsoToken = ssoToken,
                        SsoTokenExpiration = ssoExpiration.ToString(),
                        Token = exchangedToken.Token,
                        Expiration = exchangedToken.ExpiresOn.ToString()
                    };

                    await SendInvokeResponseAsync(context, HttpStatusCode.OK, null, cancellationToken).ConfigureAwait(false);
                }
                catch (MsalUiRequiredException) // Need user interaction
                {
                    var warningMsg = "The bot is unable to exchange token. Ask for user consent first.";
                    await SendInvokeResponseAsync(context, HttpStatusCode.PreconditionFailed, new TokenExchangeInvokeResponse {
                        Id = context.Activity.Id,
                        FailureDetail = warningMsg,
                    }, cancellationToken).ConfigureAwait(false);
                }
                catch (MsalServiceException ex) // Errors that returned from AAD service
                {
                    throw new ExceptionWithCode($"Failed to get access token from OAuth identity server with error: {ex.ResponseBody}", ExceptionCode.ServiceError);
                }
                catch (MsalClientException ex) // Exceptions that are local to the MSAL library
                {
                    throw new ExceptionWithCode($"Failed to get access token with error: {ex.Message}", ExceptionCode.InternalError);
                }

            }
        }
        else if (IsTeamsVerificationInvoke(context))
        {
            await SendOAuthCardToObtainTokenAsync(context, cancellationToken).ConfigureAwait(false);
            await SendInvokeResponseAsync(context, HttpStatusCode.OK, null, cancellationToken).ConfigureAwait(false);
        }

        if (tokenResponse != null)
        {
            result.Succeeded = true;
            result.Value = tokenResponse;
        } else
        {
            result.Succeeded = false;
        }
        return result;
    }

    private async Task<AccessToken> GetToken(string ssoToken, string[] scopes)
    {
        AccessToken result;
        var ssoTokenObj = Utils.ParseJwt(ssoToken);
        var ssoTokenExpiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(ssoTokenObj.Payload["exp"].ToString())); 
        
        // Get sso token
        if (scopes.Length == 0)
        {
            if (DateTimeOffset.Compare(DateTimeOffset.UtcNow, ssoTokenExpiration) > 0)
            {
                throw new ExceptionWithCode("SSO token has already expired.", ExceptionCode.TokenExpiredError);
            }
            result = new AccessToken(ssoToken, ssoTokenExpiration);
        }
        else
        {
            var authenticationResult = await _identityClientAdapter.GetAccessToken(ssoToken, scopes).ConfigureAwait(false);
            result = new AccessToken(authenticationResult.AccessToken, authenticationResult.ExpiresOn);
        }
        return result;
    }

    private static async Task SendInvokeResponseAsync(ITurnContext turnContext, HttpStatusCode statusCode, object body, CancellationToken cancellationToken)
    {
        await turnContext.SendActivityAsync(
            new Activity {
                Type = ActivityTypesEx.InvokeResponse,
                Value = new InvokeResponse {
                    Status = (int)statusCode,
                    Body = body,
                },
            }, cancellationToken).ConfigureAwait(false);
    }

    private bool IsTeamsVerificationInvoke(ITurnContext context) {
       return (context.Activity.Type == ActivityTypes.Invoke) && (context.Activity.Name == SignInConstants.VerifyStateOperationName);
    }
    private bool IsTokenExchangeRequestInvoke(ITurnContext context) {
        return (context.Activity.Type == ActivityTypes.Invoke) && (context.Activity.Name == SignInConstants.TokenExchangeOperationName);
    }

    /// <summary>
    /// Send OAuthCard that tells Teams to obtain an authentication token for the bot application.
    /// For details see https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/authentication/auth-aad-sso-bots.
    /// </summary>
    /// <param name="context">ITurnContext</param>
    /// <param name="cancellationToken">CancellationToken.</param>
    /// <returns>The task to await.</returns>
    private async Task SendOAuthCardToObtainTokenAsync(ITurnContext context, CancellationToken cancellationToken)
    {
        TeamsChannelAccount account = await _teamsInfo.GetTeamsMemberAsync(context, context.Activity.From.Id, cancellationToken).ConfigureAwait(false);

        string loginHint = account.UserPrincipalName ?? "";
        if (String.IsNullOrEmpty(account.TenantId))
        {
            throw new ExceptionWithCode("Failed to get tenant id through bot framework.", ExceptionCode.ServiceError);
        }
        string tenantId = account.TenantId ?? "";
        SignInResource signInResource = GetSignInResource(loginHint, tenantId);

        // Ensure prompt initialized
        IMessageActivity prompt = Activity.CreateMessageActivity();
        prompt.Attachments = new List<Attachment>();
        prompt.Attachments.Add(new Attachment {
            ContentType = OAuthCard.ContentType,
            Content = new OAuthCard {
                Text = "Sign In",
                Buttons = new[]
                {
                            new CardAction
                            {
                                    Title = "Teams SSO Sign In",
                                    Value = signInResource.SignInLink,
                                    Type = ActionTypes.Signin,
                            },
                        },
                TokenExchangeResource = signInResource.TokenExchangeResource,
            },
        });
        // Send prompt
        await context.SendActivityAsync(prompt, cancellationToken).ConfigureAwait(false);
    }


    /// <summary>
    /// Get sign in authentication configuration
    /// </summary>
    /// <param name="loginHint">login hint</param>
    /// <param name="tenantId">tenant id</param>
    /// <returns>sign in resource</returns>
    private SignInResource GetSignInResource(string loginHint, string tenantId)
    {
        string signInLink = $"{_settings.BotAuthOptions.InitiateLoginEndpoint }?scope={Uri.EscapeDataString(string.Join(" ", _settings.Scopes))}&clientId={_settings.BotAuthOptions.ClientId}&tenantId={tenantId}&loginHint={loginHint}";

        SignInResource signInResource = new SignInResource {
            SignInLink = signInLink,
            TokenExchangeResource = new TokenExchangeResource {
                Id = Guid.NewGuid().ToString(),
                Uri = Regex.Replace(_settings.BotAuthOptions.ApplicationIdUri, @"/\/$/", "") + "/access_as_user"
            }
        };

        return signInResource;
    }

    /// <summary>
    /// Ensure bot is running in MS Teams since TeamsBotSsoPrompt is only supported in MS Teams channel.
    /// </summary>
    /// <param name="dc">dialog context</param>
    /// <exception cref="ExceptionCode.ChannelNotSupported"> if bot channel is not MS Teams </exception>
    private void EnsureMsTeamsChannel(DialogContext dc)
    {
        if (dc.Context.Activity.ChannelId != Channels.Msteams)
        {
            var errorMessage = "Teams Bot SSO Prompt is only supported in MS Teams Channel";
            throw new ExceptionWithCode(errorMessage, ExceptionCode.ChannelNotSupported);
        }
    }
}
