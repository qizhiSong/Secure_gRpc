﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IdentityServer4.AccessTokenValidation;

namespace SecureGrpc.Server
{
    public class Startup
    {
        private string stsServer = "https://localhost:44352";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("protectedScope", policy =>
                 { policy.RequireClaim("scope", "grpc_protected_scope"); });
            });
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = stsServer;
                    options.ApiName = "ProtectedGrpc";
                    options.ApiSecret = "grpc_protected_secret";
                    options.RequireHttpsMetadata = false;
                });

            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
            });

            services.AddMvc()
               .AddNewtonsoftJson();

            services.AddHttpContextAccessor();
            services.AddSingleton<ServerGrpcSubscribers>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>().RequireAuthorization("protectedScope");
                endpoints.MapGrpcService<DuplexService>().RequireAuthorization("protectedScope");
                endpoints.MapRazorPages();
            });

        }
    }
}
