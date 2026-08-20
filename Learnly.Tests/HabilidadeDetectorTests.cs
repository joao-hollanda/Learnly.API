using Learnly.Services.IAService;
using Xunit;

namespace Learnly.Tests
{
    public class HabilidadeDetectorTests
    {
        private static string Detectar(string disciplina, string titulo) =>
            HabilidadeDetector.Detectar(new Questao { Disciplina = disciplina, Titulo = titulo });

        [Theory]
        [InlineData("Analise o uso do pronome relativo no trecho", "pronomes")]
        [InlineData("Identifique a metáfora presente no trecho", "figuras de linguagem")]
        [InlineData("Qual é a ideia principal do trecho", "interpretação de texto")]
        [InlineData("O poema apresenta rimas ricas", "literatura e poesia")]
        [InlineData("O narrador do conto observa a cena", "gêneros narrativos")]
        [InlineData("Trata-se de um texto dissertativo", "gêneros e tipologias textuais")]
        [InlineData("Analise a concordância verbal", "gramática e sintaxe")]
        [InlineData("Regras de acentuação em palavras proparoxítonas", "ortografia e acentuação")]
        [InlineData("Estudo da variação linguística regional", "variação linguística")]
        [InlineData("O anúncio usa um slogan marcante", "linguagem publicitária e multimodal")]
        [InlineData("Read the following text in english", "língua estrangeira")]
        [InlineData("Leia o trecho abaixo", "linguagens — geral")]
        public void Linguagens_ClassificaPorPalavraChave(string titulo, string esperado)
        {
            Assert.Equal(esperado, Detectar("linguagens", titulo));
        }

        [Theory]
        [InlineData("Calcule o desconto aplicado ao valor", "porcentagem e juros")]
        [InlineData("Use a regra de três simples", "razão, proporção e regra de três")]
        [InlineData("A função quadrática tem vértice na origem", "função quadrática")]
        [InlineData("Crescimento exponencial da população", "função exponencial")]
        [InlineData("Calcule o logaritmo de 100 na base 10", "logaritmos")]
        [InlineData("A função afim possui coeficiente angular 2", "função afim")]
        [InlineData("Determine o domínio da função dada", "funções — geral")]
        [InlineData("Calcule o seno do ângulo indicado", "trigonometria")]
        [InlineData("A progressão aritmética tem primeiro termo 5", "progressão aritmética")]
        [InlineData("A progressão geométrica dobra a cada termo", "progressão geométrica")]
        [InlineData("Resolva pela fórmula de bhaskara", "equação do 2º grau")]
        [InlineData("Resolva a inequação do primeiro grau", "equações e sistemas")]
        [InlineData("Qual a probabilidade de sair coroa", "probabilidade")]
        [InlineData("Calcule o fatorial de cinco", "análise combinatória")]
        [InlineData("Calcule a área do triângulo retângulo", "geometria")]
        [InlineData("Aplique o teorema de pitágoras", "semelhança e teoremas")]
        [InlineData("Calcule a média dos valores obtidos", "estatística")]
        [InlineData("Observe o gráfico de barras", "leitura de gráficos e tabelas")]
        [InlineData("A intersecção dos conjuntos A e B", "conjuntos")]
        [InlineData("Calcule o mmc entre 12 e 18", "números e operações")]
        [InlineData("Resolva o item a seguir", "matemática — geral")]
        public void Matematica_ClassificaPorPalavraChave(string titulo, string esperado)
        {
            Assert.Equal(esperado, Detectar("matematica", titulo));
        }

