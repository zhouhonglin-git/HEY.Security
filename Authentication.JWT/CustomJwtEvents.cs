using EasyCaching.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Authentication.JWT
{
    public class CustomJwtEvents : JwtBearerEvents
    {
        private readonly IRedisCachingProvider _redisCaching;
        public CustomJwtEvents(IRedisCachingProvider redisCaching)
        {
            _redisCaching = redisCaching;
        }

        public override async Task AuthenticationFailed(AuthenticationFailedContext context) => await base.AuthenticationFailed(context);
        public override async Task Challenge(JwtBearerChallengeContext context) => await base.Challenge(context);
        public override async Task Forbidden(ForbiddenContext context) => await base.Forbidden(context);
        public override async Task MessageReceived(MessageReceivedContext context) => await base.MessageReceived(context);
        public override async Task TokenValidated(TokenValidatedContext context)
        {
            var jwtSecurityToken = context.SecurityToken as JwtSecurityToken;
            var token = _redisCaching.HGet("token", jwtSecurityToken.Subject);
            //如果redis中没有此用户 验证失败
            if (string.IsNullOrWhiteSpace(token))
            {
                context.Fail(string.Empty);
                context.Response.OnStarting(async () =>
                {
                    //context.Response.StatusCode = 401;
                    context.Response.ContentType = "text/plain;charset=utf-8";
                    await context.Response.WriteAsync("该Token已失效，请重新登陆！");
                });
            }
            await base.TokenValidated(context);
        }
    }
}
