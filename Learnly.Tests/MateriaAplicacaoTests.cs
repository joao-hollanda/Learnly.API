using Learnly.Application.Applications;
using Learnly.Domain.Entities.Planos;
using Learnly.Repository.Interfaces;
using Moq;
using Xunit;

namespace Learnly.Tests
{
    public class MateriaAplicacaoTests
    {
        private readonly Mock<IMateriaRepositorio> _materiaRepo = new();

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Listar_RepassaFiltroDeGeracaoPorIA(bool geradaPorIa)
        {
            var materias = new List<Materia> { new() { MateriaId = 1, Nome = "Matemática" } };
            _materiaRepo.Setup(r => r.Listar(geradaPorIa)).ReturnsAsync(materias);
            var sut = new MateriaAplicacao(_materiaRepo.Object);

            Assert.Same(materias, await sut.Listar(geradaPorIa));
        }
    }
}
