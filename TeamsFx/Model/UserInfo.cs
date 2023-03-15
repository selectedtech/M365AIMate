﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
namespace Microsoft.TeamsFx;

/// <summary>
/// UserInfo with user displayName, objectId and preferredUserName.
/// </summary>
public class UserInfo
{
    /// <summary>
    /// User Display Name.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// User unique reference within the Azure Active Directory domain.
    /// </summary>
    public string PreferredUserName { get; set; }

    /// <summary>
    /// Usually be the email address.
    /// </summary>
    public string ObjectId { get; set; }
}
