using Cli.Entities;
using Cocona;
using Cocona.Builder;
using Mediator.CodeGen.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cli
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CoconaAppBuilder builder = CoconaApp.CreateBuilder(args);

            builder.Services.AddScoped<JsonSerializerOptions>(sp => new()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new UserIdJsonConverter() }
            });

            builder.Services.AddMediator();

            var app = builder.Build();

            app.AddCommands<Commands>();

            app.Run();
        }
    }
}
