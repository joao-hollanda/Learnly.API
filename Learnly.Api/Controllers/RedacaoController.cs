using Learnly.Api.Models.Redacoes.Request;
using Learnly.Api.Models.Redacoes.Response;
using Learnly.API.Controllers;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities.Redacoes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;

namespace Learnly.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RedacaoController : BaseController
    {
        private readonly IRedacaoAplicacao _redacaoAplicacao;

        public RedacaoController(IRedacaoAplicacao redacaoAplicacao)
        {
            _redacaoAplicacao = redacaoAplicacao;
        }

        [HttpPost("Transcrever")]
        [EnableRateLimiting("ia")]
        public async Task<IActionResult> Transcrever(IFormFile imagem)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Forbid();

            if (imagem == null || imagem.Length == 0)
                return BadRequest("Envie uma imagem da redação.");

            using var ms = new MemoryStream();
            await imagem.CopyToAsync(ms);

            var texto = await _redacaoAplicacao.Transcrever(ms.ToArray(), imagem.ContentType);

            return Success(new { texto });
        }

        [HttpGet("Tema")]
        [EnableRateLimiting("ia")]
        public async Task<IActionResult> GerarTema()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Forbid();

            var tema = await _redacaoAplicacao.GerarTema();

            return Success(new { tema });
        }

        [HttpPost]
        [EnableRateLimiting("ia")]
        public async Task<IActionResult> Corrigir([FromBody] CorrigirRedacaoRequest request)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Forbid();

            var redacao = await _redacaoAplicacao.Corrigir(usuarioId.Value, request.Tema, request.Texto);

            return Success(MapearResultado(redacao));
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Forbid();

            var redacoes = await _redacaoAplicacao.Listar(usuarioId.Value);

            return Success(redacoes.Select(r => new RedacaoResumoDto
            {
                RedacaoId = r.RedacaoId,
                Tema = r.Tema,
                NotaFinal = r.NotaFinal,
                Data = r.Data
            }));
        }

        [HttpGet("{redacaoId:int}")]
        public async Task<IActionResult> Obter([FromRoute] int redacaoId)
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Forbid();

            var redacao = await _redacaoAplicacao.Obter(redacaoId, usuarioId.Value);

            return Success(MapearResultado(redacao));
        }

        private static RedacaoResultadoDto MapearResultado(Redacao redacao)
        {
            var correcao = JsonConvert.DeserializeObject<CorrecaoRedacao>(redacao.ComentariosJson) ?? new CorrecaoRedacao();

            return new RedacaoResultadoDto
            {
                RedacaoId = redacao.RedacaoId,
                Tema = redacao.Tema,
                Texto = redacao.Texto,
                NotaFinal = redacao.NotaFinal,
                Competencias = correcao.Competencias,
                ComentarioGeral = correcao.ComentarioGeral,
                Data = redacao.Data
            };
        }
    }
}
