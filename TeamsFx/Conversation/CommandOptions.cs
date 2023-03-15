﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.TeamsFx.Conversation
{
    /// <summary>
    /// Options to initialize a <see cref="CommandBot"/>.
    /// </summary>
    public class CommandOptions
    {
        /// <summary>
        /// Gets or sets a set of command handlers used to process commands for this bot.
        /// </summary>
        /// <value>
        /// The command handler used to process command.
        /// </value>
        public IList<ITeamsCommandHandler> Commands { get; set; }
    }
}
