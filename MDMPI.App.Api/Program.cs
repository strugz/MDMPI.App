using MDMPI.App.Core.Collection.Interfaces;
using MDMPI.App.Core.Collection.Services;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Core.Logistic.Services;
using MDMPI.App.Data;
using MDMPI.App.Data.Collection.Repositories;
using MDMPI.App.Data.Common.Repositories;
using MDMPI.App.Data.Common.Services;
using MDMPI.App.Data.Logistic.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";

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

// ── Repositories (Infrastructure) ──
builder.Services.AddScoped<IRequestRepository, RequestRepository>();
builder.Services.AddScoped<IMobileRepository, MobileRepository>();
builder.Services.AddScoped<IRequestPullOutReturnPickUpRepository, RequestPullOutReturnPickUpRepository>();
builder.Services.AddScoped<IRequestIdGenerator, RequestIdGenerator>();
builder.Services.AddScoped<IBackloadIdGenerator, BackloadIdGenerator>();
builder.Services.AddScoped<IItemIdGenerator, ItemIdGenerator>();
builder.Services.AddScoped<IBatchIdGenerator, BatchIdGenerator>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<IRequestRemarksRepository, RequestRemarksRepository>();
builder.Services.AddScoped<IImagePathTypeRepository, ImagePathTypeRepository>();
builder.Services.AddScoped<IRequestPickUpRepository, RequestPickUpRepository>();
builder.Services.AddScoped<IRequestAirSeaRepository, RequestAirSeaRepository>();
builder.Services.AddScoped<IRequestBackloadRepository, RequestBackloadRepository>();
builder.Services.AddScoped<ICollectionTransactionDetailsRepository, CollectionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// ── Services (Use Cases / Application Layer) ──
builder.Services.AddScoped<IRequestService, RequestService>();
builder.Services.AddScoped<IRequestPickUpService, RequestPickUpService>();
builder.Services.AddScoped<IRequestPullOutReturnPickUpService, RequestPullOutReturnPickUpService>();
builder.Services.AddScoped<IRequestAirSeaService, RequestAirSeaService>();
builder.Services.AddScoped<IRequestBackloadService, RequestBackloadService>();
builder.Services.AddScoped<IRemarksService, RemarksService>();
builder.Services.AddScoped<IImageService, ImageUploadService>();
builder.Services.AddScoped<IMobileService, MobileService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IItemService, MDMPI.App.Core.Common.Services.ItemService>();
builder.Services.AddScoped<ICollectionTransactionService, CollectionTransactionService>();


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll",
//        policy => policy
//            .AllowAnyOrigin()
//            .AllowAnyMethod()
//            .AllowAnyHeader());
//});

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

//app.UseCors("AllowAll");

//app.Run($"http://0.0.0.0:{port}");

app.Run();
