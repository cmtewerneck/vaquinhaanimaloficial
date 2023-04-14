using FluentValidation;

namespace VaquinhaAnimal.Domain.Entities.Validations
{
    public class RedeSocialValidation : AbstractValidator<RedeSocial>
    {
        public RedeSocialValidation()
        {
            RuleFor(c => c.Tipo)
                .NotNull().WithMessage("O campo {PropertyName} precisa ser fornecido");

            RuleFor(c => c.Url)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .Length(1, 200).WithMessage("O campo {PropertyName} precisa ter entre {MinLength} e {MaxLength} caracteres");

            RuleFor(c => c.Campanha_Id)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido");
        }
    }
}
