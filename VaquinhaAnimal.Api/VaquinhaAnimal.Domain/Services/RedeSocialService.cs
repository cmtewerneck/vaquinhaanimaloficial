using VaquinhaAnimal.Domain.Entities.Validations;
using VaquinhaAnimal.Domain.Entities;
using VaquinhaAnimal.Domain.Interfaces;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace VaquinhaAnimal.Domain.Services
{
    public class RedeSocialService : BaseService, IRedeSocialService
    {
        private readonly IRedeSocialRepository _redeSocialRepository;

        public RedeSocialService(IRedeSocialRepository redeSocialRepository,
                              INotificador notificador) : base(notificador)
        {
            _redeSocialRepository = redeSocialRepository;
        }

        public async Task<bool> Adicionar(RedeSocial redeSocial)
        {
            if (!ExecutarValidacao(new RedeSocialValidation(), redeSocial)) return false;

            if (_redeSocialRepository.Buscar(f => f.Campanha_Id == redeSocial.Campanha_Id && f.Tipo == redeSocial.Tipo).Result.Any())
            {
                Notificar("Já existe essa rede social.");
                return false;
            }

            await _redeSocialRepository.Insert(redeSocial);
            return true;
        }

        public async Task<bool> Atualizar(RedeSocial redeSocial)
        {
            if (!ExecutarValidacao(new RedeSocialValidation(), redeSocial)) return false;

            if (_redeSocialRepository.Buscar(f => f.Campanha_Id == redeSocial.Campanha_Id && f.Tipo == redeSocial.Tipo && f.Id != redeSocial.Id).Result.Any())
            {
                Notificar("Já existe essa rede social.");
                return false;
            }

            await _redeSocialRepository.Update(redeSocial);
            return true;
        }

        public async Task<bool> Remover(Guid id)
        {
            await _redeSocialRepository.Delete(id);
            return true;
        }

        public void Dispose()
        {
            _redeSocialRepository?.Dispose();
        }
    }
}
