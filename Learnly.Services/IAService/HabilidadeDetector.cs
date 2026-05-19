using Learnly.Domain.Entities.Simulados;

namespace Learnly.Services.IAService
{
    public static class HabilidadeDetector
    {
        public static string Detectar(Questao q)
        {
            return q.Disciplina switch
            {
                "linguagens" => DetectarLinguagens(q),
                "matematica" => DetectarMatematica(q),
                "ciencias-natureza" => DetectarNatureza(q),
                "ciencias-humanas" => DetectarHumanas(q),
                _ => "geral"
            };
        }

        private static string TextoQuestao(Questao q) =>
            $"{q.Titulo} {q.IntroducaoAlternativa} {q.Contexto}".ToLowerInvariant();

        private static string DetectarLinguagens(Questao q)
        {
            var t = TextoQuestao(q);

            if (t.Contains("pronome pessoal") || t.Contains("pronome relativo") || t.Contains("pronome demonstrativo"))
                return "pronomes";
            if (t.Contains("figura de linguagem") || t.Contains("metáfora") || t.Contains("metonímia") || t.Contains("ironia"))
                return "figuras de linguagem";
            if (t.Contains("interpretação") || t.Contains("sentido do texto") || t.Contains("inferência"))
                return "interpretação";
            if (t.Contains("poema") || t.Contains("crônica") || t.Contains("narrativa") || t.Contains("gênero textual"))
                return "compreensão textual";
            if (t.Contains("gramática") || t.Contains("sintaxe") || t.Contains("concordância") || t.Contains("regência"))
                return "gramática";

            return "geral";
        }

        private static string DetectarMatematica(Questao q)
        {
            var t = TextoQuestao(q);

            if (t.Contains("porcent")) return "porcentagem";
            if (t.Contains("razão") || t.Contains("proporção")) return "razão e proporção";
            if (t.Contains("função quadrática") || t.Contains("função exponencial") || t.Contains("função"))
                return "funções";
            if (t.Contains("gráfico")) return "leitura de gráficos";
            if (t.Contains("equação")) return "equações";
            if (t.Contains("probabil")) return "probabilidade";
            if (t.Contains("geometr") || t.Contains("triângulo") || t.Contains("círculo"))
                return "geometria";
            if (t.Contains("média") || t.Contains("mediana") || t.Contains("estat"))
                return "estatística";

            return "geral";
        }

        private static string DetectarNatureza(Questao q)
        {
            var t = TextoQuestao(q);

            if (t.Contains("reação química") || t.Contains("equação química") || t.Contains("reagente"))
                return "reações químicas";
            if (t.Contains("energia cinética") || t.Contains("energia potencial") || t.Contains("trabalho"))
                return "energia e trabalho";
            if (t.Contains("ecossistema") || t.Contains("cadeia alimentar") || t.Contains("teia alimentar"))
                return "ecologia";
            if (t.Contains("célula") || t.Contains("dna") || t.Contains("mitose") || t.Contains("meiose"))
                return "biologia celular";
            if (t.Contains("força") || t.Contains("aceleração") || t.Contains("velocidade") || t.Contains("movimento"))
                return "cinemática e dinâmica";

            return "geral";
        }

        private static string DetectarHumanas(Questao q)
        {
            var t = TextoQuestao(q);

            if (t.Contains("revolução industrial") || t.Contains("revolução francesa") || t.Contains("guerra"))
                return "processos históricos";
            if (t.Contains("território") || t.Contains("espaço geográfico") || t.Contains("bioma") || t.Contains("clima"))
                return "geografia";
            if (t.Contains("cidadania") || t.Contains("constituição") || t.Contains("estado democrático"))
                return "política e sociedade";
            if (t.Contains("cultura") || t.Contains("identidade cultural") || t.Contains("etnia"))
                return "cultura e sociedade";

            return "geral";
        }
    }
}