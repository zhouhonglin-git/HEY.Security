using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.JWT
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var jwtSetting = new JwtSetting();
            Configuration.Bind("JwtSetting", jwtSetting);
            services.Configure<JwtSetting>(Configuration.GetSection("JwtSetting"));
            services.AddControllers();
            services.AddTransient<CustomJwtEvents>();
            services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, Permission.PermissionAuthorizationHandler>();
            //为了保证JwtClaimTypes.Subject解析正确 必须加上下面这句*
            System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.EventsType = typeof(CustomJwtEvents); //设置自定义的验证以及业务逻辑
                x.RequireHttpsMetadata = false;//是否开启https 默认 true 
                //x.SaveToken = true; //设置为true 通过 await HttpContext.GetTokenAsync("Bearer", "access_token");  获取 token
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role,
                    ValidateIssuer = true, //是否验证颁发者
                    ValidIssuer = jwtSetting.Issuer, //颁发者
                    ValidateAudience = true, //是否验证接收者
                    ValidAudience = jwtSetting.Audience, //接收者
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSetting.SecurityKey)),
                    ClockSkew = TimeSpan.Zero //代表校验过期时间的偏移量
                };
            });
            services.AddEasyCaching(options =>
            {
                options.UseRedis(x =>
                {
                    x.DBConfig.Endpoints.Add(new EasyCaching.Core.Configurations.ServerEndPoint("121.4.77.91", 6379));
                    x.DBConfig.Password = "Zhl123456";
                }).WithMessagePack();
            });
            services.AddSingleton<UserStore>();
            services.AddTransient<System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseAuthentication();
        }
    }
}
