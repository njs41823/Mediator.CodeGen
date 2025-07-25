using FluentValidation;

namespace Web.UseCases.GetRandomNumber
{
    internal sealed class GetRandomNumberQueryValidator : AbstractValidator<GetRandomNumberQuery>
    {
        public GetRandomNumberQueryValidator()
        {
            this
                .RuleFor(x => x.MinValue)
                .LessThan(x => x.MaxValue);
        }
    }
}
