// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated />
namespace Trackable.EntityFramework.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    [GeneratedCode("EntityFramework.Migrations", "6.1.3-40302")]
    public sealed partial class RenameDestinationToLocation : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(RenameDestinationToLocation));
        
        string IMigrationMetadata.Id
        {
            get { return "201702120952233_RenameDestinationToLocation"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}