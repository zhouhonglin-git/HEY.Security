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
            //Ϊ�˱�֤JwtClaimTypes.Subject������ȷ ��������������*
            System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.EventsType = typeof(CustomJwtEvents); //�����Զ������֤�Լ�ҵ���߼�
                x.RequireHttpsMetadata = false;//�Ƿ���https Ĭ�� true 
                //x.SaveToken = true; //����Ϊtrue ͨ�� await HttpContext.GetTokenAsync("Bearer", "access_token");  ��ȡ token
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role,
                    ValidateIssuer = true, //�Ƿ���֤�䷢��
                    ValidIssuer = jwtSetting.Issuer, //�䷢��
                    ValidateAudience = true, //�Ƿ���֤������
                    ValidAudience = jwtSetting.Audience, //������
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSetting.SecurityKey)),
                    ClockSkew = TimeSpan.Zero //����У�����ʱ���ƫ����
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
