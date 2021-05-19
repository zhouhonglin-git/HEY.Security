using EasyCaching.Core;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.JWT.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtSetting _jwtSetting;
        private readonly UserStore _userStore;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
        private readonly IRedisCachingProvider _redisCaching;

        public AccountController(IOptions<JwtSetting> jwtSetting
            , UserStore userStore
            , JwtSecurityTokenHandler jwtSecurityTokenHandler
            , IRedisCachingProvider redisCaching)
        {
            _jwtSetting = jwtSetting.Value;
            _userStore = userStore;
            _jwtSecurityTokenHandler = jwtSecurityTokenHandler;
            _redisCaching = redisCaching;
        }
        public SignInOutput SignIn()
        {
            var user = _userStore.FindUser("bob", "bob");
            var accountToken = GenerateAccountToken(user);
            var refreshToken = GenerateRefreshToken(user.Id.ToString());
            var result = new SignInOutput
            {
                AccountToken = accountToken,
                RefreshToken = refreshToken
            };
            //如果当前用户已经存在则清除
            if (_redisCaching.HExists("token", user.Id.ToString())) _redisCaching.HDel("token", new List<string> { user.Id.ToString() });
            //token存入redis
            if (!_redisCaching.HSet("token", user.Id.ToString(), JsonConvert.SerializeObject(result))) throw new Exception("登录失败");
            return result;
        }
        public string RefreshToken(string refreshToken)
        {
            //判断是否为有效的jwttoken
            if (!_jwtSecurityTokenHandler.CanReadToken(refreshToken)) throw new Exception("无效refresh_token");
            var jwtSecurityToken = _jwtSecurityTokenHandler.ReadJwtToken(refreshToken);
            var exp = jwtSecurityToken.Payload.Exp;
            var nbf = jwtSecurityToken.Payload.Nbf;
            if (!nbf.HasValue || !exp.HasValue) throw new Exception("refresh_token已过期");
            var now = UnixTimestamp.GetUnixTimeStamp(DateTime.Now);
            // 判断 生效时间 和 过期时间
            if (now > exp.Value || now < nbf.Value) throw new Exception("refresh_token已过期");
            var sub = jwtSecurityToken.Payload.Sub;
            if (string.IsNullOrWhiteSpace(sub)) throw new Exception("无效refresh_token");
            var user = _userStore.FindUser(int.Parse(sub)) ?? throw new Exception("无效refresh_token");
            return GenerateAccountToken(user);
        }
        [Authorize]
        [HttpGet]
        public void SignOut()
        {
            var sub = User.FindFirst(JwtClaimTypes.Subject) ?? throw new ArgumentNullException();
            var token = _redisCaching.HDel("token", new List<string> { sub.Value });
        }
        /// <summary>
        /// 生成account_token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private string GenerateAccountToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSetting.SecurityKey);
            DateTime utcNow = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
               {
                    new Claim(JwtClaimTypes.Issuer,_jwtSetting.Issuer),
                    new Claim(JwtClaimTypes.Audience,_jwtSetting.Audience),
                    new Claim(JwtClaimTypes.Subject, user.Id.ToString()),
                    new Claim(JwtClaimTypes.Name, user.Name),
                    new Claim(JwtClaimTypes.Email, user.Email),
                    new Claim(JwtClaimTypes.PhoneNumber, user.PhoneNumber),

               }),
                NotBefore = utcNow, //NotBefore Token生效时间，在此之前不可用
                Expires = utcNow.AddMinutes(5), //Expiration Token过期时间，在此之后不可用
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = _jwtSecurityTokenHandler.CreateToken(tokenDescriptor);
            return _jwtSecurityTokenHandler.WriteToken(token);
        }
        /// <summary>
        /// 生成refresh_token
        /// </summary>
        /// <param name="sub"></param>
        /// <returns></returns>
        private string GenerateRefreshToken(string sub)
        {
            DateTime utcNow = DateTime.UtcNow;
            var key = Encoding.ASCII.GetBytes(_jwtSetting.SecurityKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
               {
                    new Claim(JwtClaimTypes.Issuer,"refresh"),
                    new Claim(JwtClaimTypes.Subject, sub)
               }),
                NotBefore = utcNow, //NotBefore Token生效时间，在此之前不可用
                Expires = utcNow.AddMinutes(60), //Expiration Token过期时间，在此之后不可用
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = _jwtSecurityTokenHandler.CreateToken(tokenDescriptor);
            return _jwtSecurityTokenHandler.WriteToken(token);
        }

    }
}
