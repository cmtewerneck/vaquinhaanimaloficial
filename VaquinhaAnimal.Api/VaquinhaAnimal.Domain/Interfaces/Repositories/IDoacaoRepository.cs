using VaquinhaAnimal.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VaquinhaAnimal.Domain.Interfaces
{
    public interface IDoacaoRepository : IRepository<Doacao>
    {
        Task<List<Doacao>> GetAllMyDonationsAsync(Guid usuario_id);
        Task<List<Doacao>> ObterDoacoesDaCampanha(Guid campanhaId);
        Task<Doacao> GetDonationsByOrderIdAsync(string orderId);
        Task<Doacao> ObterDoacaoPelaCobranca(string charge_id);
        Task<int> ObterTotalDoadoresPorCampanha(Guid campanhaId);

    }
}
