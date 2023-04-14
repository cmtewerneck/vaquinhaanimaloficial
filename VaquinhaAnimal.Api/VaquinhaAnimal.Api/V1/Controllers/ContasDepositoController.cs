using AutoMapper;
using VaquinhaAnimal.Api.Controllers;
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
    [Route("api/v{version:apiVersion}/contas-deposito")]
    public class ContasDepositoController : MainController
    {
        #region VARIABLES
        private readonly IContaDepositoRepository _contaDepositoRepository;
        private readonly IContaDepositoService _contaDepositoService;
        private readonly IMapper _mapper;
        #endregion

        #region CONSTRUCTOR
        public ContasDepositoController(IContaDepositoRepository contaDepositoRepository,
                                        IContaDepositoService contaDepositoService,
                                        IMapper mapper,
                                        INotificador notificador, IUser user) : base(notificador, user)
        {
            _contaDepositoRepository = contaDepositoRepository;
            _mapper = mapper;
            _contaDepositoService = contaDepositoService;
        }
        #endregion

        #region CRUD
        [HttpPost]
        public async Task<ActionResult<ContaDepositoViewModel>> Adicionar(ContaDepositoViewModel contaDepositoViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _contaDepositoService.Adicionar(_mapper.Map<ContaDeposito>(contaDepositoViewModel));

            return CustomResponse(contaDepositoViewModel);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ContaDepositoViewModel>> Atualizar(Guid id, ContaDepositoViewModel contaDepositoViewModel)
        {
            if (id != contaDepositoViewModel.id)
            {
                NotificarErro("Os ids informados não são iguais!");
                return CustomResponse();
            }

            var contaDepositoAtualizacao = await ObterContaDeposito(id);

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            contaDepositoAtualizacao.banco = contaDepositoViewModel.banco;
            contaDepositoAtualizacao.tipo_conta = contaDepositoViewModel.tipo_conta;
            contaDepositoAtualizacao.agencia = contaDepositoViewModel.agencia;
            contaDepositoAtualizacao.agencia_digito = contaDepositoViewModel.agencia_digito;
            contaDepositoAtualizacao.conta = contaDepositoViewModel.conta;
            contaDepositoAtualizacao.conta_digito = contaDepositoViewModel.conta_digito;
            contaDepositoAtualizacao.tipo_pessoa = contaDepositoViewModel.tipo_pessoa;
            contaDepositoAtualizacao.documento = contaDepositoViewModel.documento;
            contaDepositoAtualizacao.campanha_id = contaDepositoViewModel.campanha_id;
            
            await _contaDepositoService.Atualizar(_mapper.Map<ContaDeposito>(contaDepositoAtualizacao));

            return CustomResponse(contaDepositoViewModel);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ContaDepositoViewModel>> Excluir(Guid id)
        {
            var contaDeposito = await ObterContaDeposito(id);

            if (contaDeposito == null)
            {
                NotificarErro("O id da conta não foi encontrado.");
                return CustomResponse(contaDeposito);
            }

            await _contaDepositoService.Remover(id);

            return CustomResponse(contaDeposito);
        }
        #endregion

        #region METHODS
        [HttpGet]
        public async Task<List<ContaDepositoViewModel>> ObterTodos()
        {
            return _mapper.Map<List<ContaDepositoViewModel>>(await _contaDepositoRepository.GetAllAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ContaDepositoViewModel>> ObterContaDepositoPorId(Guid id)
        {
            var contaDeposito = _mapper.Map<ContaDepositoViewModel>(await _contaDepositoRepository.GetByIdAsync(id));

            if (contaDeposito == null) return NotFound();

            return contaDeposito;
        }

        private async Task<ContaDepositoViewModel> ObterContaDeposito(Guid id)
        {
            return _mapper.Map<ContaDepositoViewModel>(await _contaDepositoRepository.GetByIdAsync(id));
        }
        #endregion
    }
}