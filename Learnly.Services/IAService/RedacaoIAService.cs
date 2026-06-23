using Learnly.Application.Interfaces;
using Learnly.Domain.Entities.Redacoes;
using Newtonsoft.Json;

namespace Learnly.Services.IAService
{
    public class RedacaoIAService : IRedacaoIAService
    {
        private readonly MistralHttpClient _mistral;

        private const string ModeloVisao = "mistral-small-latest";
        private const string ModeloCorrecao = "mistral-medium-latest";
        private const string ModeloTema = "mistral-small-latest";

        private static readonly string[] EixosTematicos =
        {
            "educação e acesso ao conhecimento",
            "meio ambiente e crise climática",
            "tecnologia, inteligência artificial e mundo digital",
            "saúde pública e bem-estar",
            "trabalho, economia e desigualdade social",
            "cultura, identidade e diversidade",
            "direitos humanos e cidadania",
            "violência, segurança e justiça",
            "grupos em situação de vulnerabilidade",
            "mobilidade urbana e infraestrutura",
            "consumo, sustentabilidade e meio digital",
            "envelhecimento, juventude e relações entre gerações",
        };

        private const string PromptTema = @"Você elabora temas de redação dissertativo-argumentativa no estilo oficial do ENEM.

Crie UM tema inédito, atual e socialmente relevante para o Brasil, inspirado em debates e tendências recentes da sociedade brasileira.

Regras:
- Aborde um problema ou desafio concreto da realidade brasileira, no padrão dos temas oficiais (ex.: ""Desafios para a valorização de comunidades e povos tradicionais no Brasil"", ""O estigma associado às doenças mentais na sociedade brasileira"", ""Manipulação do comportamento do usuário pelo controle de dados na internet"").
- Formulação clara, entre 6 e 16 palavras, sem dois-pontos e sem instruções como ""redija um texto"".
- Não copie literalmente temas já cobrados; traga um recorte contemporâneo.
- Responda EXCLUSIVAMENTE com a frase do tema, sem aspas, sem numeração e sem qualquer explicação.";

        private const string PromptCorrecao = @"Você é um corretor oficial de redação do ENEM, criterioso e calibrado. Avalie a redação dissertativo-argumentativa do aluno seguindo rigorosamente a matriz de referência das 5 competências.

Atribua a cada competência uma nota entre 0 e 200, sempre em múltiplos de 40 (0, 40, 80, 120, 160 ou 200):
1. Demonstrar domínio da modalidade escrita formal da língua portuguesa.
2. Compreender a proposta e aplicar conceitos de várias áreas do conhecimento para desenvolver o tema, dentro da estrutura dissertativo-argumentativa.
3. Selecionar, relacionar, organizar e interpretar informações, fatos, opiniões e argumentos em defesa de um ponto de vista.
4. Demonstrar conhecimento dos mecanismos linguísticos necessários para a construção da argumentação (coesão).
5. Elaborar proposta de intervenção para o problema, detalhando agente, ação, meio e finalidade, e respeitando os direitos humanos.

Calibração das notas (siga rigorosamente):
- 200: competência impecável, sem deslizes.
- 160: muito boa, com deslizes pontuais.
- 120: mediana, com problemas claros.
- 80: deficiente.
- 40 ou 0: muito aquém ou ausente.
Seja criterioso e diferencie de verdade: NÃO atribua 160 por padrão a toda redação boa e reserve 200 apenas para excelência real. Penalize redações superficiais, sem repertório legitimado ou com proposta de intervenção incompleta.

Regras:
- Fuga total ao tema ou texto que não seja dissertativo-argumentativo zera as competências pertinentes.
- Para cada competência, escreva um comentário objetivo (1 a 3 frases) que justifique a nota e diga como melhorar. Dirija-se ao aluno usando VOCÊ.
- A nota deve ser coerente com o comentário.

Responda EXCLUSIVAMENTE com um objeto JSON neste formato, sem texto fora do JSON:
{
  ""competencias"": [
    { ""numero"": 1, ""nota"": 0, ""comentario"": ""..."" },
    { ""numero"": 2, ""nota"": 0, ""comentario"": ""..."" },
    { ""numero"": 3, ""nota"": 0, ""comentario"": ""..."" },
    { ""numero"": 4, ""nota"": 0, ""comentario"": ""..."" },
    { ""numero"": 5, ""nota"": 0, ""comentario"": ""..."" }
  ],
  ""comentarioGeral"": ""visão geral do desempenho e principais pontos a evoluir""
}";

        public RedacaoIAService(MistralHttpClient mistral)
        {
            _mistral = mistral;
        }

        public async Task<string> TranscreverImagemAsync(byte[] imagem, string mimeType)
        {
            var dataUri = $"data:{mimeType};base64,{Convert.ToBase64String(imagem)}";

            var requestBody = new
            {
                model = ModeloVisao,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = "Transcreva exatamente o texto manuscrito desta redação, preservando a divisão em parágrafos. Não corrija erros, não comente e não adicione título. Responda apenas com o texto transcrito." },
                            new { type = "image_url", image_url = dataUri }
                        }
                    }
                },
                temperature = 0.1
            };

            var texto = await _mistral.EnviarAsync(requestBody)
                ?? throw new Exception("Erro ao transcrever a redação: resposta nula.");

            return texto.Trim();
        }

        public async Task<string> GerarTemaAsync()
        {
            var eixo = EixosTematicos[Random.Shared.Next(EixosTematicos.Length)];

            var requestBody = new
            {
                model = ModeloTema,
                messages = new[]
                {
                    new { role = "system", content = PromptTema },
                    new { role = "user", content = $"Eixo temático sorteado: {eixo}. Gere o tema dentro desse eixo." }
                },
                temperature = 0.9
            };

            var tema = await _mistral.EnviarAsync(requestBody)
                ?? throw new Exception("Erro ao gerar o tema: resposta nula.");

            return tema.Trim().Trim('"');
        }

        public async Task<CorrecaoRedacao> CorrigirAsync(string tema, string texto)
        {
            var requestBody = new
            {
                model = ModeloCorrecao,
                messages = new[]
                {
                    new { role = "system", content = PromptCorrecao },
                    new { role = "user", content = $"Tema da redação:\n{tema}\n\nTexto do aluno:\n{texto}" }
                },
                temperature = 0.2,
                response_format = new { type = "json_object" }
            };

            var raw = await _mistral.EnviarAsync(requestBody)
                ?? throw new Exception("Erro ao corrigir a redação: resposta nula.");

            var json = raw.Trim()
                .TrimStart("```json".ToCharArray())
                .TrimStart("```".ToCharArray())
                .TrimEnd("```".ToCharArray())
                .Trim();

            return JsonConvert.DeserializeObject<CorrecaoRedacao>(json)
                ?? throw new Exception("Não foi possível desserializar a correção da redação.");
        }
    }
}
