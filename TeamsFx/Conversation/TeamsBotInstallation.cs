﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.TeamsFx.Conversation
{
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;

    /// <summary>
    /// <para>
    /// An <see cref="INotificationTarget"/> that represents a bot installation. Teams Bot could be installed into:
    /// </para>
    /// <list type="bullet">
    ///     <item>
    ///         <description>Personal chat.</description>
    ///     </item>
    ///     <item>
    ///         <description>Group chat.</description>
    ///     </item>
    ///     <item>
    ///         <description>Team (by default the "General" channel).</description>
    ///     </item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// It's recommended to get bot installations from <c>ConversationBot.Notification.GetInstallationsAsync</c>.
    /// </remarks>
    public class TeamsBotInstallation : INotificationTarget
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="botAppId">The application ID of the bot.</param>
        /// <param name="adapter">The bot adapter.</param>
        /// <param name="conversationReference">The <see cref="ConversationReference"/> of the bot installation.</param>
        /// <exception cref="ArgumentNullException">Throws if provided parameter is null.</exception>
        /// <remarks>
        /// It's recommended to get bot installations from <c>ConversationBot.Notification.GetInstallationsAsync</c>.
        /// </remarks>
        public TeamsBotInstallation(string botAppId, BotAdapter adapter, ConversationReference conversationReference)
        {
            BotAppId = botAppId;
            Adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
            ConversationReference = conversationReference ?? throw new ArgumentNullException(nameof(conversationReference));
            Type = ConversationReference.GetTargetType();
        }

        /// <summary>
        /// The bot adapter.
        /// </summary>
        public BotAdapter Adapter { get; private set; }

        /// <summary>
        /// The application ID of the bot.
        /// </summary>
        public string BotAppId { get; private set; }

        /// <summary>
        /// The <see cref="ConversationReference"/> of the bot installation.
        /// </summary>
        public ConversationReference ConversationReference { get; private set; }

        /// <summary>
        /// The notification target type.
        /// <list type="bullet">
        ///     <item>
        ///         <description><see cref="NotificationTargetType.Channel"/> means bot is installed into a team and notification will be sent to its "General" channel.</description>
        ///     </item>
        ///     <item>
        ///         <description><see cref="NotificationTargetType.Group"/> means bot is installed into a group chat.</description>
        ///     </item>
        ///     <item>
        ///         <description><see cref="NotificationTargetType.Person"/> means bot is installed into a personal scope and notification will be sent to personal chat.</description>
        ///     </item>
        /// </list>
        /// </summary>
        public NotificationTargetType Type { get; private set; }

        /// <inheritdoc/>
        public async Task<MessageResponse> SendMessage(string message, CancellationToken cancellationToken = default)
        {
            var response = new MessageResponse();
            await Adapter.ContinueConversationAsync
            (
                BotAppId,
                ConversationReference,
                async (context, ct) => {
                    var res = await context.SendActivityAsync(message, cancellationToken: ct).ConfigureAwait(false);
                    response.Id = res?.Id;
                },
                cancellationToken
            ).ConfigureAwait(false);
            return response;
        }

        /// <inheritdoc/>
        public async Task<MessageResponse> SendAdaptiveCard(object card, CancellationToken cancellationToken = default)
        {
            var response = new MessageResponse();
            await Adapter.ContinueConversationAsync
            (
                BotAppId,
                ConversationReference,
                async (context, ct) => {
                    var res = await context.SendActivityAsync
                    (
                        MessageFactory.Attachment
                        (
                            new Attachment {
                                ContentType = "application/vnd.microsoft.card.adaptive",
                                Content = card,
                            }
                        ),
                        cancellationToken: ct
                    ).ConfigureAwait(false);
                    response.Id = res?.Id;
                },
                cancellationToken
            ).ConfigureAwait(false);
            return response;
        }

        /// <summary>
        /// Get channels from this bot installation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An array of channels if bot is installed into a team, otherwise returns an empty array.</returns>
        public async Task<Channel[]> GetChannelsAsync(CancellationToken cancellationToken = default)
        {
            var channels = new List<Channel>();
            if (Type != NotificationTargetType.Channel)
            {
                return channels.ToArray();
            }

            IList<ChannelInfo> teamsChannels = null;
            await Adapter.ContinueConversationAsync
            (
                BotAppId,
                ConversationReference,
                async (context, ct) => {
                    var teamId = context.GetTeamsBotInstallationId();
                    if (teamId != null)
                    {
                        teamsChannels = await TeamsInfo.GetTeamChannelsAsync(context, teamId, ct).ConfigureAwait(false);
                    }
                },
                cancellationToken
            ).ConfigureAwait(false);

            if (teamsChannels != null)
            {
                foreach (var teamChannel in teamsChannels)
                {
                    channels.Add(new Channel(this, teamChannel));
                }
            }

            return channels.ToArray();
        }

        /// <summary>
        /// Get members from this bot installation.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An array of members from where the bot is installed.</returns>
        public async Task<Member[]> GetMembersAsync(CancellationToken cancellationToken = default)
        {
            var members = new List<Member>();
            await Adapter.ContinueConversationAsync
            (
                BotAppId,
                ConversationReference,
                async (context, ct) => {
                    string continuationToken = null;
                    do
                    {
                        var pagedMembers = await TeamsInfo.GetPagedMembersAsync(context, null, continuationToken, ct).ConfigureAwait(false);
                        continuationToken = pagedMembers.ContinuationToken;
                        members.AddRange(pagedMembers.Members.Select(member => new Member(this, member)));
                    }
                    while (!string.IsNullOrEmpty(continuationToken));
                },
                cancellationToken
            ).ConfigureAwait(false);

            return members.ToArray();
        }

        /// <summary>
        /// Get team details from this bot installation
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The team details if bot is installed into a team, otherwise returns null.</returns>
        public async Task<TeamDetails> GetTeamDetailsAsync(CancellationToken cancellationToken = default)
        {
            if (Type != NotificationTargetType.Channel)
            {
                return null;
            }

            TeamDetails teamDetails = null;
            await Adapter.ContinueConversationAsync
            (
                BotAppId,
                ConversationReference,
                async (context, ct) => {
                    var teamId = context.GetTeamsBotInstallationId();
                    if (teamId != null)
                    {
                        teamDetails = await TeamsInfo.GetTeamDetailsAsync(context, teamId, ct).ConfigureAwait(false);
                    }
                },
                cancellationToken
            ).ConfigureAwait(false);

            return teamDetails;
        }
    }
}
