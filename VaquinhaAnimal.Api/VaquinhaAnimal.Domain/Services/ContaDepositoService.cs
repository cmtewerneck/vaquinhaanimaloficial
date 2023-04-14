using VaquinhaAnimal.Domain.Entities.Validations;
using VaquinhaAnimal.Domain.Entities;
using VaquinhaAnimal.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VaquinhaAnimal.Domain.Services
{
    public class ContaDepositoService : BaseService, IContaDepositoService
    {
        private readonly IContaDepositoRepository _contaDepositoRepository;

        public ContaDepositoService(IContaDepositoRepository contaDepositoRepository,
                              INotificador notificador) : base(notificador)
        {
            _contaDepositoRepository = contaDepositoRepository;
        }

        public async Task<bool> Adicionar(ContaDeposito contaDeposito)
        {
            if (!ExecutarValidacao(new ContaDepositoValidation(), contaDeposito)) return false;

            if (_contaDepositoRepository.Buscar(f => f.Campanha_Id == contaDeposito.Campanha_Id).Result.Any())
            {
                Notificar("Já existe uma conta cadastrada nesta campanha.");
                return false;
            }

            await _contaDepositoRepository.Insert(contaDeposito);
            return true;
        }

        public async Task<bool> Atualizar(ContaDeposito contaDeposito)
        {
            if (!ExecutarValidacao(new ContaDepositoValidation(), contaDeposito)) return false;

            if (_contaDepositoRepository.Buscar(f => f.Campanha_Id == contaDeposito.Campanha_Id && f.Id != contaDeposito.Id).Result.Any())
            {
                Notificar("Já existe uma conta cadastrada nesta campanha.");
                return false;
            }

            await _contaDepositoRepository.Update(contaDeposito);
            return true;
        }

        public async Task<bool> Remover(Guid id)
        {
            await _contaDepositoRepository.Delete(id);
            return true;
        }

        public void Dispose()
        {
            _contaDepositoRepository?.Dispose();
        }
    }
}
