using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<DataContext>(option => 
            {
                option.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });   

            services.AddCors();
            services.AddScoped<IToken, TokenService>();
            services.AddScoped<IUserRepository, UserRepository>();
           // services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);


            return services;
        }
        
    }
}