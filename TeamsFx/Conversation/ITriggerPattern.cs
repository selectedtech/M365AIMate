﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.TeamsFx.Conversation
{
    /// <summary>
    /// Defines a contract that represents a trigger used to trigger command handler.
    /// </summary>
    public interface ITriggerPattern
    {
        /// <summary>
        /// Check whether the trigger pattern is matched.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>True if the trigger can be fired, otherwise false.</returns>
        bool ShouldTrigger(string input);
    }
}
