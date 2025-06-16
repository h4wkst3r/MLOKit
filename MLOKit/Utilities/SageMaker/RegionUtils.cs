using System;
using Amazon;

namespace MLOKit.Utilities.SageMaker
{
    internal class RegionUtils
    {

        public static RegionEndpoint getRegionEndpoint(string region)
        {
            if (string.IsNullOrWhiteSpace(region))
            {
                throw new ArgumentException("Region cannot be null or empty.", nameof(region));
            }

            var endpoint = RegionEndpoint.GetBySystemName(region);

            if (endpoint == null || endpoint.SystemName != region)
            {
                throw new ArgumentException($"Invalid AWS region: {region}", nameof(region));
            }

            return endpoint;
        }

    }
}
