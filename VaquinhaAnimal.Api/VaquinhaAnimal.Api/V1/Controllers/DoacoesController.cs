using AutoMapper;
using VaquinhaAnimal.Api.Controllers;
using VaquinhaAnimal.Api.ViewModels;
using VaquinhaAnimal.Domain.Entities;
using VaquinhaAnimal.Domain.Entities.Pagarme;
using VaquinhaAnimal.Domain.Helpers;
using VaquinhaAnimal.Domain.Interfaces;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace VaquinhaAnimal.App.V1.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/doacoes")]
    public class DoacoesController : MainController
    {
        #region VARIABLES
        private readonly IDoacaoRepository _doacaoRepository;
        private readonly IDoacaoService _doacaoService;
        private readonly IMapper _mapper;
        private readonly IIdentityRepository _identityRepository;
        private readonly IUser _user;
        static BaseFont fonteBase = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, false);
        #endregion

        #region CONSTRUCTOR
        public DoacoesController(IDoacaoRepository doacaoRepository,
                                 IDoacaoService doacaoService,
                                 IIdentityRepository identityRepository,
                                 IMapper mapper,
                                 INotificador notificador, IUser user) : base(notificador, user)
        {
            _doacaoRepository = doacaoRepository;
            _mapper = mapper;
            _user = user;
            _identityRepository = identityRepository;
            _doacaoService = doacaoService;
        }
        #endregion

        #region CRUD
        [HttpPost]
        public async Task<ActionResult<PagarmePedido>> Adicionar(PagarmePedido pedido)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            foreach (var item in pedido.items)
            {
                item.code = "1";
            }

            try
            {
                //Set Basic Auth
                var userPagarme = test_key;
                var password = "";
                var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userPagarme}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //foreach (var item in pedido.items)
                //{
                //    itensPedido.Add(new PagarmePedidoItens { 
                //        amount = item.amount,
                //        description = item.description,
                //        quantity = item.quantity
                //    });
                //}

                //foreach (var pagamento in pedido.payments)
                //{
                //    pagamentos.Add(new PagarmePedidoPagamentos
                //    {
                //        payment_method = pagamento.payment_method,
                //        credit_card = new PagarmePedidoCartaoCredito
                //        {
                //            recurrence = pagamento.credit_card.recurrence,
                //            installments = pagamento.credit_card.installments,
                //            statement_descriptor = pagamento.credit_card.statement_descriptor,
                //            card = new PagarmePedidoCartaoCreditoUsado
                //            {
                //                number = pagamento.credit_card.card.number,
                //                cvv = pagamento.credit_card.card.cvv,
                //                exp_month = pagamento.credit_card.card.exp_month,
                //                exp_year = pagamento.credit_card.card.exp_year,
                //                holder_name = pagamento.credit_card.card.holder_name,
                //                billing_address = new PagarmePedidoBillingAddress
                //                {
                //                    line_1 = pagamento.credit_card.card.billing_address.line_1,
                //                    city = pagamento.credit_card.card.billing_address.city,
                //                    country = pagamento.credit_card.card.billing_address.country,
                //                    state = pagamento.credit_card.card.billing_address.state,
                //                    zip_code = pagamento.credit_card.card.billing_address.zip_code
                //                }
                //            }
                //        }
                //    });
                //}

                var usuarioId = _user.GetUserId(); // Pega o ID do usuário
                var idPagarme = _identityRepository.GetCodigoPagarme(usuarioId.ToString()); // Pega o ID da Pagarme do usuário
                
                //var pedidoToAdd = new PagarmePedido()
                //{
                //    customer_id = idPagarme,
                //    items = itensPedido,
                //    payments = pagamentos
                //};                

                HttpResponseMessage response = await client.PostAsJsonAsync(urlPagarme + "orders/", pedido);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                //var pedidoRecebido = JsonConvert.DeserializeObject<PagarmeCardResponse>(responseBody);

                // APÓS ADICIONADO NA PAGARME, ADICIONANDO PEDIDO NO BANCO DE DADOS
                //var cartaoAdd = new Cartao
                //{
                //    Customer_Id = idPagarme,
                //    Card_Id = cartaoRecebido.Id,
                //    First_Six_Digits = cartaoRecebido.First_Six_Digits,
                //    Last_Four_Digits = cartaoRecebido.Last_Four_Digits,
                //    Exp_Month = cartaoRecebido.Exp_Month,
                //    Exp_Year = cartaoRecebido.Exp_Year,
                //    Status = cartaoRecebido.Status
                //};

                //await _cartaoService.Adicionar(cartaoAdd);

                return Ok(responseBody);
            }
            catch (HttpRequestException e)
            {
                return BadRequest("Erro: " + e);
            }

            //await _doacaoService.Adicionar(_mapper.Map<Doacao>(doacaoViewModel));

            //return CustomResponse(doacaoViewModel);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<DoacaoViewModel>> Atualizar(Guid id, DoacaoViewModel doacaoViewModel)
        {
            if (id != doacaoViewModel.id)
            {
                NotificarErro("Os ids informados não são iguais!");
                return CustomResponse();
            }

            var doacaoAtualizacao = await ObterDoacao(id);

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            doacaoAtualizacao.data = doacaoViewModel.data;
            doacaoAtualizacao.valor = doacaoViewModel.valor;
            doacaoAtualizacao.customer_id = doacaoViewModel.customer_id;
            doacaoAtualizacao.transacao_id = doacaoViewModel.transacao_id;
            doacaoAtualizacao.usuario_id = doacaoViewModel.usuario_id;
            doacaoAtualizacao.forma_pagamento = doacaoViewModel.forma_pagamento;
            doacaoAtualizacao.status = doacaoViewModel.status;
            doacaoAtualizacao.campanha_id = doacaoViewModel.campanha_id;
            doacaoAtualizacao.charge_id = doacaoViewModel.charge_id;
            
            await _doacaoService.Atualizar(_mapper.Map<Doacao>(doacaoAtualizacao));

            return CustomResponse(doacaoViewModel);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<DoacaoViewModel>> Excluir(Guid id)
        {
            var doacao = await ObterDoacao(id);

            if (doacao == null)
            {
                NotificarErro("O id da doação não foi encontrado.");
                return CustomResponse(doacao);
            }

            await _doacaoService.Remover(id);

            return CustomResponse(doacao);
        }
        #endregion

        #region METHODS
        [HttpGet]
        public async Task<List<DoacaoViewModel>> ObterTodos()
        {
            return _mapper.Map<List<DoacaoViewModel>>(await _doacaoRepository.GetAllAsync());
        }

        [AllowAnonymous]
        [HttpGet("total-doadores/{campanhaId:guid}")]
        public async Task<int> ObterTotalDoadoresPorCampanha(Guid campanhaId)
        {
            var result = await _doacaoRepository.ObterTotalDoadoresPorCampanha(campanhaId);

            return result;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<DoacaoViewModel>> ObterDoacaoPorId(Guid id)
        {
            var doacao = _mapper.Map<DoacaoViewModel>(await _doacaoRepository.GetByIdAsync(id));

            if (doacao == null) return NotFound();

            return doacao;
        }

        private async Task<DoacaoViewModel> ObterDoacao(Guid id)
        {
            return _mapper.Map<DoacaoViewModel>(await _doacaoRepository.GetByIdAsync(id));
        }

        [HttpGet("minhas-doacoes")]
        public async Task<List<DoacaoViewModel>> ObterMinhasDoacoes()
        {
            var usuarioLogadoId = _user.GetUserId();
            var doacoesToReturn = await _doacaoRepository.GetAllMyDonationsAsync(usuarioLogadoId);
            return _mapper.Map<List<DoacaoViewModel>>(doacoesToReturn);
        }

        [HttpGet("export-to-pdf/{campanhaId:guid}")]
        public void GerarRelatorioPdf(Guid campanhaId)
        {
            // PEGAR DOACOES DA CAMPANHA ENVIADA
            var doacoes = _doacaoRepository.ObterDoacoesDaCampanha(campanhaId).GetAwaiter().GetResult().OrderBy(x => x.Data).ToList(); ;

            if (doacoes.Count > 0)
            {
                // CALCULAR TOTAL DE PÁGINAS
                int totalPaginas = 1;
                int totalLinhas = doacoes.Count;
                if (totalLinhas > 24)
                {
                    totalPaginas += (int)Math.Ceiling((totalLinhas - 24) / 29F);
                }

                // CONFIGURAÇÃO DO DOCUMENTO
                var pxPorMm = 72 / 25.2F;
                var pdf = new Document(PageSize.A4.Rotate(), 15 * pxPorMm, 15 * pxPorMm, 15 * pxPorMm, 20 * pxPorMm);
                var path = $"doacoes.{DateTime.Now.ToString("dd.MM.yyyy.HH.mm")}.pdf";
                var arquivo = new FileStream(path, FileMode.Create);
                var writer = PdfWriter.GetInstance(pdf, arquivo);
                writer.PageEvent = new EventoDePagina(totalPaginas);
                pdf.Open();

                // ADICIONANDO TÍTULO
                var fonteTitulo = new iTextSharp.text.Font(fonteBase, 28, iTextSharp.text.Font.NORMAL, BaseColor.Black);
                var titulo = new Paragraph("Relatório de Doações\n\n", fonteTitulo);
                titulo.Alignment = Element.ALIGN_LEFT;
                titulo.SpacingAfter = 4;
                pdf.Add(titulo);

                // ADICIONANDO A LOGOMARCA
                var pathImagem = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/logomarca.png");
                if (System.IO.File.Exists(pathImagem))
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(pathImagem);
                    float razaoLarguraAltura = logo.Width / logo.Height;
                    float alturaLogo = 60;
                    float larguraLogo = alturaLogo * razaoLarguraAltura;
                    logo.ScaleToFit(larguraLogo, alturaLogo);
                    var margemEsquerda = pdf.PageSize.Width - pdf.RightMargin - larguraLogo;
                    var margemTopo = pdf.PageSize.Height - pdf.TopMargin - 54;
                    logo.SetAbsolutePosition(margemEsquerda, margemTopo);
                    writer.DirectContent.AddImage(logo, false);
                }

                // ADICIONANDO A TABELA
                var tabela = new PdfPTable(7);
                float[] largurasColunas = { 0.5f, 1.0f, 1.0f, 1.2f, 1.0f, 1.0f, 1.0f };
                tabela.SetWidths(largurasColunas);
                tabela.DefaultCell.BorderWidth = 0;
                tabela.WidthPercentage = 100;

                // ADICIONAR TÍTULOS
                CriarCelulaTexto(tabela, "Data", PdfPCell.ALIGN_LEFT, true);
                CriarCelulaTexto(tabela, "Valor Doado", PdfPCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "Forma de Pagamento", PdfPCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "ID da Transação", PdfPCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "Plataforma (3%)", PdfPCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "Taxa da Operadora", PdfPCell.ALIGN_CENTER, true);
                CriarCelulaTexto(tabela, "Beneficiário", PdfPCell.ALIGN_CENTER, true);

                // ADICIONAR DADOS
                foreach (var doacao in doacoes)
                {
                    CriarCelulaTexto(tabela, doacao.Data.ToString("dd/MM/yyyy"), PdfPCell.ALIGN_LEFT);
                    CriarCelulaTexto(tabela, "R$ " + doacao.Valor.ToString(), PdfPCell.ALIGN_CENTER);

                    if (doacao.FormaPagamento == "billing")
                    {
                        CriarCelulaTexto(tabela, "Boleto", PdfPCell.ALIGN_CENTER);
                    } 
                    else if (doacao.FormaPagamento == "pix")
                    {
                        CriarCelulaTexto(tabela, "PIX", PdfPCell.ALIGN_CENTER);
                    }
                    else if (doacao.FormaPagamento == "credit_card")
                    {
                        CriarCelulaTexto(tabela, "Cartão de Crédito", PdfPCell.ALIGN_CENTER);
                    }

                    CriarCelulaTexto(tabela, doacao.Transacao_Id, PdfPCell.ALIGN_CENTER);
                    CriarCelulaTexto(tabela, "R$ " + doacao.ValorPlataforma.ToString(), PdfPCell.ALIGN_CENTER);
                    CriarCelulaTexto(tabela, "R$ " + doacao.ValorTaxa.ToString(), PdfPCell.ALIGN_CENTER);
                    CriarCelulaTexto(tabela, "R$ " + doacao.ValorBeneficiario.ToString(), PdfPCell.ALIGN_CENTER);
                }

                pdf.Add(tabela);

                // FECHANDO O PDF
                pdf.Close();
                arquivo.Close();

                // ABRINDO O ARQUIVO
                var caminhoPdf = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/docs/" + path);
                if (System.IO.File.Exists(caminhoPdf))
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        Arguments = $"/c start {caminhoPdf}",
                        FileName = "cmd.exe",
                        CreateNoWindow = true
                    });
                }
            }
        }

        static void CriarCelulaTexto(PdfPTable tabela, string texto, int alinhamentoHorz = PdfPCell.ALIGN_LEFT,
                                                 bool negrito = false, bool italico = false, int tamanhoFonte = 10, int alturaCelula = 25)
        {
            int estilo = iTextSharp.text.Font.NORMAL;

            if (negrito && italico)
            {
                estilo = iTextSharp.text.Font.BOLDITALIC;
            }
            else if (negrito)
            {
                estilo = iTextSharp.text.Font.BOLD;
            }
            else if (italico)
            {
                estilo = iTextSharp.text.Font.ITALIC;
            }

            var fonteCelula = new iTextSharp.text.Font(fonteBase, tamanhoFonte, estilo, BaseColor.Black);

            var bgColor = BaseColor.White;
            if (tabela.Rows.Count % 2 == 1)
                bgColor = new BaseColor(0.95F, 0.95F, 0.95F);

            var celula = new PdfPCell(new Phrase(texto, fonteCelula));
            celula.HorizontalAlignment = alinhamentoHorz;
            celula.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            celula.Border = 0;
            celula.BorderWidthBottom = 1;
            celula.FixedHeight = alturaCelula;
            celula.PaddingBottom = 5;
            celula.BackgroundColor = bgColor;
            tabela.AddCell(celula);
        }
        #endregion
    }
}