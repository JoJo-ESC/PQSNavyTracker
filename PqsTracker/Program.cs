using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PqsTracker.Data;
using PqsTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    // System.Text.Json serializes enums as integers by default; this makes
    // the API accept/return "Fundamentals" instead of 0 in JSON payloads.
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddDbContext<PqsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IQualificationService, QualificationService>();
builder.Services.AddScoped<ITraineeService, TraineeService>();
builder.Services.AddScoped<ISignOffService, SignOffService>();
builder.Services.AddScoped<IProgressService, ProgressService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply any pending EF Core migrations, then seed sample data if the
// database is empty. Top-level statements like this one support `await`
// directly — the compiler generates an async Main() behind the scenes.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PqsDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.SeedAsync(db);
}

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
