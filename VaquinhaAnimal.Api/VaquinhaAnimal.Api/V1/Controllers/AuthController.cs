using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VaquinhaAnimal.Api.Controllers;
using VaquinhaAnimal.Api.Extensions;
using VaquinhaAnimal.Api.ViewModels;
using VaquinhaAnimal.Domain.Entities.Base;
using VaquinhaAnimal.Domain.Entities.Pagarme;
using VaquinhaAnimal.Domain.Entities.Validations.Documents;
using VaquinhaAnimal.Domain.Interfaces;

namespace VaquinhaAnimal.Api.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/")]
    public class AuthController : MainController
    {
        #region VARIABLES
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IUser _user;
        private readonly IIdentityRepository _identityRepository;
        #endregion

        #region CONSTRUCTOR
        public AuthController(INotificador notificador,
                              SignInManager<ApplicationUser> signInManager,
                              IIdentityRepository identityRepository,
                              UserManager<ApplicationUser> userManager,
                              IOptions<AppSettings> appSettings, IUser user,
                              ILogger<AuthController> logger,
                              IMapper mapper,
                              IUsuarioService usuarioService) : base(notificador, user)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _logger = logger;
            _mapper = mapper;
            _user = user;
            _usuarioService = usuarioService;
            _identityRepository = identityRepository;
        }

        #endregion

        #region CRUD
        [HttpPost("nova-conta")]
        public async Task<ActionResult> Registrar(RegisterUserViewModel registerUser)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var userSameEmail = await _usuarioService.GetUserEmailAsync(registerUser.Email);
            if (userSameEmail != null)
            {
                NotificarErro("Já existe um usuário com este e-mail");
                return CustomResponse();
            }

            var idPagarme = await AddClientPagarme(registerUser);

            if (String.IsNullOrWhiteSpace(idPagarme))
            {
                NotificarErro("Não foi possível realizar seu cadastro. Verifique se algo foi preenchido incorretamente.");
                return CustomResponse();
            }

            var user = new ApplicationUser
            {
                UserName = registerUser.Email,
                Email = registerUser.Email,
                EmailConfirmed = true,
                Name = registerUser.Name,
                Code = "",
                Foto = registerUser.Foto,
                Codigo_Pagarme = idPagarme
            };

            var result = await _userManager.CreateAsync(user, registerUser.Password);
            
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                SendEmailUserAdded(user);
                return CustomResponse(await GerarJwt(user.Email));
            }
            foreach (var error in result.Errors)
            {
                NotificarErro(error.Description);
            }

            return CustomResponse(registerUser);
        }

        [HttpPost("entrar")]
        public async Task<ActionResult> Entrar(LoginUserViewModel loginUser)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var result = await _signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

            if (result.Succeeded)
            {
                _logger.LogInformation("Usuário " + loginUser.Email + " logado com sucesso.");
                return CustomResponse(await GerarJwt(loginUser.Email));
            }
            if (result.IsLockedOut)
            {
                NotificarErro("Usuário temporariamente bloqueado por excesso de tentativas. Tente em alguns minutos.");
                return CustomResponse(loginUser);
            }

            NotificarErro("Usuário e/ou Senha inválidos.");
            return CustomResponse(loginUser);
        }

        [HttpPut("atualizar-dados")]
        public async Task<ActionResult> UpdateUser(ApplicationUser user)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var userToBeUpdated = await _userManager.FindByIdAsync(_user.GetUserId().ToString());
            
            var userSameEmail = await _usuarioService.GetUserEmailAsync(user.Email);

            if (userSameEmail != null && userToBeUpdated.Id != userSameEmail.Id)
            {
                NotificarErro("Já existe um usuário com este e-mail");
                return CustomResponse();
            }

            userToBeUpdated.Name = user.Name;
            userToBeUpdated.Email = user.Email;
            userToBeUpdated.Code = user.Code;

            var resultPagarme = await EditClientPagarme(user);

            if (resultPagarme)
            {
                await _userManager.UpdateAsync(userToBeUpdated);
            }
            else
            {
                NotificarErro("Houve um erro ao atualizar. Revise os dados e tente novamente.");
                return CustomResponse();
            }


            return CustomResponse();
        }

        [HttpPut("atualizar-senha")]
        public async Task<ActionResult> UpdateUserPassword(UserPasswordViewModel user)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var userToBeUpdated = await _userManager.FindByIdAsync(_user.GetUserId().ToString());

            var result = await _userManager.ChangePasswordAsync(userToBeUpdated, user.CurrentPassword, user.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    NotificarErro(error.Description);
                }
            }            

            return CustomResponse(result);
        }

        [HttpPost("reset-password-token")]
        public async Task<ActionResult> ResetPasswordToken(ResetPasswordUserViewModel user)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var usuario = await _userManager.FindByNameAsync(user.Username);
            if (usuario == null)
            {
                NotificarErro("User not found.");
                return CustomResponse();
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);

            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // ENVIAR TOKEN POR EMAIL
            SendEmailResetPasswordToken(code, user.Username);

            return CustomResponse(token);
        }

        [HttpPost("reset-password-user")]
        public async Task<ActionResult> ResetPasswordUser(ResetPasswordViewModel user)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var usuario = await _userManager.FindByNameAsync(user.Username);
            if (usuario == null)
            {
                NotificarErro("User not found.");
                return CustomResponse();
            }

            if (string.Compare(user.NewPassword, user.ConfirmPassword) != 0)
            {
                NotificarErro("Password and confirmation don't match.");
                return CustomResponse();
            }

            if (string.IsNullOrEmpty(user.Token))
            {
                NotificarErro("Invalid token");
                return CustomResponse();
            }

            var tokenDecoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(user.Token));

            var result = await _userManager.ResetPasswordAsync(usuario, tokenDecoded, user.NewPassword);
            if (!result.Succeeded)
            {
                NotificarErro("Error reseting your password. Contact our team.");
                return CustomResponse();
            }

            return CustomResponse(result);
        }

        [HttpGet("getAllUsers")]
        public async Task<List<UsuarioListViewModel>> ObterUsuariosAsync()
        {
            var dtoList = await _usuarioService.ObterListaUsuariosAsync();
            var viewModelList = _mapper.Map<List<UsuarioListViewModel>>(dtoList);
            return viewModelList;
        }

        [HttpGet("por-id/{id}")]
        public async Task<ActionResult<ApplicationUser>> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) return BadRequest();

            return Ok(user);
        }
        #endregion

        #region METHODS
        private async Task<LoginResponseViewModel> GerarJwt(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
            {
                Issuer = _appSettings.Emissor,
                Audience = _appSettings.ValidoEm,
                Subject = identityClaims,
                Expires = DateTime.UtcNow.AddHours(_appSettings.ExpiracaoHoras),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            });

            var encodedToken = tokenHandler.WriteToken(token);

            var response = new LoginResponseViewModel
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_appSettings.ExpiracaoHoras).TotalSeconds,
                UserToken = new UserTokenViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    Nome = user.Name,
                    Claims = claims.Select(c => new ClaimViewModel { Type = c.Type, Value = c.Value })
                }
            };

            return response;
        }

        private static long ToUnixEpochDate(DateTime date)
            => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

        private async Task<string> AddClientPagarme(RegisterUserViewModel registerUser)
        {
            try
            {
                AddHeaderPagarme();

                var clienteToAdd = new PagarmeCliente()
                {
                    Name = registerUser.Name,
                    Code = "Cliente_" + registerUser.Name.Split(" ", 10, StringSplitOptions.None)[0],
                    Email = registerUser.Email
                };

                HttpResponseMessage response = await client.PostAsJsonAsync(urlPagarme + "customers", clienteToAdd);
                string responseBody = await response.Content.ReadAsStringAsync();

                var clienteRecebido = JsonConvert.DeserializeObject<PagarmeCliente>(responseBody);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return clienteRecebido.Id;
                } 
                else
                {
                    return "";
                }
            }
            catch (HttpRequestException e)
            {
                return "Erro: " + e;
            }
        }

        private async Task<bool> EditClientPagarme(ApplicationUser applicationUser)
        {
            try
            {
                AddHeaderPagarme();

                var customerId = GetUserAndPagarmeId();

                var clienteToEdit = new PagarmeCliente()
                {
                    Name = applicationUser.Name,
                    Code = "Cliente_" + applicationUser.Name.Split(" ", 10, StringSplitOptions.None)[0],
                    Email = applicationUser.Email
                };

                HttpResponseMessage response = await client.PutAsJsonAsync(urlPagarme + "customers/" + customerId, clienteToEdit);
                string responseBody = await response.Content.ReadAsStringAsync();

                var clienteRecebido = JsonConvert.DeserializeObject<PagarmeCliente>(responseBody);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (HttpRequestException e)
            {
                return false;
            }
        }

        private string GetUserAndPagarmeId()
        {
            var usuarioId = _user.GetUserId();
            return _identityRepository.GetCodigoPagarme(usuarioId.ToString());
        }

        private void SendEmailUserAdded(ApplicationUser userAdded)
        {
            var userMail = _user.GetUserEmail();

            try
            {
                MailMessage message = new MailMessage("contato@doadoresespeciais.com.br", "contato@doadoresespeciais.com.br");
                message.Subject = "Usuário criado - Doadores Especiais";
                message.Body = "<div style='font-size: 12px; font-family: Verdana; background-color: #f8f8f8; margin-left: 20px;'>" +
                    "<img src='https://www.doadoresespeciais.com.br/assets/img/logo.png' style='width: 300px;'/><br/><br/>" +
                    "<h2>Usuário criado na plataforma</h2></br></br> <h3><u>Confira os detalhes abaixo: </u></h2><br/><br/> " +
                    "<p><b>Nome: </b>" + userAdded.Name + "</p>" +
                    "<p><b>Tipo de Documento: </b>" + "" + "</p>" +
                    "<p><b>Documento: </b>" + "" + " dias </p>";

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

        private void SendEmailResetPasswordToken(string token, string userMail)
        {
            var url = "https://www.doadoresespeciais.com.br/auth/reset-password-user/" + userMail + "/" + token;

            try
            {
                MailMessage message = new MailMessage("contato@doadoresespeciais.com.br", userMail);
                message.Subject = "Recuperação de Senha - Doadores Especiais";
                message.Body = "<div style='font-size: 12px; font-family: Verdana; background-color: #f8f8f8; margin-left: 20px;'>" +
                    "<img src='https://www.doadoresespeciais.com.br/assets/img/logo.png' style='width: 300px;'/><br/><br/>" +
                    "<h2>RECUPERAÇÃO DE SENHA</h2></br></br></h2><br/><br/> " +
                    "<p><a href="+url+">CLIQUE AQUI</a></p>";

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