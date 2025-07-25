using Bogus;
using Cli.Entities;
using Mediator.CodeGen.Contracts;

namespace Cli.UseCases.GetUserById
{
    internal sealed class GetUserByIdRequestHandler : IRequestHandler<GetUserByIdRequest, User?>
    {
        public async Task<User?> Handle(GetUserByIdRequest request, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            if (Random.Shared.NextDouble() < 0.5)
            {
                return null;
            }

            var userFaker = new Faker<User>()
                .RuleFor(u => u.Id, f => request.UserId)
                .RuleFor(u => u.EmailAddress, f => f.Internet.Email())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName());

            var user = userFaker.Generate();

            return user;
        }
    }
}
