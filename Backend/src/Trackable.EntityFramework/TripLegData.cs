// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace Trackable.EntityFramework
{
    [Table("TripLegs")]
    public class TripLegData : EntityBase<int>
    {
        public DateTime StartTimeUtc { get; set; }

        public DateTime EndTimeUtc { get; set; }
        
        public DbGeography Route { get; set; }

        public double AverageSpeed { get; set; }

        public int TripDataId { get; set; }

        public TripData Trip { get; set; }

        public string StartLocationId { get; set; }

        public string EndLocationId { get; set; }

        public LocationData StartLocation { get; set; }

        public LocationData EndLocation { get; set; }
    }
}