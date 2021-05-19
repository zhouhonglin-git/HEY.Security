using EasyCaching.Core;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.JWT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestRedisController : Controller
    {
        private readonly IRedisCachingProvider _redisCaching;
        public TestRedisController(IRedisCachingProvider redisCaching)
        {

            _redisCaching = redisCaching;
        }
        [HttpGet]
        public string Get()
        {
            return _redisCaching.StringGet("TestRedis");
        }
        [HttpPost]
        public void Post()
        {
            _redisCaching.StringSet("TestRedis", "Test");
        }
    }
}
