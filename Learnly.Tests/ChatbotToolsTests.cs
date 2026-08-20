using Learnly.Services.IAService;
using Xunit;

namespace Learnly.Tests
{
    public class ChatbotToolsTests
    {
        [Fact]
        public void ObterFerramentas_ExpoeAsFerramentasDoMentor()
        {
            var nomes = ChatbotTools.ObterFerramentas().Select(f => f.function.name).ToList();

            Assert.Equal(new[]
            {
                "buscar_desempenho_do_aluno",
                "buscar_pontos_fracos_por_habilidade",
                "revisar_questoes_erradas",
                "buscar_plano_estudo_atual",
                "reajustar_carga_horaria_plano",
                "adicionar_ou_remover_disciplina",
                "reagendar_topicos_atrasados",
                "criar_eventos_estudo"
            }, nomes);
        }

        [Fact]
        public void ObterFerramentas_TodasDeclaramTipoDescricaoEParametros()
        {
            var ferramentas = ChatbotTools.ObterFerramentas();

            Assert.All(ferramentas, ferramenta =>
            {
                Assert.Equal("function", ferramenta.type);
                Assert.False(string.IsNullOrWhiteSpace(ferramenta.function.description));
                Assert.NotNull(ferramenta.function.parameters);
            });
        }
    }
}
