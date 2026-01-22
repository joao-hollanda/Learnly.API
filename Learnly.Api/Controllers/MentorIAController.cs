using Learnly.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using consumindoIA.Domain;

namespace Learnly.Api.Controllers
{
    [ApiController]
    [Route("api/ia")]
    public class IAController : ControllerBase
    {
        private readonly IIAService _iaService;

        public IAController(IIAService iaService)
        {
            _iaService = iaService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chatbot([FromBody] MentorIARequest request)
        {
            if (request?.Mensagens == null || !request.Mensagens.Any())
                return BadRequest("Mensagens inválidas.");

            var mensagens = new List<Message>
            {
                new Message
                {
                    role = "system",
                    content = @"Você é o MentorIA, uma IA educacional.
Explique de forma clara, objetiva e didática.
Use exemplos simples quando necessário.
Faça uma formatação de texto que deixe a leitura clara e fluida."
                }
            };

            mensagens.AddRange(request.Mensagens);

            var resposta = await _iaService.Chatbot(mensagens);

            return Ok(new
            {
                resposta
            });
        }
    }
}
