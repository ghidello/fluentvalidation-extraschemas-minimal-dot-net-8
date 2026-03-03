using FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add FV
builder.Services.AddScoped<IValidator<WeatherForecastRequest>, WeatherForecastRequestValidator>();

builder.Services.AddFluentValidationRulesToSwagger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast",
        Results<Ok<WeatherForecast[]>, ValidationProblem> ([AsParameters] WeatherForecastRequest request,
            [FromServices] IValidator<WeatherForecastRequest> validator) =>
        {
            var validationResult = validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return TypedResults.ValidationProblem(validationResult.ToDictionary());
            }

            var forecast = Enumerable.Range(1, request.PageSize).Select(index =>
                    new WeatherForecast
                    (
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(-20, 55),
                        summaries[Random.Shared.Next(summaries.Length)]
                    ))
                .ToArray();

            return TypedResults.Ok(forecast);
        })
    .WithName("GetWeatherForecast");

app.Run();

public class WeatherForecastRequestValidator : AbstractValidator<WeatherForecastRequest>
{
    public WeatherForecastRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(500);
    }
};

public record WeatherForecastRequest(int Page, int PageSize);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}