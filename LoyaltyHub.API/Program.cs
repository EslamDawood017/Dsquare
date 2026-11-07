using LoyaltyHub.Application.Interfaces.Repositories;
using LoyaltyHub.Application.Interfaces.Services;
using LoyaltyHub.Application.Mappings;
using LoyaltyHub.Application.Services;
using LoyaltyHub.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(err =>
                    string.IsNullOrWhiteSpace(err.ErrorMessage)
                        ? "Invalid value provided."
                        : err.ErrorMessage))
                .ToList();

            var body = new
            {
                Code    = "INVALID_REQUEST",
                Message = "The request body is malformed or contains invalid fields.",
                Details = errors
            };

            return new BadRequestObjectResult(body);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IWalletRepository, WalletRepository>();
builder.Services.AddSingleton<IAwardRepository, AwardRepository>();

builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IAwardService, AwardService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
