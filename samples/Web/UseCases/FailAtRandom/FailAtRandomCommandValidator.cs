using FluentValidation;

namespace Web.UseCases.FailAtRandom
{
    internal sealed class FailAtRandomCommandValidator : AbstractValidator<FailAtRandomCommand>
    {
        public FailAtRandomCommandValidator()
        {
            this
                .RuleFor(x => x.FailureProbability)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(1)
                .When(x => x is not null);
        }
    }
}
