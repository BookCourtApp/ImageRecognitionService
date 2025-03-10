using Microsoft.EntityFrameworkCore;
using Serilog;
using Nest;

using Infrastructure;
using Infrastructure.DataAccessLayer;
using Core.Repository;
using BusinessLogic;

Log.Logger = new LoggerConfiguration()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog(
        (hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
        }
    );

    Log.Information("Starting web application");

    // Add services to the container.
    builder.Services.AddDbContextFactory<ApplicationContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("ConnectionString"), b => b.MigrationsAssembly("Api")));

    builder.Services.AddDbContextFactory<BooksContext>(options =>
        options.UseSqlite("Data Source=/home/alex/Documents/projects/csharp/ImageRecognitionService/Core/LABIRINT_BOOK24.db"));

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddScoped<IRepository, MakrupRepository>();
    builder.Services.AddScoped<LearningManager>();
    builder.Services.AddScoped<IElasticClient>(sp =>
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200")).DefaultIndex("books");
            return new ElasticClient(settings);
        }
    );

    builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAllOrigins",
                builder =>
                {
                    builder.WithOrigins("http://localhost:3000",
                    "https://cea5-176-59-134-95.ngrok-free.app")
                        .AllowCredentials()
                        .AllowAnyHeader()
                        .AllowAnyMethod();

                }
            );
        }
    );

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseCors(policy => policy.AllowAnyHeader()
        .AllowAnyMethod()
        .WithOrigins("http://localhost:3000", "https://cea5-176-59-134-95.ngrok-free.app")
        .AllowCredentials());

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch(Exception ex){
    Log.Fatal(ex, $"Fatal error: {ex.Message}");
}
finally{
    Log.CloseAndFlush();
}
