using VaquinhaAnimal.Domain.Entities.Validations.Documents;
using VaquinhaAnimal.Domain.Enums;
using FluentValidation;

namespace VaquinhaAnimal.Domain.Entities.Validations
{
    public class ContaDepositoValidation : AbstractValidator<ContaDeposito>
    {
        public ContaDepositoValidation()
        {
            RuleFor(c => c.Banco)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido");

            RuleFor(c => c.TipoConta)
                .NotNull().WithMessage("O campo {PropertyName} precisa ser fornecido");

            RuleFor(c => c.Agencia)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .Length(1, 10).WithMessage("O campo {PropertyName} precisa ter entre {MinLength} e {MaxLength} caracteres");

            RuleFor(c => c.AgenciaDigito)
                .MaximumLength(1).When(c => c.AgenciaDigito != "").WithMessage("O campo {PropertyName} precisa ter no máximo 1 caracter");

            RuleFor(c => c.Conta)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .Length(1, 13).WithMessage("O campo {PropertyName} precisa ter entre {MinLength} e {MaxLength} caracteres");

            RuleFor(c => c.ContaDigito)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido")
                .Length(1,2).WithMessage("O campo {PropertyName} precisa ter entre {MinLength} e {MaxLength} caracteres");

            RuleFor(c => c.TipoPessoa)
                .NotNull().WithMessage("O campo {PropertyName} precisa ser fornecido");

            When(f => f.TipoPessoa == TipoPessoaEnum.Fisica, () =>
            {
                RuleFor(f => f.Documento.Length).Equal(CpfValidacao.TamanhoCpf)
                    .WithMessage("O campo Documento precisa ter {ComparisonValue} caracteres e foi fornecido {PropertyValue}");

                RuleFor(f => CpfValidacao.Validar(f.Documento)).Equal(true)
                    .WithMessage("O Documento fornecido é inválido.");
            });

            When(f => f.TipoPessoa == TipoPessoaEnum.Juridica, () =>
            {
                RuleFor(f => f.Documento.Length).Equal(CnpjValidacao.TamanhoCnpj)
                    .WithMessage("O campo Documento precisa ter {ComparisonValue} caracteres e foi fornecido {PropertyValue}");

                RuleFor(f => CnpjValidacao.Validar(f.Documento)).Equal(true)
                    .WithMessage("O Documento fornecido é inválido.");
            });

            RuleFor(c => c.Campanha_Id)
                .NotEmpty().WithMessage("O campo {PropertyName} precisa ser fornecido");
        }
    }
}
