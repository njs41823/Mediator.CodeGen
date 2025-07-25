using Cli.Entities;
using Mediator.CodeGen.Contracts;

namespace Cli.UseCases.GetUserById
{
    internal sealed record GetUserByIdRequest(UserId UserId) : IRequest<User?>;
}
