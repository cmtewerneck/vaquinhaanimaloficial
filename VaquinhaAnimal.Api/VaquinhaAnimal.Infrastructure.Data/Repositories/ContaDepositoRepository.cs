using VaquinhaAnimal.Data.Context;
using VaquinhaAnimal.Data.Repositories;
using VaquinhaAnimal.Domain.Entities;
using VaquinhaAnimal.Domain.Interfaces;

namespace VaquinhaAnimal.Data.Repository
{
    public class ContaDepositoRepository : Repository<ContaDeposito>, IContaDepositoRepository
    {
        public ContaDepositoRepository(VaquinhaDbContext context) : base(context) { }
    }
}
