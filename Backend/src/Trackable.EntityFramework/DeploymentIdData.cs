// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trackable.EntityFramework
{
    [Table("DeploymentId")]
    public class DeploymentIdData : EntityBase<Guid>
    {
    }
}
