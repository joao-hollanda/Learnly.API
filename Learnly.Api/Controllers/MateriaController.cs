using Learnly.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MateriaController : ControllerBase
    {
        private readonly IMateriaAplicacao _materiaAplicacao;

        public MateriaController(IMateriaAplicacao materiaAplicacao)
        {
            _materiaAplicacao = materiaAplicacao;
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var materias = await _materiaAplicacao.Listar();
            return Ok(materias);
        }
    }
}
