using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using VaquinhaAnimal.Api.Controllers;
using VaquinhaAnimal.Api.ViewModels;
using VaquinhaAnimal.Domain.Entities;
using VaquinhaAnimal.Domain.Entities.Base;
using VaquinhaAnimal.Domain.Entities.Pagarme;
using VaquinhaAnimal.Domain.Interfaces;

namespace VaquinhaAnimal.App.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/transacoes")]
    public class PagarmeController : MainController
    {
        #region VARIABLES
        private readonly IUser _user;
        private readonly IIdentityRepository _identityRepository;
        private readonly IDoacaoService _doacaoService;
        private readonly ICartaoService _cartaoService;
        private readonly ICartaoRepository _cartaoRepository;
        private readonly IDoacaoRepository _doacaoRepository;
        private readonly ICampanhaService _campanhaService;
        private readonly ISignalR _signalR;
        private readonly ICampanhaRepository _campanhaRepository;
        #endregion

        #region CONSTRUCTOR
        public PagarmeController(INotificador notificador,
                                 IUser user,
                                 IDoacaoService doacaoService,
                                 ICartaoService cartaoService,
                                 ICartaoRepository cartaoRepository,
                                 IDoacaoRepository doacaoRepository,
                                 ICampanhaRepository campanhaRepository,
                                 ICampanhaService campanhaService,
                                 ISignalR signalR,
                                 IIdentityRepository identityRepository)
                                 : base(notificador, user)
        {
            _user = user;
            _identityRepository = identityRepository;
            _doacaoService = doacaoService;
            _cartaoService = cartaoService;
            _cartaoRepository = cartaoRepository;
            _doacaoRepository = doacaoRepository;
            _campanhaService = campanhaService;
            _campanhaRepository = campanhaRepository;
            _signalR = signalR;
        }
        #endregion

        #region PAGARME ---> CARTÕES
        [HttpPost("add-card")]
        public async Task<ActionResult> AddCardPagarme(PagarmeCard card)
        {
            try
            {
                AddHeaderPagarme();
                var idPagarme = GetUserAndPagarmeId();

                HttpResponseMessage response = await client.PostAsJsonAsync(urlPagarme + "customers/" + idPagarme + "/cards", card);
                string teste = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(responseBody);

                // ADICIONANDO O CARTÃO COM O RESPONSE RECEBIDO
                var cartaoToAdd = new Cartao
                {
                    Card_Id = (string)obj["id"],
                    Exp_Month = (int)obj["exp_month"],
                    Exp_Year = (int)obj["exp_year"],
                    First_Six_Digits = (string)obj["first_six_digits"],
                    Last_Four_Digits = (string)obj["last_four_digits"],
                    Customer_Id = GetUserAndPagarmeId(),
                    Status = (string)obj["status"]
                };

                var adicionandoCartao = await _cartaoService.Adicionar(cartaoToAdd);

                return Ok(obj);
            }
            catch (HttpRequestException e)
            {
                return BadRequest("Erro: " + e);
            }
        }

        [HttpGet("list-user-card")]
        public async Task<object> ListUserCards()
        {
            var customerId = GetUserAndPagarmeId();

            var result = await _cartaoRepository.GetAllCardsAsync(customerId);

            return result;
        }

        [HttpGet("list-card")]
        public async Task<object> ListCardPagarme()
        {
            AddHeaderPagarme();
            var idPagarme = GetUserAndPagarmeId();

            HttpResponseMessage response = await client.GetAsync(urlPagarme + "customers/" + idPagarme + "/cards");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var cartoes = JsonConvert.DeserializeObject<PagarmeResponse<PagarmeCardResponse>>(responseBody);

            return cartoes;
        }

        [HttpDelete("delete-card/{cardId}")]
        public async Task<ActionResult> DeleteCardPagarme(string cardId)
        {
            try
            {
                AddHeaderPagarme();
                var idPagarme = GetUserAndPagarmeId();

                HttpResponseMessage response = await client.DeleteAsync(urlPagarme + "customers/" + idPagarme + "/cards/" + cardId);
                string responseBody = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();

                return Ok(responseBody);
            }
            catch (HttpRequestException e)
            {
                return BadRequest("Erro: " + e);
            }
        }
        #endregion

        #region PAGARME ---> PEDIDOS
        [HttpPost("add-order/{campanhaId}")]
        public async Task<ActionResult> AddOrderPagarme(PagarmePedido order, Guid campanhaId)
        {
            try
            {
                AddHeaderPagarme();

                var valorDoacao = order.items[0].amount;
                var valorPlataforma = Math.Round(0.03 * valorDoacao + 0.99, 2);
                var valorBeneficiario = valorDoacao - valorPlataforma;
                
                order.customer_id = GetUserAndPagarmeId();
                order.items[0].amount = valorDoacao * 100;

                // CALCULAR TAXAS DO SPLIT
                var campanhaRecebedora = await _campanhaRepository.GetByIdWithImagesAsync(campanhaId);
                order.items[0].code = campanhaRecebedora.Titulo;

                order.payments[0].split = new List<PagarmePedidoSplit>();

                // SPLIT RECEBEDOR PLATAFORMA --> 3% + R$ 0,99
                order.payments[0].split.Add(new PagarmePedidoSplit
                {
                    //recipient_id = "rp_V0YW6MvI3I84xj59", // TESTE
                    recipient_id = "rp_JW8g38RmcPfqpPxj", // PRODUÇÃO
                    amount = Convert.ToInt32(valorPlataforma * 100),
                    type = "flat",
                    options = new PagarmeSplitOptions
                    {
                        charge_processing_fee = false,
                        charge_remainder_fee = true,
                        liable = false
                    }
                });

                // SPLIT RECEBEDOR BENEFICIÁRIO --> Restante - taxas
                order.payments[0].split.Add(new PagarmePedidoSplit
                {
                    recipient_id = campanhaRecebedora.Beneficiario.RecebedorId,
                    amount = Convert.ToInt32(valorBeneficiario * 100),
                    type = "flat",
                    options = new PagarmeSplitOptions
                    {
                        charge_processing_fee = true,
                        charge_remainder_fee = false,
                        liable = true
                    }
                });

                HttpResponseMessage response = await client.PostAsJsonAsync(urlPagarme + "orders/", order);
                string responseBody2 = await response.Content.ReadAsStringAsync();
                JObject obj2 = JObject.Parse(responseBody2);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(responseBody);

                if ((string)obj["status"] == "failed")
                {
                    return BadRequest("Erro na transação. Tente novamente ou mude o cartão!");
                }

                // ADICIONANDO A DOAÇÃO COM O RESPONSE RECEBIDO
                var doacaoToAdd = new Doacao
                {
                    Data = (DateTime)obj["created_at"],
                    FormaPagamento = (string)obj.SelectToken("charges[0].payment_method"),
                    Status = (string)obj["status"],
                    Campanha_Id = campanhaId,
                    Transacao_Id = (string)obj["id"],
                    Usuario_Id = _user.GetUserId().ToString(),
                    Valor = ((decimal)obj["amount"]) / 100,
                    ValorPlataforma = (decimal)valorPlataforma,
                    ValorBeneficiario = (decimal)(valorDoacao - valorPlataforma - (valorDoacao * 0.0449)),
                    ValorTaxa = (decimal)(valorDoacao * 0.0449),
                    Customer_Id = GetUserAndPagarmeId(),
                    Charge_Id = (string)obj.SelectToken("charges[0].id"),
                    Url_Download = (string)obj.SelectToken("charges[0].last_transaction.pdf") // CASO DE BOLETO
                };

                var adicionandoDoacao = await _doacaoService.Adicionar(doacaoToAdd);

                // SE FALHAR O INSERT DE DOAÇÃO, RETORNA ERRO E DELETA A COBRANÇA NA PAGARME
                if (adicionandoDoacao == false)
                {
                    await DeleteCharge(doacaoToAdd.Charge_Id);

                    return BadRequest("Sua doação não pôde ser adicionada.");
                }

                if ((string)obj["status"] == "paid")
                {
                    var campanhaPraAlterar = await _campanhaRepository.GetByIdAsync(doacaoToAdd.Campanha_Id);
                    campanhaPraAlterar.TotalArrecadado += doacaoToAdd.Valor;

                    var alterandoCampanha = await _campanhaService.Atualizar(campanhaPraAlterar);

                    // SE DER ERRO PARA ATUALIZAR O SALDO DA CAMPANHA
                    if (alterandoCampanha == false)
                    {
                        // DELETA A COBRANÇA NA PAGARME
                        await DeleteCharge(doacaoToAdd.Charge_Id);

                        // DELETA A DOAÇÃO QUE JÁ HAVIA SIDO ADICIONADA AO BANCO
                        var doacaoToRemove = await _doacaoRepository.ObterDoacaoPelaCobranca((string)obj.SelectToken("charges[0].id"));
                        await _doacaoService.Remover(doacaoToRemove.Id);

                        return BadRequest("Sua doação não pôde ser adicionada.");
                    }
                }

                return CustomResponse((string)obj["status"]);
            }
            catch (HttpRequestException e)
            {
                return BadRequest("Erro: " + e);
            }
        }

        [HttpPost("add-order-cartao-novo/{campanhaId}")]
        public async Task<ActionResult> AddOrderCartaoNovoPagarme(PagarmePedidoCartaoNovo order, Guid campanhaId)
        {
            try
            {
                AddHeaderPagarme();

                var valorDoacao = order.items[0].amount;
                var valorPlataforma = Math.Round(0.03 * valorDoacao + 0.99, 2);
                var valorBeneficiario = valorDoacao - valorPlataforma;

                order.customer_id = GetUserAndPagarmeId();
                order.items[0].amount = valorDoacao * 100;

                // CALCULAR TAXAS DO SPLIT
                var campanhaRecebedora = await _campanhaRepository.GetByIdWithImagesAsync(campanhaId);
                order.items[0].code = campanhaRecebedora.Titulo;

                order.payments[0].split = new List<PagarmePedidoCartaoCreditoCartaoNovoSplit>();

                // SPLIT RECEBEDOR PLATAFORMA --> 3% + R$ 0,99
                order.payments[0].split.Add(new PagarmePedidoCartaoCreditoCartaoNovoSplit
                {
                    //recipient_id = "rp_V0YW6MvI3I84xj59", // TESTE
                    recipient_id = "rp_JW8g38RmcPfqpPxj", // PRODUÇÃO
                    amount = Convert.ToInt32(valorPlataforma * 100),
                    type = "flat",
                    options = new PagarmePedidoCartaoCreditoCartaoNovoOptions
                    {
                        charge_processing_fee = false,
                        charge_remainder_fee = true,
                        liable = false
                    }
                });

                // SPLIT RECEBEDOR BENEFICIÁRIO --> Restante - taxas
                order.payments[0].split.Add(new PagarmePedidoCartaoCreditoCartaoNovoSplit
                {
                    recipient_id = campanhaRecebedora.Beneficiario.RecebedorId,
                    amount = Convert.ToInt32(valorBeneficiario * 100),
                    type = "flat",
                    options = new PagarmePedidoCartaoCreditoCartaoNovoOptions
                    {
                        charge_processing_fee = true,
                        charge_remainder_fee = false,
                        liable = true
                    }
                });

                HttpResponseMessage response = await client.PostAsJsonAsync(urlPagarme + "orders/", order);
                string responseBody2 = await response.Content.ReadAsStringAsync();
                JObject obj2 = JObject.Parse(responseBody2);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(responseBody);

                if ((string)obj["status"] == "failed")
                {
                    return BadRequest("Erro na transação. Tente novamente ou mude o cartão!");
                }

                // ADICIONANDO A DOAÇÃO COM O RESPONSE RECEBIDO
                var doacaoToAdd = new Doacao
                {
                    Data = (DateTime)obj["created_at"],
                    FormaPagamento = (string)obj.SelectToken("charges[0].payment_method"),
                    Status = (string)obj["status"],
                    Campanha_Id = campanhaId,
                    Transacao_Id = (string)obj["id"],
                    Usuario_Id = _user.GetUserId().ToString(),
                    Valor = ((decimal)obj["amount"]) / 100,
                    ValorPlataforma = (decimal)valorPlataforma,
                    ValorBeneficiario = (decimal)(valorDoacao - valorPlataforma - (valorDoacao * 0.0449)),
                    ValorTaxa = (decimal)(valorDoacao * 0.0449),
                    Customer_Id = GetUserAndPagarmeId(),
                    Charge_Id = (string)obj.SelectToken("charges[0].id"),
                    Url_Download = (string)obj.SelectToken("charges[0].last_transaction.pdf") // CASO DE BOLETO
                };

                var adicionandoDoacao = await _doacaoService.Adicionar(doacaoToAdd);

                // SE FALHAR O INSERT DE DOAÇÃO, RETORNA ERRO E DELETA A COBRANÇA NA PAGARME
                if (adicionandoDoacao == false)
                {
                    await DeleteCharge(doacaoToAdd.Charge_Id);

                    return BadRequest("Sua doação não pôde ser adicionada.");
                }

                if ((string)obj["status"] == "paid")
                {
                    var campanhaPraAlterar = await _campanhaRepository.GetByIdAsync(doacaoToAdd.Campanha_Id);
                    campanhaPraAlterar.TotalArrecadado += doacaoToAdd.Valor;

                    var alterandoCampanha = await _campanhaService.Atualizar(campanhaPraAlterar);

                    // SE DER ERRO PARA ATUALIZAR O SALDO DA CAMPANHA
                    if (alterandoCampanha == false)
                    {
                        // DELETA A COBRANÇA NA PAGARME
                        await DeleteCharge(doacaoToAdd.Charge_Id);

                        // DELETA A DOAÇÃO QUE JÁ HAVIA SIDO ADICIONADA AO BANCO
                        var doacaoToRemove = await _doacaoRepository.ObterDoacaoPelaCobranca((string)obj.SelectToken("charges[0].id"));
                        await _doacaoService.Remover(doacaoToRemove.Id);

                        return BadRequest("Sua doação não pôde ser adicionada.");
                    }
                }

                //if (order.salvarCartao == true)
                //{
                //    // ADICIONAR CARTÃO
                //    var cartaoToAdd = new PagarmeCard
                //    {
                //        Number = order.payments[0].credit_card.card.number,
                //        Holder_Name = order.payments[0].credit_card.card.holder_name,
                //        Holder_Document = order.payments[0].credit_card.card.holder_document,
                //        Exp_Year = order.payments[0].credit_card.card.exp_year,
                //        Exp_Month = order.payments[0].credit_card.card.exp_month,
                //        CVV = order.payments[0].credit_card.card.cvv,
                //        Brand = order.payments[0].credit_card.card.brand,
                //        Billing_Address = new PagarmeCardBillingAddress
                //        {
                //            City = order.payments[0].credit_card.card.billing_address.city,
                //            State = order.payments[0].credit_card.card.billing_address.state,
                //            Country = order.payments[0].credit_card.card.billing_address.country,
                //            Line_1 = order.payments[0].credit_card.card.billing_address.line_1,
                //            Line_2 = "",
                //            Zip_Code = order.payments[0].credit_card.card.billing_address.zip_code
                //        }
                //    };

                //    HttpResponseMessage responseCartao = await client.PostAsJsonAsync(urlPagarme + "customers/" + order.customer_id + "/cards", cartaoToAdd);
                //    response.EnsureSuccessStatusCode();
                //    string responseCartaoBody = await response.Content.ReadAsStringAsync();
                //    JObject objCartao = JObject.Parse(responseBody);
                //}

                return CustomResponse((string)obj["status"]);
            }
            catch (HttpRequestException e)
            {
                return BadRequest("Erro: " + e);
            }
        }

        [HttpPost("add-order-boleto/{campanhaId}")]
        public async Task<ActionResult> AddOrderPagarmeBoleto(PagarmePedidoBoleto order, Guid campanhaId)
        {
            if (order.items[0].amount < 10)
            {
                NotificarErro("Valor mínimo de doação em boleto: R$ 10,00");
                return CustomResponse();
            }

            try
            {
                AddHeaderPagarme();

                var valorDoacao = order.items[0].amount;
                var valorPlataforma = Math.Round(0.03 * valorDoacao, 2);
                var valorBeneficiario = valorDoacao - valorPlataforma;

                order.customer_id = GetUserAndPagarmeId();
                order.items[0].amount = valorDoacao * 100;

                // CALCULAR TAXAS DO SPLIT
                var campanhaRecebedora = await _campanhaRepository.GetByIdWithImagesAsync(campanhaId);
                order.items[0].code = campanhaRecebedora.Titulo;

                order.payments[0].split = new List<PagarmePedidoBoletoSplit>();

                // SPLIT RECEBEDOR PLATAFORMA --> 3%
                order.payments[0].split.Add(new PagarmePedidoBoletoSplit
                {
                    //recipient_id = "rp_V0YW6MvI3I84xj59", // TESTE
                    recipient_id = "rp_JW8g38RmcPfqpPxj", // PRODUÇÃO
                    amount = Convert.ToInt32(valorPlataforma * 100),
                    type = "flat",
                    options = new PagarmeSplitBoletoOptions
                    {
                        charge_processing_fee = false,
                        charge_remainder_fee = true,
                        liable = false
                    }
                });

                // SPLIT RECEBEDOR BENEFICIÁRIO --> Restante - taxa de 3,49
                order.payments[0].split.Add(new PagarmePedidoBoletoSplit
                {
                    recipient_id = campanhaRecebedora.Beneficiario.RecebedorId,
                    amount = Convert.ToInt32(valorBeneficiario * 100),
                    type = "flat",
                    options = new PagarmeSplitBoletoOptions
                    {
                        charge_processing_fee = true,
                        charge_remainder_fee = false,
                        liable = true
                    }
                });

                HttpResponseMessage response = await client.PostAsJsonAsync(urlPagarme + "orders/", order);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(responseBody);

                if ((string)obj["status"] == "failed")
                {
                    return BadRequest("Erro na transação. Tente novamente ou mude o cartão!");
                }

                // ADICIONANDO A DOAÇÃO COM O RESPONSE RECEBIDO
                var doacaoToAdd = new Doacao
                {
                    Data = (DateTime)obj["created_at"],
                    FormaPagamento = (string)obj.SelectToken("charges[0].payment_method"),
                    Status = (string)obj["status"],
                    Campanha_Id = campanhaId,
                    Transacao_Id = (string)obj["id"],
                    Usuario_Id = _user.GetUserId().ToString(),
                    Valor = ((decimal)obj["amount"]) / 100,
                    ValorPlataforma = (decimal)valorPlataforma,
                    ValorBeneficiario = (decimal)(valorDoacao - valorPlataforma - 3.49),
                    ValorTaxa = 3.49m,
                    Customer_Id = GetUserAndPagarmeId(),
                    Url_Download = (string)obj.SelectToken("charges[0].last_transaction.pdf"),
                    Charge_Id = (string)obj.SelectToken("charges[0].id")
                };

                var adicionandoDoacao = await _doacaoService.Adicionar(doacaoToAdd);

                // SE FALHAR O INSERT DE DOAÇÃO, RETORNA ERRO E DELETA A COBRANÇA NA PAGARME
                if (adicionandoDoacao == false)
                {
                    await DeleteCharge(doacaoToAdd.Charge_Id);

                    return BadRequest("Sua doação não pôde ser adicionada.");
                }

                if ((string)obj["status"] == "paid")
                {
                    var campanhaPraAlterar = await _campanhaRepository.GetByIdAsync(doacaoToAdd.Campanha_Id);
                    campanhaPraAlterar.TotalArrecadado += doacaoToAdd.Valor;

                    var alterandoCampanha = await _campanhaService.Atualizar(campanhaPraAlterar);

                    if (alterandoCampanha == false)
                    {
                        await DeleteCharge(doacaoToAdd.Charge_Id);
                        //await _doacaoService.Remover(); COMO PEGAR O ID DA DOACAO?

                        return BadRequest("Sua doação não pôde ser adicionada.");
                    }
                }

                return CustomResponse(obj);
            }
            catch (HttpRequestException e)
            {
                return BadRequest("Erro: " + e);
            }
        }

        [HttpPost("add-order-pix/{campanhaId}")]
        public async Task<ActionResult> AddOrderPagarmePix(PagarmePedidoPix order, Guid campanhaId)
        {
            try
            {
                AddHeaderPagarme();

                var valorDoacao = order.items[0].amount;
                var valorPlataforma = Math.Round(0.03 * valorDoacao, 2);
                var valorBeneficiario = valorDoacao - valorPlataforma;

                order.customer_id = GetUserAndPagarmeId();
                order.items[0].amount = valorDoacao * 100;

                // CALCULAR TAXAS DO SPLIT
                var campanhaRecebedora = await _campanhaRepository.GetByIdWithImagesAsync(campanhaId);
                order.items[0].code = campanhaRecebedora.Titulo;

                order.payments[0].split = new List<PagarmePedidoPixSplit>();

                // SPLIT RECEBEDOR PLATAFORMA --> 3%
                order.payments[0].split.Add(new PagarmePedidoPixSplit
                {
                    //recipient_id = "rp_V0YW6MvI3I84xj59", // TESTE
                    recipient_id = "rp_JW8g38RmcPfqpPxj", // PRODUÇÃO
                    amount = Convert.ToInt32(valorPlataforma * 100),
                    type = "flat",
                    options = new PagarmeSplitPixOptions
                    {
                        charge_processing_fee = false,
                        charge_remainder_fee = true,
                        liable = false
                    }
                });

                // SPLIT RECEBEDOR BENEFICIÁRIO --> Restante - taxa de 1,19%
                order.payments[0].split.Add(new PagarmePedidoPixSplit
                {
                    recipient_id = campanhaRecebedora.Beneficiario.RecebedorId,
                    amount = Convert.ToInt32(valorBeneficiario * 100),
                    type = "flat",
                    options = new PagarmeSplitPixOptions
                    {
                        charge_processing_fee = true,
                        charge_remainder_fee = false,
                        liable = true
                    }
                });

                    HttpResponseMessage response = await client.PostAsJsonAsync(urlPagarme + "orders/", order);
                string responseBody2 = await response.Content.ReadAsStringAsync();
                JObject obj2 = JObject.Parse(responseBody2);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(responseBody);

                if ((string)obj["status"] == "failed")
                {
                    return BadRequest("Erro na transação. Tente novamente ou mude a forma de pagamento!");
                }

                // ADICIONANDO A DOAÇÃO COM O RESPONSE RECEBIDO
                var doacaoToAdd = new Doacao
                {
                    Data = (DateTime)obj["created_at"],
                    FormaPagamento = (string)obj.SelectToken("charges[0].payment_method"),
                    Status = (string)obj["status"],
                    Campanha_Id = campanhaId,
                    Transacao_Id = (string)obj["id"],
                    Usuario_Id = _user.GetUserId().ToString(),
                    Valor = ((decimal)obj["amount"]) / 100,
                    ValorPlataforma = (decimal)valorPlataforma,
                    ValorBeneficiario = (decimal)(valorDoacao - valorPlataforma - (valorDoacao * 0.0119)),
                    ValorTaxa = (decimal)(valorDoacao * 0.0119),
                    Customer_Id = GetUserAndPagarmeId(),
                    Url_Download = (string)obj.SelectToken("charges[0].last_transaction.pdf"),
                    Charge_Id = (string)obj.SelectToken("charges[0].id")
                };

                var adicionandoDoacao = await _doacaoService.Adicionar(doacaoToAdd);

                // SE FALHAR O INSERT DE DOAÇÃO, RETORNA ERRO E DELETA A COBRANÇA NA PAGARME
                if (adicionandoDoacao == false)
                {
                    await DeleteCharge(doacaoToAdd.Charge_Id);

                    return BadRequest("Sua doação não pôde ser adicionada.");
                }

                if ((string)obj["status"] == "paid")
                {
                    var campanhaPraAlterar = await _campanhaRepository.GetByIdAsync(doacaoToAdd.Campanha_Id);
                    campanhaPraAlterar.TotalArrecadado += doacaoToAdd.Valor;

                    var alterandoCampanha = await _campanhaService.Atualizar(campanhaPraAlterar);

                    if (alterandoCampanha == false)
                    {
                        await DeleteCharge(doacaoToAdd.Charge_Id);
                        //await _doacaoService.Remover(); COMO PEGAR O ID DA DOACAO?

                        return BadRequest("Sua doação não pôde ser adicionada.");
                    }
                }

                return CustomResponse((string)obj.SelectToken("charges[0].last_transaction.qr_code_url"));
            }
            catch (HttpRequestException e)
            {
                return BadRequest("Erro: " + e);
            }
        }

        [HttpPost("add-recorrencia")]
        public async Task<ActionResult> AddRecorrenciaPagarme(RecorrenciaViewModel recorrencia)
        {
            try
            {
                AddHeaderPagarme();

                // Criando JSON
                var recorrenciaToSend = new PagarmePedidoRecorrencia()
                {
                    payment_method = "credit_card",
                    currency = "BRL",
                    interval = "month",
                    interval_count = 1,
                    billing_type = "exact_day",
                    billing_day = recorrencia.billing_day,
                    installments = 1,
                    card_id = recorrencia.card_id,
                    customer_id = GetUserAndPagarmeId(),
                    items = new List<RecorrenciaItem>()
                    {
                        new RecorrenciaItem()
                        {
                            description = "Assinatura - DOE",
                            quantity = 1,
                            pricing_scheme = new RecorrenciaScheme()
                            {
                                price = (recorrencia.value * 100)
                            }
                        }
                    }
                };

                HttpResponseMessage response = await client.PostAsJsonAsync(urlPagarme + "subscriptions/", recorrenciaToSend);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(responseBody);

                if ((string)obj["status"] == "failed")
                {
                    return BadRequest("Erro na transação. Tente novamente ou mude o cartão!");
                }

                if ((string)obj["status"] == "paid")
                {
                    return BadRequest("Sua doação não pôde ser adicionada.");
                }

                return Ok(obj);
            }
            catch (HttpRequestException e)
            {
                return BadRequest("Erro: " + e);
            }
        }

        [HttpDelete("delete-recorrencia/{assinaturaId}")]
        public async Task<ActionResult> DeleteRecorrenciaPagarme(string assinaturaId)
        {
            try
            {
                AddHeaderPagarme();

                HttpResponseMessage response = await client.DeleteAsync(urlPagarme + "subscriptions/" + assinaturaId);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(responseBody);

                return Ok(obj);
            }
            catch (HttpRequestException e)
            {
                return BadRequest("Erro: " + e);
            }
        }

        [HttpGet("list-recurrencies")]
        public async Task<object> ListAssinaturasPagarme()
        {
            AddHeaderPagarme();
            var idPagarme = GetUserAndPagarmeId();

            var query = new Dictionary<string, string>
            {
                ["customer_id"] = idPagarme
            };

            HttpResponseMessage response = await client.GetAsync(QueryHelpers.AddQueryString(urlPagarme + "subscriptions", query));
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var assianturas = JsonConvert.DeserializeObject<PagarmeResponse<PagarmeRecurrenciesResponse>>(responseBody);

            return assianturas;
        }
        #endregion

        #region PAGARME ---> WEBHOOKS
        [AllowAnonymous]
        [HttpPost("pedidos-webhook")]
        public async Task<ActionResult> PedidosWebhook([FromBody] WebhookPedidos request)
        {
            // Obter a doação
            var doacao = await _doacaoRepository.ObterDoacaoPelaCobranca(request.data.charges[0].id);

            if (doacao != null)
            {
                // Obter a campanha
                var campanha = await _campanhaRepository.GetByIdAsync(doacao.Campanha_Id);

                // Caso o request venha PAID
                if (request.data.status == "paid")
                {
                    // PENDING to PAID
                    if (doacao.Status == "pending")
                    {
                        // Atualizo o status da doação para PAID
                        doacao.Status = "paid";
                        await _doacaoService.Atualizar(doacao);

                        // Atualizo o valor da campanha, ADICIONO o valor
                        campanha.TotalArrecadado += ((request.data.amount) / 100);
                        await _campanhaService.Atualizar(campanha);

                        if (request.data.charges[0].payment_method == "pix")
                        {
                            await _signalR.PixIsPaid(true);
                        }

                        return Ok();
                    }

                    // PROCESSING to PAID
                    if (doacao.Status == "processing")
                    {
                        // Atualizo o status da doação para PAID
                        doacao.Status = "paid";
                        await _doacaoService.Atualizar(doacao);

                        // Atualizo o valor da campanha, ADICIONO o valor
                        campanha.TotalArrecadado += ((request.data.amount) / 100);
                        await _campanhaService.Atualizar(campanha);

                        if (request.data.charges[0].payment_method == "pix")
                        {
                            await _signalR.PixIsPaid(true);
                        }

                        return Ok();
                    }
                }

                // Caso o request venha FAILED
                if (request.data.status == "failed")
                {
                    // PENDING to FAILED
                    if (doacao.Status == "pending")
                    {
                        // Atualizo o status da doação para FAILED
                        doacao.Status = "failed";
                        await _doacaoService.Atualizar(doacao);

                        if (request.data.charges[0].payment_method == "pix")
                        {
                            await _signalR.PixIsPaid(false);
                        }

                        return Ok();
                    }

                    // PROCESSING to FAILED
                    if (doacao.Status == "processing")
                    {
                        // Atualizo o status da doação para FAILED
                        doacao.Status = "failed";
                        await _doacaoService.Atualizar(doacao);

                        if (request.data.charges[0].payment_method == "pix")
                        {
                            await _signalR.PixIsPaid(false);
                        }

                        return Ok();
                    }

                    // PAID to FAILED
                    if (doacao.Status == "paid")
                    {
                        // Atualizo o status da doação para PAID
                        doacao.Status = "failed";
                        await _doacaoService.Atualizar(doacao);

                        // Atualizo o valor da campanha, SUBTRAIO o valor
                        campanha.TotalArrecadado -= ((request.data.amount) / 100);
                        await _campanhaService.Atualizar(campanha);

                        return Ok();
                    }
                }

                // Caso o request venha CANCELED
                if (request.data.status == "canceled")
                {
                    // PENDING to CANCELED
                    if (doacao.Status == "pending")
                    {
                        // Atualizo o status da doação para FAILED
                        doacao.Status = "canceled";
                        await _doacaoService.Atualizar(doacao);

                        return Ok();
                    }

                    // PROCESSING to CANCELED
                    if (doacao.Status == "processing")
                    {
                        // Atualizo o status da doação para FAILED
                        doacao.Status = "canceled";
                        await _doacaoService.Atualizar(doacao);

                        return Ok();
                    }

                    // PAID to CANCELED
                    if (doacao.Status == "paid")
                    {
                        // Atualizo o status da doação para PAID
                        doacao.Status = "canceled";
                        await _doacaoService.Atualizar(doacao);

                        // Atualizo o valor da campanha, SUBTRAIO o valor
                        campanha.TotalArrecadado -= ((request.data.amount) / 100);
                        await _campanhaService.Atualizar(campanha);

                        return Ok();
                    }
                }
            }

            return Ok();
        }
        #endregion

        #region PAGARME ---> INTERNAL METHODS
        private string GetUserAndPagarmeId()
        {
            var usuarioId = _user.GetUserId();
            return _identityRepository.GetCodigoPagarme(usuarioId.ToString());
        }

        private async Task<ActionResult> DeleteCharge(string chargeId)
        {
            AddHeaderPagarme();

            HttpResponseMessage response = await client.DeleteAsync(urlPagarme + "charges/" + chargeId);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JObject obj = JObject.Parse(responseBody);

            return Ok();
        }

        private async Task<ActionResult<string>> CallApi(string method, string urlCompleta, string objetoString = "")
        {
            object objJson = null;
            HttpResponseMessage response = null;

            if (objetoString != "" || objetoString != null)
            {
                objJson = JsonConvert.DeserializeObject(objetoString);
            }

            if (method == "post")
            {
                response = await client.PostAsJsonAsync(urlCompleta, objJson);
            }

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            JObject obj = JObject.Parse(responseBody);
            var objToReturn = JsonConvert.SerializeObject(obj);

            return Ok(objToReturn);
        }
        #endregion

    }
}