using Areas.Identity.Models;
using FluentValidation;

namespace Areas.Identity.Validator;

public class RequestValidator : AbstractValidator<List<EnergyDataBody>>
{
    public RequestValidator()
    {
        RuleFor(x => x).NotEmpty();
    }
}
