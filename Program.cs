using System;
using System.IO;
using ImageApi.Services;

namespace ImageApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpClient();
        builder.Services.AddScoped<IImageService, ImageService>();

        var app = builder.Build();

        var dir = Environment.CurrentDirectory;
        bool imageDirectoryExists = Directory.Exists(Path.Combine(dir, "images"));
        if (!imageDirectoryExists)
            Directory.CreateDirectory(Path.Combine(dir, "images"));

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
