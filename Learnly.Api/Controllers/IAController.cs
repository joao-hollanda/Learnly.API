using Learnly.Services.Interfaces;
using Learnly.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using consumindoIA.Domain;
using Learnly.Application.Interfaces;
using Learnly.Api.Models.Planos.Request;

namespace Learnly.Api.Controllers
{
    [ApiController]
    [Route("api/ia")]
    [Authorize]
    public class IAController : BaseController
    {
        private readonly IIAAplicacao _iaAplicacao;

        public IAController(IIAAplicacao iaAplicacao)
        {
            _iaAplicacao = iaAplicacao;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chatbot([FromBody] MentorIARequest request)
        {
            if (request?.Mensagens == null || !request.Mensagens.Any())
                return BadRequest("Mensagens inválidas.");

            var userId = GetUserId();
            if (!userId.HasValue) return Unauthorized();

            var mensagens = new List<Message>(request.Mensagens);
            var resultado = await _iaAplicacao.ChatbotAsync(mensagens, userId.Value);

            return Success(resultado);
        }
        [HttpPost("criar-plano")]
        public async Task<IActionResult> CriarComIA([FromBody] CriarPlanoIARequest request)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Unauthorized();

            var dto = new Application.DTOs.CriarPlanoIADTO
            {
                Titulo = request.Titulo,
                Objetivo = request.Objetivo,
                HorasPorSemana = request.HorasPorSemana,
                UsuarioId = usuarioId.Value,
                PlanoIa = true
            };

            var plano = await _iaAplicacao.GerarPlanoAsync(usuarioId.Value, dto);
            return Success(plano);
        }

    }
}
