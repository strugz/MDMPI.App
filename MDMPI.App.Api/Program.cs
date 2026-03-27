using MDMPI.App.Core.Collection.Interfaces;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Data;
using MDMPI.App.Data.Collection.Repositories;
using MDMPI.App.Data.Common.Repositories;
using MDMPI.App.Data.Common.Services;
using MDMPI.App.Data.Logistic.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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

// Register Gemini AI service
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection(GeminiSettings.SectionName));
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

builder.Services.AddScoped<IRequestRepository, RequestRepository>();

builder.Services.AddScoped<IMobileRepository, MobileRepository>();

builder.Services.AddScoped<IRequestPullOutReturnPickUpRepository, RequestPullOutReturnPickUpRepository>();

builder.Services.AddScoped<IRequestIdGenerator, RequestIdGenerator>();

// Register item & batch id generators
builder.Services.AddScoped<IItemIdGenerator, ItemIdGenerator>();
builder.Services.AddScoped<IBatchIdGenerator, BatchIdGenerator>();

// Register item repository
builder.Services.AddScoped<IItemRepository, ItemRepository>();

builder.Services.AddScoped<IRequestRemarksRepository, RequestRemarksRepository>();

builder.Services.AddScoped<IImagePathTypeRepository, ImagePathTypeRepository>();

builder.Services.AddScoped<IRequestPickUpRepository, RequestPickUpRepository>();

builder.Services.AddScoped<IRequestAirSeaRepository, RequestAirSeaRepository>();

// Register application for Collection Module

builder.Services.AddScoped<ICollectionTransactionDetailsRepository, CollectionRepository>();

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
