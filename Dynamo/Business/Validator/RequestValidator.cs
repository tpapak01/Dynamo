using Dynamo.Business.APIbodies;
using FluentValidation;

namespace Dynamo.Business.Validator;

public class RequestValidator : AbstractValidator<List<EnergyDataBody>>
{
    public RequestValidator()
    {
        RuleFor(x => x).NotEmpty();
    }
}
