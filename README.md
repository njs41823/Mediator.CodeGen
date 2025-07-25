# Mediator.CodeGen
   
Simple mediator implementation in .NET using code generation. Scans the target project using the Rosyln APIs and generates an ISender implementation.

## Installing Mediator.CodeGen

You should install Mediator.CodeGen with NuGet or the .NET Core command line interface:

`Install-Package Mediator.CodeGen`

or

`dotnet add package Mediator.CodeGen`

## Getting Started
Mediator.CodeGen supports Microsoft.Extensions.DependencyInjection.Abstractions directly:

`services.AddMediator();`

This registers:

- ISender as scoped
- IRequestHandler<,> closed implementations as scoped
- Attempts to close any open IPipelineBehavior<,> types and register them as scoped