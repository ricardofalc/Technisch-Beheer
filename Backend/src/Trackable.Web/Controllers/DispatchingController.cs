// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trackable.Services;

namespace Trackable.Web.Controllers
{
    [Route("api/dispatching")]
    public class DispatchingController : ControllerBase
    {
        private readonly IDispatchingService dispatchingService;
        private readonly IAssetService assetService;

        public DispatchingController(
            ILoggerFactory loggerFactory,
            IDispatchingService dispatchingService,
            IAssetService assetService)
            : base(loggerFactory)
        {
            this.dispatchingService = dispatchingService;
            this.assetService = assetService;
        }

        /// <summary>
        /// Dispatch according to route
        /// </summary>
        /// <param name="dispatchingParameters">The parameters required for dispatching</param>
        /// <returns>Route results</returns>
        // Post api/dispatching
        [HttpPost]
        public async Task<IEnumerable<DispatchingResults>> Post([FromBody]DispatchingParameters dispatchingParameters)
        {
            var asset = await this.assetService.GetAsync(dispatchingParameters.AssetID);

            return await this.dispatchingService.CallRoutingAPI(dispatchingParameters, asset.AssetProperties);
        }
    }
}
