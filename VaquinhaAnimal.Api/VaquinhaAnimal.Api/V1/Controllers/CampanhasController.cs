using AutoMapper;
using VaquinhaAnimal.Api.Controllers;
using VaquinhaAnimal.Api.ViewModels;
using VaquinhaAnimal.Domain.Entities;
using VaquinhaAnimal.Domain.Entities.Base;
using VaquinhaAnimal.Domain.Entities.Pagarme;
using VaquinhaAnimal.Domain.Entities.Validations.Documents;
using VaquinhaAnimal.Domain.Enums;
using VaquinhaAnimal.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Threading.Tasks;

namespace VaquinhaAnimal.App.V1.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/campanhas")]
    public class CampanhasController : MainController
    {
        #region VARIABLES
        private readonly ICampanhaRepository _campanhaRepository;
        private readonly ICampanhaService _campanhaService;
        private readonly IImagemService _imagemService;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUser _user;
        #endregion

        #region CONSTRUCTOR
        public CampanhasController(ICampanhaRepository campanhaRepository,
                                  ICampanhaService campanhaService,
                                  IImagemService imagemService,
                                  IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  INotificador notificador, IUser user) : base(notificador, user)
        {
            _imagemService = imagemService;
            _campanhaRepository = campanhaRepository;
            _mapper = mapper;
            _campanhaService = campanhaService;
            _userManager = userManager;
            _user = user;
        }
        #endregion

        #region CRUD
        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 3145728)]
        [RequestSizeLimit(3145728)]
        public async Task<ActionResult<CampanhaViewModel>> Adicionar(CampanhaViewModel campanhaViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            campanhaViewModel.DataCriacao = DateTime.Now;
            campanhaViewModel.StatusCampanha = (int)StatusCampanhaEnum.EDITANDO;
            campanhaViewModel.TotalArrecadado = 0;
            campanhaViewModel.Usuario_Id = _user.GetUserId();

            if (campanhaViewModel.Imagens != null && campanhaViewModel.Imagens.Count != 0)
            {
                foreach (var imagem in campanhaViewModel.Imagens)
                {
                    var arquivoNome = Guid.NewGuid() + "_" + imagem.arquivo;

                    if (!UploadArquivo(imagem.arquivo_upload, arquivoNome))
                    {
                        return BadRequest("Erro ao tentar carregar imagem.");
                    }

                    imagem.arquivo = arquivoNome;
                }
            }

            if (campanhaViewModel.Beneficiario.Tipo == "individual")
            {
                if (campanhaViewModel.Beneficiario.Documento.Length != CpfValidacao.TamanhoCpf)
                {
                    NotificarErro("Quantidade de caracteres inválida.");
                    return CustomResponse();
                }

                var cpfValido = CpfValidacao.Validar(campanhaViewModel.Beneficiario.Documento);

                if (cpfValido == false)
                {
                    NotificarErro("CPF inválido.");
                    return CustomResponse();
                }
            }
            else if (campanhaViewModel.Beneficiario.Tipo == "company")
            {
                if (campanhaViewModel.Beneficiario.Documento.Length != CnpjValidacao.TamanhoCnpj)
                {
                    NotificarErro("Quantidade de caracteres inválida.");
                    return CustomResponse();
                }

                var cnpjValido = CnpjValidacao.Validar(campanhaViewModel.Beneficiario.Documento);

                if (cnpjValido == false)
                {
                    NotificarErro("CNPJ inválido.");
                    return CustomResponse();
                }
            }
            else
            {
                NotificarErro("Erro ao validar documento.");
                return CustomResponse();
            }

            var campanhaToAdd = _mapper.Map<Campanha>(campanhaViewModel);

            var result = await _campanhaService.Adicionar(campanhaToAdd);

            //if (result)
            //{
            //    SendEmailCampanhaAdded(campanhaToAdd);
            //}

            return CustomResponse(campanhaViewModel);
        }

        [HttpPut("{id:guid}")]
        [RequestFormLimits(MultipartBodyLengthLimit = 3145728)]
        [RequestSizeLimit(3145728)]
        public async Task<ActionResult<CampanhaViewModel>> Atualizar(Guid id, CampanhaViewModel campanhaViewModel)
        {
            if (id != campanhaViewModel.Id)
            {
                NotificarErro("Os ids informados não são iguais!");
                return CustomResponse();
            }

            var campanhaAtualizacao = await ObterCampanha(id);

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (campanhaViewModel.Beneficiario.Tipo == "individual")
            {
                if (campanhaViewModel.Beneficiario.Documento.Length != CpfValidacao.TamanhoCpf)
                {
                    NotificarErro("Quantidade de caracteres inválida.");
                    return CustomResponse();
                }

                var cpfValido = CpfValidacao.Validar(campanhaViewModel.Beneficiario.Documento);

                if (cpfValido == false)
                {
                    NotificarErro("CPF inválido.");
                    return CustomResponse();
                }
            }

            if (campanhaViewModel.Beneficiario.Tipo == "company")
            {
                if (campanhaViewModel.Beneficiario.Documento.Length != CnpjValidacao.TamanhoCnpj)
                {
                    NotificarErro("Quantidade de caracteres inválida.");
                    return CustomResponse();
                }

                var cnpjValido = CnpjValidacao.Validar(campanhaViewModel.Beneficiario.Documento);

                if (cnpjValido == false)
                {
                    NotificarErro("CNPJ inválido.");
                    return CustomResponse();
                }
            }

            //ATUALIZANDO IMAGEM
            if (campanhaViewModel.Imagens != null && campanhaViewModel.Imagens.Count != 0)
            {
                foreach (var imagem in campanhaViewModel.Imagens)
                {
                    var arquivoNome = Guid.NewGuid() + "_" + imagem.arquivo;

                    if (!UploadArquivo(imagem.arquivo_upload, arquivoNome))
                    {
                        NotificarErro("Erro ao atualizar imagem!");
                        return CustomResponse();
                    }

                    imagem.arquivo = arquivoNome;

                    // REMOVENDO OUTRA POSSÍVEL CAPA
                    var imagensPraRemover = new List<ImagemViewModel>();

                    foreach (var image in campanhaAtualizacao.Imagens)
                    {
                        if (image.tipo == 1)
                        {
                            imagensPraRemover.Add(image);
                        }
                    }

                    foreach (var imageToRemove in imagensPraRemover)
                    {
                        campanhaAtualizacao.Imagens.Remove(imageToRemove);

                        var resultExclusao = await _imagemService.Remover(imageToRemove.id);

                        if (!resultExclusao)
                        {
                            NotificarErro("Erro ao deletar imagem do banco");
                            return CustomResponse();
                        }

                        var exclusaoImagemDB = RemoverArquivo(imageToRemove);

                        if (!exclusaoImagemDB)
                        {
                            NotificarErro("Erro ao deletar arquivo da imagem da pasta");
                            return CustomResponse();
                        }
                    }

                    campanhaAtualizacao.Imagens.Add(imagem);
                }
            }

            campanhaAtualizacao.DataCriacao = campanhaViewModel.DataCriacao;
            campanhaAtualizacao.DataInicio = campanhaViewModel.DataInicio;
            campanhaAtualizacao.DataEncerramento = campanhaViewModel.DataEncerramento;
            campanhaAtualizacao.DuracaoDias = campanhaViewModel.DuracaoDias;
            campanhaAtualizacao.TagCampanha = campanhaViewModel.TagCampanha;
            campanhaAtualizacao.Titulo = campanhaViewModel.Titulo;
            campanhaAtualizacao.DescricaoCurta = campanhaViewModel.DescricaoCurta;
            campanhaAtualizacao.DescricaoLonga = campanhaViewModel.DescricaoLonga;
            campanhaAtualizacao.ValorDesejado = campanhaViewModel.ValorDesejado;
            campanhaAtualizacao.TotalArrecadado = campanhaViewModel.TotalArrecadado;
            campanhaAtualizacao.Termos = campanhaViewModel.Termos;
            campanhaAtualizacao.Premium = campanhaViewModel.Premium;
            campanhaAtualizacao.StatusCampanha = campanhaViewModel.StatusCampanha;
            campanhaAtualizacao.VideoUrl = campanhaViewModel.VideoUrl;

            campanhaAtualizacao.Beneficiario.DigitoAgencia = campanhaViewModel.Beneficiario.DigitoAgencia;
            campanhaAtualizacao.Beneficiario.NumeroAgencia = campanhaViewModel.Beneficiario.NumeroAgencia;
            campanhaAtualizacao.Beneficiario.DigitoConta = campanhaViewModel.Beneficiario.DigitoConta;
            campanhaAtualizacao.Beneficiario.CodigoBanco = campanhaViewModel.Beneficiario.CodigoBanco;
            campanhaAtualizacao.Beneficiario.NumeroConta = campanhaViewModel.Beneficiario.NumeroConta;
            campanhaAtualizacao.Beneficiario.Documento = campanhaViewModel.Beneficiario.Documento;
            campanhaAtualizacao.Beneficiario.Tipo = campanhaViewModel.Beneficiario.Tipo;
            campanhaAtualizacao.Beneficiario.TipoConta = campanhaViewModel.Beneficiario.TipoConta;
            campanhaAtualizacao.Beneficiario.Nome = campanhaViewModel.Beneficiario.Nome;
            campanhaAtualizacao.Beneficiario.RecebedorId = campanhaViewModel.Beneficiario.RecebedorId;

            var result = await _campanhaService.Atualizar(_mapper.Map<Campanha>(campanhaAtualizacao));

            return CustomResponse(campanhaViewModel);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<CampanhaViewModel>> Excluir(Guid id)
        {
            var campanha = await ObterCampanha(id);

            if (campanha == null)
            {
                NotificarErro("O id da campanha não foi encontrado.");
                return CustomResponse(campanha);
            }

            // REMOVENDO IMAGENS
            foreach (var imagem in campanha.Imagens)
            {
                var resultExclusao = await _imagemService.Remover(imagem.id);

                if (!resultExclusao)
                {
                    NotificarErro("Erro ao deletar imagem do banco");
                    return CustomResponse();
                }

                var exclusaoImagemDB = RemoverArquivo(imagem);

                if (!exclusaoImagemDB)
                {
                    NotificarErro("Erro ao deletar arquivo da imagem da pasta");
                    return CustomResponse();
                }
            }

            await _campanhaService.Remover(id);

            return CustomResponse(campanha);
        }

        [HttpPut("enviar-para-analise/{id:guid}")]
        public async Task<ActionResult<CampanhaViewModel>> EnviarParaAnalise(Guid id)
        {
            var campanhaAtualizar = await ObterCampanha(id);

            if (campanhaAtualizar == null)
            {
                NotificarErro("Campanha não encontrada!");
                return CustomResponse();
            }

            campanhaAtualizar.StatusCampanha = (int)StatusCampanhaEnum.ANALISE;

            var result = await _campanhaService.Atualizar(_mapper.Map<Campanha>(campanhaAtualizar));

            if (result)
            {
                SendEmailCampanhaEnviadaPraAnalise();
            }

            return CustomResponse(campanhaAtualizar);
        }

        [HttpPut("iniciar-campanha/{id:guid}")]
        public async Task<ActionResult<CampanhaViewModel>> IniciarCampanha(Guid id)
        {
            var campanhaIniciar = await ObterCampanha(id);

            if (campanhaIniciar == null)
            {
                NotificarErro("Campanha não encontrada!");
                return CustomResponse();
            }

            // ADICIONANDO RECEBEDOR NA PAGARME PRO SPLIT
            var recebedor_id = await AddRecebedor(campanhaIniciar);

            if (String.IsNullOrWhiteSpace(recebedor_id))
            {
                NotificarErro("Erro ao adicionar recebedor.");
                return CustomResponse();
            }

            campanhaIniciar.Beneficiario.RecebedorId = recebedor_id;
            campanhaIniciar.StatusCampanha = (int)StatusCampanhaEnum.ANDAMENTO;
            campanhaIniciar.DataInicio = DateTime.Today;

            if (campanhaIniciar.TipoCampanha == TipoCampanhaEnum.UNICA)
            {
                campanhaIniciar.DataEncerramento = DateTime.Today.AddDays((double)campanhaIniciar.DuracaoDias);
            }

            try
            {
                var result = await _campanhaService.Atualizar(_mapper.Map<Campanha>(campanhaIniciar));

                if (result)
                {
                    SendEmailCampanhaIniciada(campanhaIniciar.Usuario_Id);
                }

                return CustomResponse(result);
            }
            catch (Exception e)
            {
                throw new Exception("Erro: " + e);
            }

        }

        [HttpPut("rejeitar-campanha/{id:guid}")]
        public async Task<ActionResult<CampanhaViewModel>> RejeitarCampanha(Guid id)
        {
            var campanhaRejeitar = await ObterCampanha(id);

            if (campanhaRejeitar == null)
            {
                NotificarErro("Campanha não encontrada!");
                return CustomResponse();
            }

            campanhaRejeitar.StatusCampanha = (int)StatusCampanhaEnum.REJEITADA;

            var result = await _campanhaService.Atualizar(_mapper.Map<Campanha>(campanhaRejeitar));

            if (result)
            {
                SendEmailCampanhaRejeitada(campanhaRejeitar.Usuario_Id);
            }

            return CustomResponse(campanhaRejeitar);
        }

        [HttpPut("retornar-campanha/{id:guid}")]
        public async Task<ActionResult<CampanhaViewModel>> RetornarCampanha(Guid id)
        {
            var campanhaRetornar = await ObterCampanha(id);

            if (campanhaRetornar == null)
            {
                NotificarErro("Campanha não encontrada!");
                return CustomResponse();
            }

            campanhaRetornar.StatusCampanha = (int)StatusCampanhaEnum.EDITANDO;

            var result = await _campanhaService.Atualizar(_mapper.Map<Campanha>(campanhaRetornar));

            if (result)
            {
                SendEmailCampanhaRetornada(campanhaRetornar.Usuario_Id);
            }

            return CustomResponse(campanhaRetornar);
        }

        [HttpPut("parar-campanha/{id:guid}")]
        public async Task<ActionResult<CampanhaViewModel>> PararCampanha(Guid id)
        {
            var campanhaAtualizar = await ObterCampanha(id);

            if (campanhaAtualizar == null)
            {
                NotificarErro("Campanha não encontrada!");
                return CustomResponse();
            }

            campanhaAtualizar.StatusCampanha = (int)StatusCampanhaEnum.FINALIZADA;

            var result = await _campanhaService.Atualizar(_mapper.Map<Campanha>(campanhaAtualizar));

            if (result)
            {
                SendEmailCampanhaFinalizada(campanhaAtualizar.Usuario_Id, campanhaAtualizar.Titulo);
            }

            return CustomResponse(campanhaAtualizar);
        }
        #endregion

        #region METHODS
        [AllowAnonymous]
        [HttpGet]
        public async Task<List<CampanhaViewModel>> ObterTodos()
        {
            var usuario = _user.GetUserEmail();
            var result = await _campanhaRepository.GetAllCampaignsAndImagesAsync(usuario);
            return _mapper.Map<List<CampanhaViewModel>>(result);
        }

        [AllowAnonymous]
        [HttpGet("todos-paginado/{PageSize:int}/{PageNumber:int}")]
        public async Task<ActionResult> ObterTodosPaginado(int PageSize, int PageNumber)
        {
            var result = await _campanhaRepository.ListAsync(PageSize, PageNumber);

            return Ok(result);
        }

        [HttpGet("minhas-campanhas")]
        public async Task<List<CampanhaViewModel>> ObterMinhasCampanhas()
        {
            var usuarioLogadoId = _user.GetUserId();
            var campanhas = await _campanhaRepository.GetAllMyCampaignsAsync(usuarioLogadoId);
            return _mapper.Map<List<CampanhaViewModel>>(campanhas);
        }

        [HttpGet("quantidade")]
        public async Task<int> ObterQuantidadeCampanhasCadastradas()
        {
            return _mapper.Map<int>(await _campanhaRepository.ObterTotalRegistros());
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<CampanhaViewModel>> ObterCampanhaPorId(Guid id)
        {
            var result = await _campanhaRepository.GetByIdWithImagesAsync(id);

            var campanha = _mapper.Map<CampanhaViewModel>(result);

            if (campanha == null) return NotFound();

            return campanha;
        }

        private async Task<CampanhaViewModel> ObterCampanha(Guid id)
        {
            var campanhaToReturn = await _campanhaRepository.GetByIdWithImagesAsync(id);
            return _mapper.Map<CampanhaViewModel>(campanhaToReturn);
        }

        private async Task<string> AddRecebedor(CampanhaViewModel dados)
        {
            var agenciaDigito = "";

            if (dados.Beneficiario.DigitoAgencia == "")
            {
                agenciaDigito = null;
            }
            else 
			{ 
				agenciaDigito = dados.Beneficiario.DigitoAgencia; 
			}

            try
            {
                AddHeaderPagarme();

                var recebedorToAdd = new PagarmeRecebedor()
                {
                    name = dados.Beneficiario.Nome,
                    type = dados.Beneficiario.Tipo,
                    document = dados.Beneficiario.Documento,
                    default_bank_account = new PagarmeRecebedorBankAcoount()
                    {
                        holder_name = dados.Beneficiario.Nome,
                        bank = dados.Beneficiario.CodigoBanco,
                        branch_number = dados.Beneficiario.NumeroAgencia,
                        branch_check_digit = agenciaDigito,
                        account_number = dados.Beneficiario.NumeroConta,
                        account_check_digit = dados.Beneficiario.DigitoConta,
                        holder_type = dados.Beneficiario.Tipo,
                        holder_document = dados.Beneficiario.Documento,
                        type = dados.Beneficiario.TipoConta
                    },
                    transfer_settings = new PagarmeRecebedorTransferSettings()
                    {
                        transfer_enabled = false,
                        transfer_interval = "daily",
                        transfer_day = 0
                    },
                    automatic_anticipation_settings = new PagarmeRecebedorAntecipationSettings()
                    {
                        enables = false
                    }
                };

                HttpResponseMessage response = await client.PostAsJsonAsync(urlPagarme + "recipients", recebedorToAdd);
                string responseBody = await response.Content.ReadAsStringAsync();

                var recebedorResponse = JsonConvert.DeserializeObject<PagarmeRecebedorResponse>(responseBody);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return recebedorResponse.id;
                }
                else
                {
                    return "";
                }
            }
            catch (HttpRequestException e)
            {
                return "";
            }
        }

        private bool UploadArquivo(string arquivo, string imgNome)
        {
            if (string.IsNullOrEmpty(arquivo))
            {
                return false;
            }

            var imageDataByteArray = Convert.FromBase64String(arquivo);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);

            return true;
        }

        private bool RemoverArquivo(ImagemViewModel image)
        {
            if (image == null)
            {
                return false;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", image.arquivo);

            if (!System.IO.File.Exists(filePath))
            {
                return false;
            }

            System.IO.File.Delete(filePath);

            return true;
        }

        private void SendEmailCampanhaAdded(Campanha campanha)
        {
            var userMail = _user.GetUserEmail();

            try
            {
                MailMessage message = new MailMessage("contato@doadoresespeciais.com.br", userMail);
                message.Subject = "Campanha adicionada - Doadores Especiais";
                message.Body = "<div style='font-size: 12px; font-family: Verdana; background-color: #f8f8f8; margin-left: 20px;'>" +
                    "<img src='https://www.doadoresespeciais.com.br/assets/img/logo.png' style='width: 300px;'/><br/><br/>" +
                    "<h2>Obrigado por criar a sua campanha</h2></br></br> <h3><u>Confira os detalhes abaixo: </u></h2><br/><br/> " +
                    "<p><b>Título: </b>" + campanha.Titulo + "</p>" +
                    "<p><b>Data de Criação: </b>" + campanha.DataCriacao + "</p>" +
                    "<p><b>Duração: </b>" + campanha.DuracaoDias + " dias </p>" +
                    "<p><b>Breve Descrição: </b>" + campanha.DescricaoCurta + "</p>" +
                    "<p><b>Valor Desejado: </b>R$ " + campanha.ValorDesejado + "</p>" +
                    "<p><b>Status da Campanha:</b> " + campanha.StatusCampanha + "</p><br/><br/>" +
                    "<h4>Para que nossa equipe possa autorizar, acesse MINHAS CAMPANHAS no menu da plataforma e envie para análise.</h4></div>";

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

        private void SendEmailCampanhaEnviadaPraAnalise()
        {
            var userMail = _user.GetUserEmail();

            try
            {
                MailMessage message = new MailMessage("contato@doadoresespeciais.com.br", userMail);
                message.Subject = "Campanha em análise - Doadores Especiais";
                message.Body = "<div style='font-size: 12px; font-family: Verdana; background-color: #f8f8f8; margin-left: 20px;'>" +
                    "<img src='https://www.doadoresespeciais.com.br/assets/img/logo.png' style='width: 300px;'/><br/><br/>" +
                    "<h2>Sua campanha foi enviada para análise</h2></br></br> <h3><u>O que fazer agora? </u></h2><br/><br/> " +
                    "<p><b>Prazo: </b></br> Nossa equipe irá analisar a fundo sua campanha e em no máximo 48h sua campanha será autorizada ou não.</p>" +
                    "<p><b>Divulgação: </b></br> A divulgação da sua campanha será ponto chave para o sucesso dela. Utiliza as mídias sociais e o contato direto para conseguir arrecadar o valor desejado.</p>" +
                    "<p><b>Acompanhamento: </b></br> Ao criar uma campanha com a Doadores Especiais, fique constantemente de olho para ver se a sua meta está sendo atingida.</p>";

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

        private void SendEmailCampanhaIniciada(Guid usuarioId)
        {
            // Pegar email do usuário
            var usuario = _userManager.FindByIdAsync(usuarioId.ToString()).Result;

            try
            {
                MailMessage message = new MailMessage("contato@doadoresespeciais.com.br", usuario.Email);
                message.Subject = "Campanha Iniciada - Doadores Especiais";
                message.Body = "<div style='font-size: 12px; font-family: Verdana; background-color: #f8f8f8; margin-left: 20px;'>" +
                    "<img src='https://www.doadoresespeciais.com.br/assets/img/logo.png' style='width: 300px;'/><br/><br/>" +
                    "<h2>Sua campanha está EM ANDAMENTO!!! =)</h2></br></br> <h3><u>O que fazer agora? </u></h2><br/><br/> " +
                    "<p><b>Divulgação: </b></br> A divulgação da sua campanha será ponto chave para o sucesso dela. Utiliza as mídias sociais e o contato direto para conseguir arrecadar o valor desejado.</p>" +
                    "<p><b>Acompanhamento: </b></br> Ao criar uma campanha com a Doadores Especiais, fique constantemente de olho para ver se a sua meta está sendo atingida.</p>";

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

        private void SendEmailCampanhaRejeitada(Guid usuarioId)
        {
            // Pegar email do usuário
            var usuario = _userManager.FindByIdAsync(usuarioId.ToString()).Result;

            try
            {
                MailMessage message = new MailMessage("contato@doadoresespeciais.com.br", usuario.Email);
                message.Subject = "Campanha Rejeitada - Doadores Especiais";
                message.Body = "<div style='font-size: 12px; font-family: Verdana; background-color: #f8f8f8; margin-left: 20px;'>" +
                    "<img src='https://www.doadoresespeciais.com.br/assets/img/logo.png' style='width: 300px;'/><br/><br/>" +
                    "<h2>Sua campanha não atendeu aos nossos requisitos e foi REJEITADA!</h2></br></br> <h3><u>O que fazer agora? </u></h2><br/><br/> " +
                    "<p>Se ainda desejar criar a campanha, entre em contato conosco para entender o motivo.</p>" +
                    "<p>Leia atentamente nossas regras e envie novamente sua campanha.</p>";

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

        private void SendEmailCampanhaRetornada(Guid usuarioId)
        {
            // Pegar email do usuário
            var usuario = _userManager.FindByIdAsync(usuarioId.ToString()).Result;

            try
            {
                MailMessage message = new MailMessage("contato@doadoresespeciais.com.br", usuario.Email);
                message.Subject = "Campanha Editável - Doadores Especiais";
                message.Body = "<div style='font-size: 12px; font-family: Verdana; background-color: #f8f8f8; margin-left: 20px;'>" +
                    "<img src='https://www.doadoresespeciais.com.br/assets/img/logo.png' style='width: 300px;'/><br/><br/>" +
                    "<h2>Sua campanha voltou ao status de EDITÁVEL</h2></br></br> <h3><u>O que fazer agora? </u></h2><br/><br/> " +
                    "<p>Você está livre para realizar as alterações necessárias. Fique a vontade e depois envie sua campanha novamente para análise.</p>";

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

        private void SendEmailCampanhaFinalizada(Guid usuarioId, string titulo)
        {
            // Pegar email do usuário
            var usuario = _userManager.FindByIdAsync(usuarioId.ToString()).Result;

            try
            {
                MailMessage message = new MailMessage("contato@doadoresespeciais.com.br", usuario.Email);
                message.Subject = "Campanha Finalizada - Doadores Especiais";
                message.Body = "<div style='font-size: 12px; font-family: Verdana; background-color: #f8f8f8; margin-left: 20px;'>" +
                    "<img src='https://www.doadoresespeciais.com.br/assets/img/logo.png' style='width: 300px;'/><br/><br/>" +
                    "<h2>Sua campanha está Finalizada!!</h2></br></br> <h3><u>O que fazer agora? </u></h2><br/><br/> " +
                    "<p><b>Título: </b>" + titulo + "</p>" +
                    "<p><b>Pagamento: </b></br> Nossa equipe entrará em contato para falar sobre o envio do valor disponibilizado para o beneficiário.</p>" +
                    "<p><b>Prazo: </b></br> Segundo nossos termos e mediante liberação da Pagarme, nossa operadora de pagamentos, solicitamos um prazo de 15 a 30 dias para a liberação do valor.</p>";

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
        #endregion
    }
}