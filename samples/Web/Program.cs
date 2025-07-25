using Mediator.CodeGen.Contracts;
using Mediator.CodeGen.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using Web.Shared;
using Web.UseCases.FailAtRandom;
using Web.UseCases.GetGuid;
using Web.UseCases.GetRandomNumber;

namespace Web
{
    internal sealed class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped(sp => new JsonSerializerOptions
            {
                Converters = { new ResultJsonConverterFactory() },
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            builder.Services.AddMemoryCache();

            builder.Services.AddAuthorization();

            builder.Services.AddMediator();

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGet("/getRandomNumber", async (
                [FromServices] ISender sender,
                [FromQuery] long minValue,
                [FromQuery] long maxValue,
                CancellationToken cancellationToken) =>
            {
                var query = new GetRandomNumberQuery(minValue, maxValue);

                var response = await sender.Send(query, cancellationToken);

                var result = new
                {
                    isSuccess = response.IsSuccess,
                    value = (long?)null,
                    error = (Error?)null
                };

                if (result.isSuccess)
                {
                    result = result with { value = response.Value };
                }
                else
                {
                    result = result with { error = response.Error };
                }

                return result;
            });

            app.MapGet("/getGuid", async (
                [FromServices] ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new GetGuidQuery();

                var response = await sender.Send(query, cancellationToken);

                var result = new
                {
                    isSuccess = response.IsSuccess,
                    value = (Guid?)null,
                    error = (Error?)null
                };

                if (result.isSuccess)
                {
                    result = result with { value = response.Value };
                }
                else
                {
                    result = result with { error = response.Error };
                }

                return result;
            });

            app.MapGet("/failAtRandom", async (
                [FromServices] ISender sender,
                [FromQuery] double? failureProbability,
                CancellationToken cancellationToken) =>
            {
                var command = new FailAtRandomCommand(failureProbability);

                var response = await sender.Send(command, cancellationToken);

                var result = new
                {
                    isSuccess = response.IsSuccess,
                    error = (Error?)null
                };

                if (!result.isSuccess)
                {
                    result = result with { error = response.Error };
                }

                return result;
            });

            app.Run();
        }
    }
}
