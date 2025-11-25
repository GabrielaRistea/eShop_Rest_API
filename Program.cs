
using Microsoft.EntityFrameworkCore;
using Proiect.Context;
using Proiect.Repositories.Interfaces;
using Proiect.Repositories;
using Proiect.Services.Interfaces;
using Proiect.Services;

namespace Proiect
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            //builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<ShopContext>(
            optionsBuilder =>
                optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("EShopDb"))
        );

            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                //app.MapOpenApi();
            }

            //// Configure the HTTP request pipeline.
            //if (app.Environment.IsDevelopment())
            //{
            //    app.MapOpenApi();
            //}

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseDefaultFiles(); // cauta index.html
            app.UseStaticFiles();  // permite accesul la folderul wwwroot

            app.MapControllers();

            app.Run();
        }
    }
}