        [Theory]
        [InlineData("Observe a reação química descrita", "reações químicas")]
        [InlineData("Calcule o ph após a neutralização", "ácidos, bases e sais")]
        [InlineData("A ligação covalente entre os átomos", "ligações químicas")]
        [InlineData("Determine a massa molar do composto", "estequiometria")]
        [InlineData("O número atômico do elemento indicado", "estrutura atômica e tabela periódica")]
        [InlineData("Cadeia de hidrocarboneto ramificada", "química orgânica")]
        [InlineData("Calcule a concentração do soluto", "soluções")]
        [InlineData("A transformação exotérmica libera entalpia", "termoquímica")]
        [InlineData("A pilha sofre oxidação no eletrodo", "eletroquímica")]
        [InlineData("Calcule a velocidade do móvel", "cinemática")]
        [InlineData("A força resultante segundo newton", "dinâmica e leis de Newton")]
        [InlineData("A energia cinética do corpo em movimento", "energia, trabalho e potência")]
        [InlineData("O empuxo sobre o fluido deslocado", "hidrostática e fluidos")]
        [InlineData("A dilatação do metal aquecido", "termologia e termodinâmica")]
        [InlineData("O comprimento de onda emitido", "ondas, óptica e acústica")]
        [InlineData("A corrente no circuito em série", "eletricidade")]
        [InlineData("O campo magnético gerado pela bobina", "eletromagnetismo")]
        [InlineData("A meia-vida do isótopo instável", "radioatividade e física nuclear")]
        [InlineData("A membrana da célula animal", "citologia")]
        [InlineData("A molécula de dna e seus genes", "genética molecular")]
        [InlineData("Etapas da meiose em organismos diploides", "divisão celular")]
        [InlineData("Alelo dominante e recessivo", "genética mendeliana")]
        [InlineData("A cadeia alimentar do ecossistema", "ecologia")]
        [InlineData("A fotossíntese produz glicose", "metabolismo celular")]
        [InlineData("O neurônio transmite o impulso pela sinapse", "fisiologia humana — nervoso e endócrino")]
        [InlineData("O sistema circulatório e o coração", "fisiologia humana — órgãos e sistemas")]
        [InlineData("A seleção natural segundo darwin", "evolução")]
        [InlineData("A bactéria causadora da infecção", "microbiologia e doenças")]
        [InlineData("Analise a situação descrita", "ciências da natureza — geral")]
        public void CienciasNatureza_ClassificaPorPalavraChave(string titulo, string esperado)
        {
            Assert.Equal(esperado, Detectar("ciencias-natureza", titulo));
        }

        [Theory]
        [InlineData("A revolução industrial e a burguesia", "história — industrialização e ideologias")]
        [InlineData("O iluminismo e a revolução francesa", "história — revoluções modernas")]
        [InlineData("A guerra fria e o nazismo", "história — guerras e totalitarismos")]
        [InlineData("A abolição da escravidão", "história do brasil — colônia e escravidão")]
        [InlineData("A ditadura militar e a redemocratização", "história do brasil — república")]
        [InlineData("A resistência indígena no território", "história — povos indígenas")]
        [InlineData("O imperialismo europeu do século XIX", "história — imperialismo e descolonização")]
        [InlineData("O feudalismo na idade média", "história — idade média e renascimento")]
        [InlineData("O bioma do cerrado brasileiro", "geografia — biomas brasileiros")]
        [InlineData("O clima semiárido registra pouca chuva", "geografia — clima e meteorologia")]
        [InlineData("A urbanização acelerada nas metrópoles", "geografia urbana")]
        [InlineData("A globalização e os blocos econômicos", "geopolítica e globalização")]
        [InlineData("A natalidade e a mortalidade infantil", "geografia — população e demografia")]
        [InlineData("A bacia hidrográfica e o relevo local", "geografia física")]
        [InlineData("O desmatamento e o aquecimento global", "meio ambiente e sustentabilidade")]
        [InlineData("O agronegócio e a monocultura", "geografia agrária")]
        [InlineData("Os direitos humanos e a cidadania", "política e cidadania")]
        [InlineData("O etnocentrismo e o relativismo cultural", "cultura e identidade")]
        [InlineData("O contrato social de rousseau", "filosofia política")]
        [InlineData("Durkheim e o fato social", "sociologia clássica")]
        [InlineData("A desigualdade e a mobilidade social", "desigualdade e desenvolvimento social")]
        [InlineData("A intolerância religiosa e o estado laico", "religião e sociedade")]
        [InlineData("Leia o texto do autor a seguir", "ciências humanas — geral")]
        public void CienciasHumanas_ClassificaPorPalavraChave(string titulo, string esperado)
        {
            Assert.Equal(esperado, Detectar("ciencias-humanas", titulo));
        }

        [Theory]
        [InlineData("redacao")]
        [InlineData("")]
        [InlineData(null)]
        public void DisciplinaDesconhecida_RetornaGeral(string disciplina)
        {
            Assert.Equal("geral", Detectar(disciplina, "Qualquer enunciado"));
        }

        [Fact]
        public void Detectar_ConsideraContextoEIntroducaoAlemDoTitulo()
        {
            var questao = new Questao
            {
                Disciplina = "matematica",
                Titulo = "Questão 12",
                IntroducaoAlternativa = "Com base no texto,",
                Contexto = "O gráfico apresenta a evolução das vendas"
            };

            Assert.Equal("leitura de gráficos e tabelas", HabilidadeDetector.Detectar(questao));
        }

        [Fact]
        public void Detectar_IgnoraCaixaDoTexto()
        {
            Assert.Equal("probabilidade", Detectar("matematica", "QUAL A PROBABILIDADE DO EVENTO"));
        }
    }
}
