using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Data;
using MDMPI.App.Data.Common;
using MDMPI.App.Data.Common.Services;
using MDMPI.App.Data.Logistic.Repositories;
using Microsoft.EntityFrameworkCore;
using MDMPI.App.Data.Common.Repositories;
using MDMPI.App.Data.Common.Repositories; // for CategoryRepository

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DB")));

// Register application services
builder.Services.AddSingleton<ImageService>();

builder.Services.AddScoped<IRequestRepository, RequestRepository>();

builder.Services.AddScoped<IMobileRepository, MobileRepository>();

builder.Services.AddScoped<IRequestPullOutReturnPickUpRepository, RequestPullOutReturnPickUpRepository>();

builder.Services.AddScoped<IRequestIdGenerator, RequestIdGenerator>();

builder.Services.AddScoped<IRequestRemarksRepository, RequestRemarksRepository>();

builder.Services.AddScoped<IImagePathTypeRepository, ImagePathTypeRepository>();

builder.Services.AddScoped<IRequestPickUpRepository, RequestPickUpRepository>();

builder.Services.AddScoped<IRequestAirSeaRepository, RequestAirSeaRepository>();

// register category repository
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

var app = builder.Build();

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
