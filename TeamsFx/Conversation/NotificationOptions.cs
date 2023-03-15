﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.TeamsFx.Conversation
{
    /// <summary>
    /// Options to initialize <see cref="NotificationBot"/>.
    /// </summary>
    public class NotificationOptions
    {
        /// <summary>
        /// The application ID of the bot.
        /// </summary>
        public string BotAppId { get; set; } = string.Empty;

        /// <summary>
        /// An optional storage to persist bot notification connections.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <c>Storage</c> is not provided, a default local file storage will be used, which stores notification connections into:
        /// </para>
        /// <list type="bullet">
        ///     <item>
        ///         <description>"{$env:TEAMSFX_NOTIFICATION_LOCALSTORE_DIR}/.notification.localstore.json" if running locally.</description>
        ///     </item>
        ///     <item>
        ///         <description>"{$env:TEMP}/.notification.localstore.json" if {$env:RUNNING_ON_AZURE} is set to "1".</description>
        ///     </item>
        ///     <item>
        ///         <description>"{<see cref="Environment.CurrentDirectory"/>}/.notification.localstore.json" if all above environment variables are not set.</description>
        ///     </item>
        /// </list>
        /// <para>
        /// It's recommended to use your own shared storage for production environment.
        /// </para>
        /// </remarks>
        public INotificationTargetStorage Storage { get; set; }
    }
}
