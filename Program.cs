using Backend.data;
using Backend.interfaces;
using Backend.repositories;
using Backend.services;
using Backend.services.AI;
using Backend.services.Mapping;
using Backend.services.Middleware;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//~ Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

//~ Register DeepSeek configuration
builder.Services.Configure<DeepseekConfig>(builder.Configuration.GetSection("DeepSeek"));

//~ Register services
builder.Services.AddHttpClient();
builder.Services.AddScoped<IDeepseekService, DeepseekService>();
builder.Services.AddScoped<ISourceService, SourceService>();
builder.Services.AddScoped<IChatService, ChatService>();

//~ Register repositories
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IContextSummaryRepository, ContextSummaryRepository>();

//~ Connection string
builder.Services.AddDbContext<ThesisDappDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseVector()));  // Add vector support for Pgvector

//~ Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

//~ Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseRouting();

//~ Use custom middleware
app.UseChatLogging();

app.MapControllers();

app.Run();
