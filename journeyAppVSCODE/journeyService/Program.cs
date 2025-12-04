using journeyService.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ParamsSetting>
    (
        builder.Configuration.GetSection("Params")
    );

builder.Services.Configure<DataBaseSetting>
    (
        builder.Configuration.GetSection(DataBaseSetting.SectionName)
    );


var EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

//Debug.WriteLine("EnvironmentName");

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();

    app.UseStaticFiles();
    app.UseDefaultFiles();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
