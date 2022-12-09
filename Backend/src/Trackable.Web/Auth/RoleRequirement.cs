// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authorization;
using System;

namespace Trackable.Web.Auth
{
    public class RoleRequirement : Attribute, IAuthorizationRequirement
    {
        public string Role { get; }

        public RoleRequirement(string role)
        {
            this.Role = role;
        }
    }
}
