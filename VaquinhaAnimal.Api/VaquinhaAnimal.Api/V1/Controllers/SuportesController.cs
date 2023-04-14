using AutoMapper;
using VaquinhaAnimal.Api.Controllers;
using VaquinhaAnimal.Api.ViewModels;
using VaquinhaAnimal.Domain.Entities;
using VaquinhaAnimal.Domain.Entities.Base;
using VaquinhaAnimal.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace VaquinhaAnimal.App.V1.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/tickets")]
    public class SuportesController : MainController
    {

        #region VARIABLES
        private readonly ISuporteRepository _suporteRepository;
        private readonly ISuporteService _suporteService;
        private readonly IUsuarioService _usuarioService;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUser _user;
        #endregion

        #region CONSTRUCTOR
        public SuportesController(ISuporteRepository suporteRepository,
                                  ISuporteService suporteService,
                                  IUsuarioService usuarioService,
                                  IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  INotificador notificador, IUser user) : base(notificador, user)
        {
            _suporteRepository = suporteRepository;
            _mapper = mapper;
            _suporteService = suporteService;
            _usuarioService = usuarioService;
            _userManager = userManager;
            _user = user;
        }
        #endregion

        #region CRUD
        [HttpGet("meus-tickets")]
        public async Task<List<SuporteViewModel>> ObterMeusTickets()
        {
            var usuarioLogadoId = _user.GetUserId();
            var tickets = await _suporteRepository.GetAllMyTicketsAsync(usuarioLogadoId);
            return _mapper.Map<List<SuporteViewModel>>(tickets);
        }

        [HttpGet("all-tickets")]
        public async Task<List<SuporteViewModel>> ObterAllTickets()
        {
            var tickets = await _suporteRepository.GetAllTicketsAsync();
            return _mapper.Map<List<SuporteViewModel>>(tickets);
        }

        [HttpPost]
        public async Task<ActionResult<SuporteViewModel>> Adicionar(SuporteViewModel suporteViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            suporteViewModel.data = DateTime.Now;
            suporteViewModel.usuario_id = _user.GetUserId();

            var ticketToAdd = _mapper.Map<Suporte>(suporteViewModel);

            var result = await _suporteService.Adicionar(ticketToAdd);

            if (result)
            {
                SendEmailTicketAdded(ticketToAdd);
            }

            return CustomResponse(suporteViewModel);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<SuporteViewModel>> Atualizar(Guid id, SuporteViewModel suporteViewModel)
        {
            if (id != suporteViewModel.id)
            {
                NotificarErro("Os ids informados não são iguais!");
                return CustomResponse();
            }

            var ticketAtualizacao = await ObterTicket(id);

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            ticketAtualizacao.data = suporteViewModel.data;
            ticketAtualizacao.assunto = suporteViewModel.assunto;
            ticketAtualizacao.mensagem = suporteViewModel.mensagem;
            ticketAtualizacao.respondido = suporteViewModel.respondido;
            ticketAtualizacao.resposta = suporteViewModel.resposta;
            ticketAtualizacao.usuario_id = suporteViewModel.usuario_id;

            var result = await _suporteService.Atualizar(_mapper.Map<Suporte>(ticketAtualizacao));

            return CustomResponse(suporteViewModel);
        }

        [HttpPut("resposta-ticket/{id:guid}")]
        public async Task<ActionResult<SuporteViewModel>> RespostaTicket(Guid id, SuporteViewModel suporteViewModel)
        {
            if (id != suporteViewModel.id)
            {
                NotificarErro("Os ids informados não são iguais!");
                return CustomResponse();
            }

            var ticketAtualizacao = await ObterTicket(id);

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            ticketAtualizacao.data = suporteViewModel.data;
            ticketAtualizacao.assunto = suporteViewModel.assunto;
            ticketAtualizacao.mensagem = suporteViewModel.mensagem;
            ticketAtualizacao.respondido = suporteViewModel.respondido;
            ticketAtualizacao.resposta = suporteViewModel.resposta;
            ticketAtualizacao.usuario_id = suporteViewModel.usuario_id;

            var result = await _suporteService.Atualizar(_mapper.Map<Suporte>(ticketAtualizacao));
            if (result)
            {
                await SendEmailTicketRespondido(ticketAtualizacao.usuario_id);
            }

            return CustomResponse(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<SuporteViewModel>> Excluir(Guid id)
        {
            var ticket = await ObterTicket(id);

            if (ticket == null)
            {
                NotificarErro("O id do ticket não foi encontrado.");
                return CustomResponse(ticket);
            }

            await _suporteService.Remover(id);

            return CustomResponse(ticket);
        }
        #endregion

        #region METHODS
        private void SendEmailTicketAdded(Suporte suporte)
        {
            var userMail = _user.GetUserEmail();

            try
            {
                MailMessage message = new MailMessage("contato@doadoresespeciais.com.br", userMail);
                message.Subject = "Ticket criado - Doadores Especiais";
                message.Body = "<div style='font-size: 12px; font-family: Verdana; background-color: #f8f8f8; margin-left: 20px;'>" +
                    "<img src='https://www.doadoresespeciais.com.br/assets/img/logo.png' style='width: 300px;'/><br/><br/>" +
                    "<h2>Obrigado por deixar a sua dúvida</h2></br></br> <h3><u>Confira os detalhes abaixo: </u></h2><br/><br/> " +
                    "<p><b>Data: </b>" + suporte.Data + "</p>" +
                    "<p><b>Assunto: </b>" + suporte.Assunto + "</p>" +
                    "<p><b>Mensagem: </b>" + suporte.Mensagem + "</p>" +
                    "<h4>Fique ligado, você será avisado por e-mail quando sua resposta for emitida.</h4></div>";

                MailAddress copy = new MailAddress("contato@doadoresespeciais.com.br");
                message.CC.Add(copy);
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.zoho.com";
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("contato@doadoresespeciais.com.br", "Vasco10@");
                smtp.EnableSsl = true;

                message.IsBodyHtml = true;
                message.Priority = MailPriority.Normal;
                smtp.Send(message);
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }

        private async Task SendEmailTicketRespondido(Guid usuario_id)
        {
            var usuarioEmail = await _usuarioService.GetEmailById(usuario_id);

            try
            {
                MailMessage message = new MailMessage("contato@doadoresespeciais.com.br", usuarioEmail.Email);
                message.Subject = "Ticket Respondido - Doadores Especiais";
                message.Body = "<div style='font-size: 12px; font-family: Verdana; background-color: #f8f8f8; margin-left: 20px;'>" +
                    "<div class='container'><div style='text-align: center; margin-top: 20px;'><img src='https://www.doadoresespeciais.com.br/assets/img/logo.png' style='width: 300px;'/></div><br/><br/>" +
                    "<h2>Obrigado por deixar a sua dúvida</h2></br></br> <h3><u>Acesse sua dashboard para verificar a resposta </u></h2><br/><br/><br/> " +
                    "<div style='text-align: center; margin-bottom: 20px;'><a class='btn btn-success' href='https://www.doadoresespeciais.com.br'>VER RESPOSTA</a></div></div> ";

                MailAddress copy = new MailAddress("contato@doadoresespeciais.com.br");
                message.CC.Add(copy);
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.zoho.com";
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential("contato@doadoresespeciais.com.br", "Vasco10@");
                smtp.EnableSsl = true;

                message.IsBodyHtml = true;
                message.Priority = MailPriority.Normal;
                smtp.Send(message);
            }

            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }

        }

        private async Task<SuporteViewModel> ObterTicket(Guid id)
        {
            var ticketToReturn = await _suporteRepository.GetByIdAsync(id);
            return _mapper.Map<SuporteViewModel>(ticketToReturn);
        }
        #endregion

    }
}