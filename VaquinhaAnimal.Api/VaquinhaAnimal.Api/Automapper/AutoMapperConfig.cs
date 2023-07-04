using AutoMapper;
using VaquinhaAnimal.Api.ViewModels;
using VaquinhaAnimal.Domain.Entities;
using VaquinhaAnimal.Domain.Entities.Base;

namespace VaquinhaAnimal.Api.AutoMapper
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<ApplicationUser, ApplicationUserViewModel>().ReverseMap();
            CreateMap<Artigo, ArtigoViewModel>().ReverseMap();
            CreateMap<Adocao, AdocaoCreateViewModel>().ReverseMap();
            CreateMap<Adocao, AdocaoListViewModel>().ReverseMap();
            CreateMap<Campanha, CampanhaViewModel>().ReverseMap();
            CreateMap<Beneficiario, BeneficiarioViewModel>().ReverseMap();
            CreateMap<ContaDeposito, ContaDepositoViewModel>().ReverseMap();
            CreateMap<Doacao, DoacaoViewModel>().ReverseMap();
            CreateMap<Imagem, ImagemViewModel>().ReverseMap();
            CreateMap<RedeSocial, RedeSocialViewModel>().ReverseMap();
            CreateMap<Suporte, SuporteViewModel>().ReverseMap();
        }
    }
}
