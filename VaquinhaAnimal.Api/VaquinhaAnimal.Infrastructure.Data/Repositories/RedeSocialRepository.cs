using VaquinhaAnimal.Data.Context;
using VaquinhaAnimal.Data.Repositories;
using VaquinhaAnimal.Domain.Entities;
using VaquinhaAnimal.Domain.Interfaces;

namespace VaquinhaAnimal.Data.Repository
{
    public class RedeSocialRepository : Repository<RedeSocial>, IRedeSocialRepository
    {
        public RedeSocialRepository(VaquinhaDbContext context) : base(context) { }
    }
}
