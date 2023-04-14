using VaquinhaAnimal.Api.Configuration;
using VaquinhaAnimal.Api.Data;
using VaquinhaAnimal.Api.Extensions;
using VaquinhaAnimal.Data.Context;
using VaquinhaAnimal.Data.Repository;
using VaquinhaAnimal.Domain.Entities.Base;
using VaquinhaAnimal.Domain.Interfaces;
using VaquinhaAnimal.Domain.Notificacoes;
using VaquinhaAnimal.Domain.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace VaquinhaAnimal.Api.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection ResolveDependencies(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<VaquinhaDbContext>();

            // SERVICES
            services.AddScoped<ICartaoService, CartaoService>();
            services.AddScoped<ISuporteService, SuporteService>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<ICampanhaService, CampanhaService>();
            services.AddScoped<IContaDepositoService, ContaDepositoService>();
            services.AddScoped<IDoacaoService, DoacaoService>();
            services.AddScoped<IImagemService, ImagemService>();
            services.AddScoped<IRedeSocialService, RedeSocialService>();
            services.AddScoped<IUsuarioService, UsuarioService>();

            // REPOSITORIES
            services.AddScoped<ICartaoRepository, CartaoRepository>();
            services.AddScoped<ISuporteRepository, SuporteRepository>();
            services.AddScoped<ICampanhaRepository, CampanhaRepository>();
            services.AddScoped<IContaDepositoRepository, ContaDepositoRepository>();
            services.AddScoped<IDoacaoRepository, DoacaoRepository>();
            services.AddScoped<IImagemRepository, ImagemRepository>();
            services.AddScoped<IRedeSocialRepository, RedeSocialRepository>();
            services.AddScoped<IIdentityRepository, IdentityRepository>();
            
            services.AddScoped<ISignalR, SignalRHub>(); // SIGNAL R
            services.AddScoped<INotificador, Notificador>();
            services.AddScoped<IUser, AspNetUser>();
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            return services;
        }
    }
}
