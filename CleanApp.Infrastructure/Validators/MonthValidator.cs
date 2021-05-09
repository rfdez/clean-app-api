using CleanApp.Core.DTOs;
using FluentValidation;

namespace CleanApp.Infrastructure.Validators
{
    public class MonthValidator : AbstractValidator<MonthDto>
    {
        public MonthValidator()
        {
            RuleFor(month => month.YearId)
                .NotNull()
                .NotEmpty();

            RuleFor(month => month.MonthValue)
                .NotNull()
                .NotEmpty()
                .LessThan(13)
                .GreaterThan(0);
        }
    }
}
