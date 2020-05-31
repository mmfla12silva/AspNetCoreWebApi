using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using MimicApi.Database;
using MimicApi.Helpers;
using MimicApi.Helpers.Swagger;
using MimicApi.V1.Repository;
using MimicApi.V1.Repository.Interfaces;

namespace MimicApi
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            #region AutoMapper - Configuração
            var config = new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(new DTOMapperProfile());
                }
            );

            IMapper mapper = config.CreateMapper();
            services.AddSingleton(mapper);
            #endregion

            services.AddDbContext<MimicContext>(opt =>
            {
                opt.UseSqlite("Data Source=Database\\Mimic.db");
            });
            services.AddMvc();
            services.AddScoped<IPalavraRepository, PalavraRepository>();
            services.AddApiVersioning(cfg =>
            {
                cfg.ReportApiVersions = true;
                cfg.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
            });

            services.AddSwaggerGen(cfg =>
            {
                cfg.ResolveConflictingActions(apiDescription =>
                    apiDescription.First()
                    );
                cfg.SwaggerDoc("v2.0", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "Mimic Api - 2.0",
                    Version = "v2.0"
                });
                cfg.SwaggerDoc("v1.1", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "Mimic Api - 1..",
                    Version = "v1.1"
                });
                cfg.SwaggerDoc("v1.0", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "Mimic Api - 1.0",
                    Version = "v1.0"
                });

                var caminhoProjeto = PlatformServices.Default.Application.ApplicationBasePath;
                var nomeProjeto = $"{PlatformServices.Default.Application.ApplicationName}MimicApi.xml";
                var caminhoArquivoXml = Path.Combine(caminhoProjeto, nomeProjeto);

                caminhoArquivoXml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                cfg.IncludeXmlComments(caminhoArquivoXml);

                cfg.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var actionApiVersionModel = apiDesc.ActionDescriptor?.GetApiVersion();
                    // would mean this action is unversioned and should be included everywhere
                    if (actionApiVersionModel == null)
                    {
                        return true;
                    }
                    if (actionApiVersionModel.DeclaredApiVersions.Any())
                    {
                        return actionApiVersionModel.DeclaredApiVersions.Any(v => $"v{v.ToString()}" == docName);
                    }
                    return actionApiVersionModel.ImplementedApiVersions.Any(v => $"v{v.ToString()}" == docName);
                });

                cfg.OperationFilter<ApiVersionOperationFilter>();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStatusCodePages();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(cfg =>
            {
                cfg.SwaggerEndpoint("/swagger/v2.0/swagger.json", "Mimic Api V2.0");
                cfg.SwaggerEndpoint("/swagger/v1.1/swagger.json", "Mimic Api V1.1");
                cfg.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Mimic Api V1.0");
                cfg.RoutePrefix = String.Empty;
            });
        }
    }
}
