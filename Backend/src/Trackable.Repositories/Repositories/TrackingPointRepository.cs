// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Trackable.Common;
using Trackable.Common.Exceptions;
using Trackable.EntityFramework;
using Trackable.Models;

namespace Trackable.Repositories
{
    internal class TrackingPointRepository : DbRepositoryBase<int, TrackingPointData, TrackingPoint>, ITrackingPointRepository
    {
        public TrackingPointRepository(TrackableDbContext db, IMapper mapper)
            : base(db, mapper)
        {
        }

        public override async Task<TrackingPoint> AddAsync(TrackingPoint model)
        {
            var device = await this.Db.TrackingDevices
                .Where(d => !d.Deleted)
                .Include(d => d.Asset.LatestPosition)
                .Include(d => d.LatestPosition)
                .SingleAsync(d => d.Id == model.TrackingDeviceId);

            if (device.Asset == null)
            {
                throw new InvalidOperationException("Can't add a tracking point while device not linked to an asset");
            }

            model.AssetId = device.Asset.Id;

            return await this.AddAsyncInternal(model, device);
        }

        public override async Task<IEnumerable<TrackingPoint>> AddAsync(IEnumerable<TrackingPoint> models)
        {
            if (!models.Any())
            {
                return new List<TrackingPoint>();
            }

            var devicePointsLookup = models.ToLookup(m => m.TrackingDeviceId);
            var deviceIds = devicePointsLookup.Select(g => g.Key).ToList();

            var devicesDictionary = await this.Db.TrackingDevices
                   .Where(d => !d.Deleted && deviceIds.Contains(d.Id))
                   .Include(d => d.Asset.LatestPosition)
                   .Include(d => d.LatestPosition)
                   .ToDictionaryAsync(d => d.Id, d => d);

            if (devicesDictionary.Count < devicePointsLookup.Count)
            {
                throw new BadArgumentException("A Device Id does not exist");
            }

            if (devicesDictionary.Any(d => d.Value.Asset == null))
            {
                throw new BadArgumentException("A Device is not linked to an asset");
            }

            var savedModels = new List<TrackingPoint>();
            foreach (var dpl in devicePointsLookup)
            {
                var device = devicesDictionary[dpl.Key];

                // All points should have the asset id they were assigned to
                dpl.ForEach(model => model.AssetId = device.Asset.Id);

                // Save all the points except the latest
                var orderedModels = dpl.OrderBy(p => p.DeviceTimestampUtc);
                savedModels.AddRange(await base.AddAsync(orderedModels.Take(dpl.Count() - 1)));

                // Update the current position using the latest point
                var latestPoint = await this.AddAsyncInternal(orderedModels.Last(), device);
                savedModels.Add(latestPoint);
            }

            return savedModels;
        }

        public async Task<IEnumerable<TrackingPoint>> GetByAssetIdAsync(string assetId)
        {
            var data = await this
                .FindBy(a => a.AssetId == assetId)
                .ToListAsync();
            return data.Select(d => this.ObjectMapper.Map<TrackingPoint>(d));
        }

        public async Task<IEnumerable<TrackingPoint>> GetByDeviceIdAsync(string deviceId)
        {
            var data = await this
                .FindBy(a => a.TrackingDeviceId == deviceId)
                .ToListAsync();
            return data.Select(d => this.ObjectMapper.Map<TrackingPoint>(d));
        }

        public async Task<IEnumerable<TrackingPoint>> GetByAssetIdAfterDateAsync(string assetId, DateTime date, bool includeDebug)
        {
            var data = await this
                .FindBy(a => a.CreatedAtTimeUtc > date && a.AssetId == assetId && (includeDebug || !includeDebug && !a.Debug))
                .ToListAsync();
            return data.Select(d => this.ObjectMapper.Map<TrackingPoint>(d));
        }

        public async Task<TrackingPoint> GetByAssetIdLastLabeledAsync(string assetId)
        {
            var data = await this
                .FindBy(a => a.TripId != null && a.AssetId == assetId)
                .OrderByDescending(a => a.CreatedAtTimeUtc)
                .FirstOrDefaultAsync();
            return this.ObjectMapper.Map<TrackingPoint>(data);
        }

        public async Task<IEnumerable<TrackingPoint>> GetNearestPoints(int tripId, IPoint point, int count)
        {
            var data = await this.FindBy(a => a.TripId == tripId)
                .OrderBy(p => p.Location.Distance(
                        DbGeography.PointFromText("POINT(" + point.Longitude + " " + point.Latitude + ")", 4326)))
                .Take(count)
                .ToListAsync();
            return data.Select(d => this.ObjectMapper.Map<TrackingPoint>(d));
        }

        public async Task AssignPointsToTripAsync(int tripId, IEnumerable<TrackingPoint> points)
        {
            var dataPoints = new List<TrackingPointData>();
            foreach (var p in points)
            {
                p.TripId = tripId;
                dataPoints.Add(this.ObjectMapper.Map<TrackingPointData>(p));
            }

            await this.Db.UpdateAsync(dataPoints, "TripId");
        }

        public async Task<IDictionary<string, int>> PointsPerAssetCountAsync()
        {
            return await this.FindBy(a => true)
                .GroupBy(p => p.AssetId)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<IDictionary<DateTime, int>> PointsPerDayCountAsync()
        {
            return await this.FindBy(a => true)
                .GroupBy(p => DbFunctions.TruncateTime(p.CreatedAtTimeUtc))
                .ToDictionaryAsync(g => g.Key.Value, g => g.Count());
        }

        protected override Expression<Func<TrackingPointData, object>>[] Includes => new Expression<Func<TrackingPointData, object>>[]
        {
            data => data.Asset
        };

        private async Task<TrackingPoint> AddAsyncInternal(TrackingPoint trackingPoint, TrackingDeviceData deviceData)
        {
            var data = this.ObjectMapper.Map<TrackingPointData>(trackingPoint);

            var addedData = this.Db.TrackingPoints.Add(data);

            this.Db.TrackingDevices.Attach(deviceData);

            var deviceLatestPointTime = deviceData?.LatestPosition?.DeviceTimestampUtc;
            if (!deviceLatestPointTime.HasValue || trackingPoint.DeviceTimestampUtc > deviceLatestPointTime.Value)
            {
                deviceData.LatestPosition = addedData;
            }

            var assetLatestPointTime = deviceData?.Asset?.LatestPosition?.DeviceTimestampUtc;
            if (!assetLatestPointTime.HasValue || trackingPoint.DeviceTimestampUtc > assetLatestPointTime.Value)
            {
                deviceData.Asset.LatestPosition = addedData;
            }

            await this.Db.SaveChangesAsync();

            return this.ObjectMapper.Map<TrackingPoint>(addedData);
        }
    }
}
