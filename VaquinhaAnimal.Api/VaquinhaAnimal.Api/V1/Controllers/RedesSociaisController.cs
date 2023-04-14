using AutoMapper;
using VaquinhaAnimal.Api.Controllers;
using VaquinhaAnimal.Api.Extensions;
using VaquinhaAnimal.Api.ViewModels;
using VaquinhaAnimal.Domain.Entities;
using VaquinhaAnimal.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VaquinhaAnimal.App.V1.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/redes-sociais")]
    public class RedesSociaisController : MainController
    {
        #region VARIABLES
        private readonly IRedeSocialRepository _redeSocialRepository;
        private readonly IRedeSocialService _redeSocialService;
        private readonly IMapper _mapper;
        #endregion

        #region CONSTRUCTOR
        public RedesSociaisController(IRedeSocialRepository redeSocialRepository,
                                      IRedeSocialService redeSocialService,
                                      IMapper mapper,
                                      INotificador notificador, IUser user) : base(notificador, user)
        {
            _redeSocialRepository = redeSocialRepository;
            _mapper = mapper;
            _redeSocialService = redeSocialService;
        }
        #endregion

        #region CRUD
        [HttpPost]
        public async Task<ActionResult<RedeSocialViewModel>> Adicionar(RedeSocialViewModel redeSocialViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _redeSocialService.Adicionar(_mapper.Map<RedeSocial>(redeSocialViewModel));

            return CustomResponse(redeSocialViewModel);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<RedeSocialViewModel>> Atualizar(Guid id, RedeSocialViewModel redeSocialViewModel)
        {
            if (id != redeSocialViewModel.id)
            {
                NotificarErro("Os ids informados não são iguais!");
                return CustomResponse();
            }

            var redeSocialAtualizacao = await ObterRedeSocial(id);

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            redeSocialAtualizacao.tipo = redeSocialViewModel.tipo;
            redeSocialAtualizacao.url = redeSocialViewModel.url;
            redeSocialAtualizacao.campanha_id = redeSocialViewModel.campanha_id;

            await _redeSocialService.Atualizar(_mapper.Map<RedeSocial>(redeSocialAtualizacao));

            return CustomResponse(redeSocialViewModel);
        }

        [ClaimsAuthorize("RedeSocial", "Excluir")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<RedeSocialViewModel>> Excluir(Guid id)
        {
            var redeSocial = await ObterRedeSocial(id);

            if (redeSocial == null)
            {
                NotificarErro("O id da rede social não foi encontrado.");
                return CustomResponse(redeSocial);
            }

            await _redeSocialService.Remover(id);

            return CustomResponse(redeSocial);
        }
        #endregion

        #region METHODS
        [HttpGet]
        public async Task<List<RedeSocialViewModel>> ObterTodos()
        {
            return _mapper.Map<List<RedeSocialViewModel>>(await _redeSocialRepository.GetAllAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RedeSocialViewModel>> ObterRedeSocialPorId(Guid id)
        {
            var redeSocial = _mapper.Map<RedeSocialViewModel>(await _redeSocialRepository.GetByIdAsync(id));

            if (redeSocial == null) return NotFound();

            return redeSocial;
        }

        private async Task<RedeSocialViewModel> ObterRedeSocial(Guid id)
        {
            return _mapper.Map<RedeSocialViewModel>(await _redeSocialRepository.GetByIdAsync(id));
        }
        #endregion
    }
}