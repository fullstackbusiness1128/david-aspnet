using CMS.Models.BusinessModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace CMS.Services.Cache
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILogger<CacheService> _loggerN;

        public CacheService(IDistributedCache distributedCache, IHttpContextAccessor httpContextAccessor, ILogger<CacheService> logger)
        {
            _distributedCache = distributedCache;
            _accessor = httpContextAccessor;
            _loggerN = logger;
        }


    }
}
