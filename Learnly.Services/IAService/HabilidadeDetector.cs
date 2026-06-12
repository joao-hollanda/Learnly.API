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

            if (t.Contains("pronome pessoal") || t.Contains("pronome relativo") ||
                t.Contains("pronome demonstrativo") || t.Contains("pronome possessivo") ||
                t.Contains("pronome indefinido") || t.Contains("pronome interrogativo"))
                return "pronomes";

            if (t.Contains("figura de linguagem") || t.Contains("metáfora") ||
                t.Contains("metonímia") || t.Contains("ironia") || t.Contains("hipérbole") ||
                t.Contains("eufemismo") || t.Contains("antítese") || t.Contains("paradoxo") ||
                t.Contains("personificação") || t.Contains("prosopopeia") || t.Contains("aliteração") ||
                t.Contains("sinestesia") || t.Contains("comparação"))
                return "figuras de linguagem";

            if (t.Contains("interpretação") || t.Contains("sentido do texto") ||
                t.Contains("inferência") || t.Contains("implícito") || t.Contains("explícito") ||
                t.Contains("tema central") || t.Contains("ideia principal") || t.Contains("compreensão"))
                return "interpretação de texto";

            if (t.Contains("poema") || t.Contains("poesia") || t.Contains("verso") ||
                t.Contains("estrofe") || t.Contains("rima") || t.Contains("ritmo") ||
                t.Contains("lírico") || t.Contains("épico"))
                return "literatura e poesia";

            if (t.Contains("crônica") || t.Contains("conto") || t.Contains("romance") ||
                t.Contains("narrativa") || t.Contains("narrador") || t.Contains("personagem") ||
                t.Contains("enredo") || t.Contains("foco narrativo"))
                return "gêneros narrativos";

            if (t.Contains("gênero textual") || t.Contains("gênero discursivo") ||
                t.Contains("tipologia") || t.Contains("dissertativo") || t.Contains("argumentativo") ||
                t.Contains("descritivo") || t.Contains("injuntivo") || t.Contains("expositivo"))
                return "gêneros e tipologias textuais";

            if (t.Contains("concordância") || t.Contains("regência") || t.Contains("crase") ||
                t.Contains("pontuação") || t.Contains("vírgula") || t.Contains("sintaxe") ||
                t.Contains("sujeito") || t.Contains("predicado") || t.Contains("período") ||
                t.Contains("oração") || t.Contains("subordinada") || t.Contains("coordenada"))
                return "gramática e sintaxe";

            if (t.Contains("ortografia") || t.Contains("acentuação") || t.Contains("hífen") ||
                t.Contains("grafia") || t.Contains("letra maiúscula"))
                return "ortografia e acentuação";

            if (t.Contains("variação linguística") || t.Contains("dialeto") || t.Contains("registro") ||
                t.Contains("linguagem formal") || t.Contains("linguagem informal") || t.Contains("norma culta"))
                return "variação linguística";

            if (t.Contains("publicidade") || t.Contains("propaganda") || t.Contains("anúncio") ||
                t.Contains("slogan") || t.Contains("persuasão") || t.Contains("multimodal"))
                return "linguagem publicitária e multimodal";

            if (t.Contains("inglês") || t.Contains("english") || t.Contains("spanish") ||
                t.Contains("espanhol") || t.Contains("língua estrangeira"))
                return "língua estrangeira";

            return "linguagens — geral";
        }

        private static string DetectarMatematica(Questao q)
        {
            var t = TextoQuestao(q);

            if (t.Contains("porcent") || t.Contains("desconto") || t.Contains("acréscimo") ||
                t.Contains("juros") || t.Contains("taxa"))
                return "porcentagem e juros";

            if (t.Contains("razão") || t.Contains("proporção") || t.Contains("regra de três") ||
                t.Contains("grandeza") || t.Contains("diretamente proporcional") ||
                t.Contains("inversamente proporcional"))
                return "razão, proporção e regra de três";

            if (t.Contains("função quadrática") || t.Contains("parábola") || t.Contains("vértice") ||
                t.Contains("raiz da função"))
                return "função quadrática";

            if (t.Contains("função exponencial") || t.Contains("crescimento exponencial") ||
                t.Contains("decaimento"))
                return "função exponencial";

            if (t.Contains("logaritmo") || t.Contains("log ") || t.Contains("log("))
                return "logaritmos";

            if (t.Contains("função afim") || t.Contains("função linear") ||
                t.Contains("coeficiente angular") || t.Contains("coeficiente linear"))
                return "função afim";

            if (t.Contains("função") && !t.Contains("quadrática") && !t.Contains("exponencial") &&
                !t.Contains("logaritmo") && !t.Contains("afim"))
                return "funções — geral";

            if (t.Contains("seno") || t.Contains("cosseno") || t.Contains("tangente") ||
                t.Contains("trigonometria") || t.Contains("sen(") || t.Contains("cos("))
                return "trigonometria";

            if (t.Contains("progressão aritmética") || t.Contains("pa ") || t.Contains("p.a."))
                return "progressão aritmética";

            if (t.Contains("progressão geométrica") || t.Contains("pg ") || t.Contains("p.g."))
                return "progressão geométrica";

            if (t.Contains("equação do 2") || t.Contains("equação quadrática") ||
                t.Contains("bhaskara") || t.Contains("delta"))
                return "equação do 2º grau";

            if (t.Contains("equação") || t.Contains("inequação") || t.Contains("sistema de equação"))
                return "equações e sistemas";

            if (t.Contains("probabilidade") || t.Contains("evento") || t.Contains("espaço amostral") ||
                t.Contains("chance"))
                return "probabilidade";

            if (t.Contains("combinação") || t.Contains("permutação") || t.Contains("arranjo") ||
                t.Contains("fatorial") || t.Contains("análise combinatória"))
                return "análise combinatória";

            if (t.Contains("área") || t.Contains("perímetro") || t.Contains("volume") ||
                t.Contains("triângulo") || t.Contains("círculo") || t.Contains("circunferência") ||
                t.Contains("retângulo") || t.Contains("trapézio") || t.Contains("polígono") ||
                t.Contains("sólido") || t.Contains("cone") || t.Contains("cilindro") ||
                t.Contains("esfera") || t.Contains("pirâmide"))
                return "geometria";

            if (t.Contains("semelhança") || t.Contains("congruência") || t.Contains("tales") ||
                t.Contains("pitágoras"))
                return "semelhança e teoremas";

            if (t.Contains("média") || t.Contains("mediana") || t.Contains("moda") ||
                t.Contains("desvio") || t.Contains("estatística") || t.Contains("frequência"))
                return "estatística";

            if (t.Contains("gráfico") || t.Contains("tabela") || t.Contains("pictograma") ||
                t.Contains("histograma"))
                return "leitura de gráficos e tabelas";

            if (t.Contains("conjunto") || t.Contains("união") || t.Contains("intersecção") ||
                t.Contains("subconjunto"))
                return "conjuntos";

            if (t.Contains("número real") || t.Contains("número inteiro") || t.Contains("número racional") ||
                t.Contains("mmc") || t.Contains("mdc") || t.Contains("divisibilidade"))
                return "números e operações";

            return "matemática — geral";
        }

        private static string DetectarNatureza(Questao q)
        {
            var t = TextoQuestao(q);

            // Química
            if (t.Contains("reação química") || t.Contains("equação química") ||
                t.Contains("reagente") || t.Contains("produto") || t.Contains("balanceamento"))
                return "reações químicas";

            if (t.Contains("ácido") || t.Contains("base") || t.Contains("ph") ||
                t.Contains("neutralização") || t.Contains("sal "))
                return "ácidos, bases e sais";

            if (t.Contains("ligação covalente") || t.Contains("ligação iônica") ||
                t.Contains("ligação metálica") || t.Contains("ligação química"))
                return "ligações químicas";

            if (t.Contains("mol ") || t.Contains("mol)") || t.Contains("massa molar") ||
                t.Contains("estequiometria") || t.Contains("quantidade de matéria"))
                return "estequiometria";

            if (t.Contains("tabela periódica") || t.Contains("elemento químico") ||
                t.Contains("número atômico") || t.Contains("número de massa") ||
                t.Contains("próton") || t.Contains("nêutron") || t.Contains("elétron") ||
                t.Contains("camada eletrônica") || t.Contains("valência"))
                return "estrutura atômica e tabela periódica";

            if (t.Contains("orgânico") || t.Contains("carbono") || t.Contains("hidrocarboneto") ||
                t.Contains("álcool") || t.Contains("aldeído") || t.Contains("cetona") ||
                t.Contains("ácido carboxílico") || t.Contains("éster") || t.Contains("amina"))
                return "química orgânica";

            if (t.Contains("solução") || t.Contains("soluto") || t.Contains("solvente") ||
                t.Contains("concentração") || t.Contains("diluição") || t.Contains("molaridade"))
                return "soluções";

            if (t.Contains("termoquímica") || t.Contains("entalpia") || t.Contains("exotérmica") ||
                t.Contains("endotérmica") || t.Contains("calor"))
                return "termoquímica";

            if (t.Contains("eletroquímica") || t.Contains("eletrólise") || t.Contains("pilha") ||
                t.Contains("oxidação") || t.Contains("redução") || t.Contains("oxirredução"))
                return "eletroquímica";

            // Física
            if (t.Contains("velocidade") || t.Contains("aceleração") || t.Contains("deslocamento") ||
                t.Contains("movimento uniforme") || t.Contains("mru") || t.Contains("mrua") ||
                t.Contains("queda livre") || t.Contains("lançamento"))
                return "cinemática";

            if (t.Contains("força") || t.Contains("newton") || t.Contains("atrito") ||
                t.Contains("peso") || t.Contains("normal") || t.Contains("resultante") ||
                t.Contains("inércia") || t.Contains("dinâmica"))
                return "dinâmica e leis de Newton";

            if (t.Contains("energia cinética") || t.Contains("energia potencial") ||
                t.Contains("trabalho") || t.Contains("potência") || t.Contains("conservação de energia"))
                return "energia, trabalho e potência";

            if (t.Contains("pressão") || t.Contains("empuxo") || t.Contains("hidrostática") ||
                t.Contains("pascal") || t.Contains("arquimedes") || t.Contains("fluido"))
                return "hidrostática e fluidos";

            if (t.Contains("temperatura") || t.Contains("termômetro") || t.Contains("dilatação") ||
                t.Contains("gás") || t.Contains("lei dos gases") || t.Contains("termodinâmica") ||
                t.Contains("calor específico") || t.Contains("capacidade térmica"))
                return "termologia e termodinâmica";

            if (t.Contains("onda") || t.Contains("frequência") || t.Contains("comprimento de onda") ||
                t.Contains("som") || t.Contains("luz") || t.Contains("reflexão") ||
                t.Contains("refração") || t.Contains("espelho") || t.Contains("lente"))
                return "ondas, óptica e acústica";

            if (t.Contains("elétrica") || t.Contains("corrente") || t.Contains("tensão") ||
                t.Contains("resistência") || t.Contains("circuito") || t.Contains("ohm") ||
                t.Contains("potência elétrica") || t.Contains("transformador"))
                return "eletricidade";

            if (t.Contains("campo magnético") || t.Contains("campo elétrico") ||
                t.Contains("indução") || t.Contains("magnetismo") || t.Contains("força magnética"))
                return "eletromagnetismo";

            if (t.Contains("radioatividade") || t.Contains("fissão") || t.Contains("fusão nuclear") ||
                t.Contains("decaimento") || t.Contains("meia-vida") || t.Contains("radiação"))
                return "radioatividade e física nuclear";

            // Biologia
            if (t.Contains("célula") || t.Contains("membrana") || t.Contains("mitocôndria") ||
                t.Contains("ribossomo") || t.Contains("núcleo celular") || t.Contains("organela"))
                return "citologia";

            if (t.Contains("dna") || t.Contains("rna") || t.Contains("gene") ||
                t.Contains("cromossomo") || t.Contains("mutação") || t.Contains("código genético") ||
                t.Contains("transcrição") || t.Contains("tradução"))
                return "genética molecular";

            if (t.Contains("mitose") || t.Contains("meiose") || t.Contains("divisão celular") ||
                t.Contains("ciclo celular"))
                return "divisão celular";

            if (t.Contains("mendeliana") || t.Contains("dominante") || t.Contains("recessivo") ||
                t.Contains("fenótipo") || t.Contains("genótipo") || t.Contains("hereditar") ||
                t.Contains("1ª lei de mendel") || t.Contains("2ª lei de mendel"))
                return "genética mendeliana";

            if (t.Contains("ecossistema") || t.Contains("cadeia alimentar") ||
                t.Contains("teia alimentar") || t.Contains("nicho") || t.Contains("habitat") ||
                t.Contains("bioma") || t.Contains("impacto ambiental") || t.Contains("biodiversidade"))
                return "ecologia";

            if (t.Contains("fotossíntese") || t.Contains("respiração celular") ||
                t.Contains("fermentação") || t.Contains("glicose") || t.Contains("atp") ||
                t.Contains("cloroplasto"))
                return "metabolismo celular";

            if (t.Contains("sistema nervoso") || t.Contains("neurônio") || t.Contains("sinapse") ||
                t.Contains("sistema endócrino") || t.Contains("hormônio") || t.Contains("glândula"))
                return "fisiologia humana — nervoso e endócrino";

            if (t.Contains("sistema digestório") || t.Contains("digestão") ||
                t.Contains("sistema circulatório") || t.Contains("coração") ||
                t.Contains("sistema respiratório") || t.Contains("pulmão") ||
                t.Contains("sistema excretor") || t.Contains("rim"))
                return "fisiologia humana — órgãos e sistemas";

            if (t.Contains("evolução") || t.Contains("darwin") || t.Contains("seleção natural") ||
                t.Contains("adaptação") || t.Contains("especiação") || t.Contains("lamarck"))
                return "evolução";

            if (t.Contains("vírus") || t.Contains("bactéria") || t.Contains("fungo") ||
                t.Contains("protozoário") || t.Contains("parasita") || t.Contains("doença"))
                return "microbiologia e doenças";

            return "ciências da natureza — geral";
        }

        private static string DetectarHumanas(Questao q)
        {
            var t = TextoQuestao(q);

            // História
            if (t.Contains("revolução industrial") || t.Contains("capitalismo") ||
                t.Contains("burguesia") || t.Contains("proletariado") || t.Contains("socialismo") ||
                t.Contains("comunismo") || t.Contains("marxismo"))
                return "história — industrialização e ideologias";

            if (t.Contains("revolução francesa") || t.Contains("iluminismo") ||
                t.Contains("absolutismo") || t.Contains("ancien régime") || t.Contains("declaração dos direitos"))
                return "história — revoluções modernas";

            if (t.Contains("primeira guerra") || t.Contains("segunda guerra") ||
                t.Contains("guerra fria") || t.Contains("nazismo") || t.Contains("fascismo") ||
                t.Contains("holocausto") || t.Contains("totalitarismo"))
                return "história — guerras e totalitarismos";

            if (t.Contains("colonização") || t.Contains("colônia") || t.Contains("escravidão") ||
                t.Contains("abolição") || t.Contains("lei áurea") || t.Contains("senzala") ||
                t.Contains("quilombo"))
                return "história do brasil — colônia e escravidão";

            if (t.Contains("república") || t.Contains("vargas") || t.Contains("ditadura") ||
                t.Contains("militar") || t.Contains("redemocratização") || t.Contains("constituição de 1988") ||
                t.Contains("impeachment"))
                return "história do brasil — república";

            if (t.Contains("indígena") || t.Contains("povos originários") || t.Contains("aldeamento") ||
                t.Contains("resistência indígena"))
                return "história — povos indígenas";

            if (t.Contains("descolonização") || t.Contains("africa") || t.Contains("ásia") ||
                t.Contains("imperialismo") || t.Contains("neocolonialismo"))
                return "história — imperialismo e descolonização";

            if (t.Contains("renascimento") || t.Contains("idade média") || t.Contains("feudalismo") ||
                t.Contains("cruzadas") || t.Contains("reforma protestante") || t.Contains("contrarreforma"))
                return "história — idade média e renascimento";

            // Geografia
            if (t.Contains("bioma") || t.Contains("cerrado") || t.Contains("amazônia") ||
                t.Contains("caatinga") || t.Contains("pantanal") || t.Contains("mata atlântica") ||
                t.Contains("pampas"))
                return "geografia — biomas brasileiros";

            if (t.Contains("clima") || t.Contains("temperatura") || t.Contains("precipitação") ||
                t.Contains("umidade") || t.Contains("tropical") || t.Contains("semiárido") ||
                t.Contains("equatorial"))
                return "geografia — clima e meteorologia";

            if (t.Contains("urbanização") || t.Contains("cidade") || t.Contains("metrópole") ||
                t.Contains("periferias") || t.Contains("segregação") || t.Contains("favelização"))
                return "geografia urbana";

            if (t.Contains("globalização") || t.Contains("comércio internacional") ||
                t.Contains("fluxo") || t.Contains("geopolítica") || t.Contains("blocos econômicos") ||
                t.Contains("mercosul") || t.Contains("união europeia"))
                return "geopolítica e globalização";

            if (t.Contains("população") || t.Contains("natalidade") || t.Contains("mortalidade") ||
                t.Contains("migração") || t.Contains("êxodo rural") || t.Contains("crescimento populacional") ||
                t.Contains("densidade demográfica"))
                return "geografia — população e demografia";

            if (t.Contains("relevo") || t.Contains("solo") || t.Contains("erosão") ||
                t.Contains("sedimentação") || t.Contains("hidrografia") || t.Contains("bacia hidrográfica") ||
                t.Contains("planalto") || t.Contains("planície"))
                return "geografia física";

            if (t.Contains("desmatamento") || t.Contains("aquecimento global") ||
                t.Contains("efeito estufa") || t.Contains("sustentabilidade") ||
                t.Contains("desenvolvimento sustentável") || t.Contains("energia renovável"))
                return "meio ambiente e sustentabilidade";

            if (t.Contains("agropecuária") || t.Contains("agricultura") || t.Contains("monocultura") ||
                t.Contains("latifúndio") || t.Contains("reforma agrária") || t.Contains("agronegócio"))
                return "geografia agrária";

            // Filosofia e Sociologia
            if (t.Contains("cidadania") || t.Contains("constituição") || t.Contains("estado democrático") ||
                t.Contains("direitos fundamentais") || t.Contains("direitos humanos") ||
                t.Contains("estado de direito"))
                return "política e cidadania";

            if (t.Contains("cultura") || t.Contains("identidade cultural") || t.Contains("etnia") ||
                t.Contains("multiculturalismo") || t.Contains("relativismo cultural") ||
                t.Contains("etnocentrismo"))
                return "cultura e identidade";

            if (t.Contains("iluminismo") || t.Contains("contrato social") || t.Contains("locke") ||
                t.Contains("rousseau") || t.Contains("montesquieu") || t.Contains("voltaire") ||
                t.Contains("hobbes"))
                return "filosofia política";

            if (t.Contains("sociologia") || t.Contains("durkheim") || t.Contains("weber") ||
                t.Contains("marx") || t.Contains("fato social") || t.Contains("estrutura social") ||
                t.Contains("classes sociais"))
                return "sociologia clássica";

            if (t.Contains("desigualdade") || t.Contains("pobreza") || t.Contains("exclusão social") ||
                t.Contains("mobilidade social") || t.Contains("gini") || t.Contains("idh"))
                return "desigualdade e desenvolvimento social";

            if (t.Contains("religião") || t.Contains("laicidade") || t.Contains("estado laico") ||
                t.Contains("intolerância religiosa") || t.Contains("sincretismo"))
                return "religião e sociedade";

            return "ciências humanas — geral";
        }
    }
}