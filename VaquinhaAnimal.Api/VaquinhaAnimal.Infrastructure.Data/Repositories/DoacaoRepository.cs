using VaquinhaAnimal.Domain.Interfaces;
using VaquinhaAnimal.Domain.Entities;
using VaquinhaAnimal.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using VaquinhaAnimal.Data.Repositories;
using System.Linq;
using System.Collections.Generic;

namespace VaquinhaAnimal.Data.Repository
{
    public class DoacaoRepository : Repository<Doacao>, IDoacaoRepository
    {
        public DoacaoRepository(VaquinhaDbContext context) : base(context) { }

        public async Task<Doacao> GetDonationsByOrderIdAsync(string orderId)
        {
            return await Db.Doacoes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Transacao_Id == orderId);
        }

        public async Task<List<Doacao>> GetAllMyDonationsAsync(Guid id)
        {
            return await Db.Doacoes
                .AsNoTracking()
                .Where(c => c.Usuario_Id == id.ToString())
                .Include(c => c.Campanha)
                .OrderByDescending(p => p.Data)
                .ToListAsync();
        }

        public async Task<List<Doacao>> ObterDoacoesDaCampanha(Guid campanhaId)
        {
            return await Db.Doacoes
                .AsNoTracking()
                .Where(c => c.Campanha_Id == campanhaId && c.Status == "paid")
                .OrderByDescending(p => p.Data)
                .ToListAsync();
        }

        public async Task<Doacao> ObterDoacaoPelaCobranca(string charge_id)
        {
            return await Db.Doacoes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Charge_Id == charge_id);
        }
        public async Task<int> ObterTotalDoadoresPorCampanha(Guid campanhaId)
        {
            var result = await Db.Doacoes
                .AsNoTracking()
                .Where(x => x.Campanha_Id == campanhaId)
                .Where(x => x.Status == "paid")
                .CountAsync();
            
            return result;
        }
    }
}
