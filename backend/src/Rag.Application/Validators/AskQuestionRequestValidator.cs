using FluentValidation;
using Rag.Application.DTOs;

namespace Rag.Application.Validators;

public class AskQuestionRequestValidator : AbstractValidator<AskQuestionRequest>
{
    public AskQuestionRequestValidator()
    {
        RuleFor(x => x.Question).NotEmpty().MaximumLength(2000);
    }
}
