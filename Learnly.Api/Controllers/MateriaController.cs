using Learnly.Application.Interfaces;
using Learnly.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MateriaController : BaseController
    {
        private readonly IMateriaAplicacao _materiaAplicacao;

        public MateriaController(IMateriaAplicacao materiaAplicacao)
        {
            _materiaAplicacao = materiaAplicacao;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var materias = await _materiaAplicacao.Listar(false);
            return Success(materias);
        }
    }
}
