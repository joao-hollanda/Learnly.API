using Learnly.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DesempenhoController : BaseController
    {
        private readonly IDesempenhoAplicacao _desempenhoAplicacao;

        public DesempenhoController(IDesempenhoAplicacao desempenhoAplicacao)
        {
            _desempenhoAplicacao = desempenhoAplicacao;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> ObterDashboard()
        {
            var usuarioId = GetUserId();
            if (usuarioId == null) return Forbid();

            var dashboard = await _desempenhoAplicacao.ObterDashboard(usuarioId.Value);
            return Success(dashboard);
        }
    }
}
