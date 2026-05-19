using consumindoIA.Domain;
using System.Collections.Generic;

namespace Learnly.Services.IAService
{
    public static class ChatbotTools
    {
        public static List<Tool> ObterFerramentas()
        {
            return new List<Tool>
            {
                // 1. Histórico e Desempenho
                new Tool
                {
                    type = "function",
                    function = new ToolFunction
                    {
                        name = "buscar_desempenho_do_aluno",
                        description = "Busca as notas, média geral e quantidade de acertos do aluno nos últimos simulados para dar conselhos precisos sobre o progresso dele.",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                disciplina = new { type = "string", description = "A disciplina desejada (ex: matematica, linguagens). Opcional." }
                            }
                        }
                    }
                },
                new Tool
                {
                    type = "function",
                    function = new ToolFunction
                    {
                        name = "buscar_pontos_fracos_por_habilidade",
                        description = "Traz as habilidades e micro-tópicos (ex: Cinemática, Geometria, Interpretação) onde o aluno tem a menor taxa de acerto.",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                disciplina = new { type = "string", description = "Disciplina para filtrar as habilidades fracas. Opcional." }
                            }
                        }
                    }
                },
                new Tool
                {
                    type = "function",
                    function = new ToolFunction
                    {
                        name = "revisar_questoes_erradas",
                        description = "Busca questões reais que o aluno errou recentemente em simulados para o Mentor revisar junto com ele.",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                disciplina = new { type = "string", description = "Disciplina para buscar a questão (ex: ciencias-natureza, matematica). Opcional." }
                            }
                        }
                    }
                },
                // 2. Consulta de Planos de Estudo
                new Tool
                {
                    type = "function",
                    function = new ToolFunction
                    {
                        name = "buscar_plano_estudo_atual",
                        description = "Retorna o que o aluno tem programado para estudar na semana/dia atual.",
                        parameters = new
                        {
                            type = "object",
                            properties = new { } // Sem parâmetros necessários
                        }
                    }
                },
                new Tool
                {
                    type = "function",
                    function = new ToolFunction
                    {
                        name = "reajustar_carga_horaria_plano",
                        description = "Altera a quantidade de horas semanais do plano de estudos atual e recalcula o cronograma.",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                novaCargaHoraria = new { type = "integer", description = "A nova quantidade de horas semanais disponíveis." }
                            },
                            required = new[] { "novaCargaHoraria" }
                        }
                    }
                },
                new Tool
                {
                    type = "function",
                    function = new ToolFunction
                    {
                        name = "adicionar_ou_remover_disciplina",
                        description = "Substitui, adiciona ou remove uma disciplina específica do plano de estudos atual do aluno.",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                acao = new { type = "string", description = "Ação desejada: 'adicionar', 'remover' ou 'substituir'" },
                                disciplinaAlvo = new { type = "string", description = "A disciplina que sofrerá a ação principal." },
                                disciplinaSubstituta = new { type = "string", description = "A disciplina nova, caso a ação seja 'substituir'. Opcional." }
                            },
                            required = new[] { "acao", "disciplinaAlvo" }
                        }
                    }
                },
                new Tool
                {
                    type = "function",
                    function = new ToolFunction
                    {
                        name = "reagendar_topicos_atrasados",
                        description = "Redistribui tópicos não estudados das semanas anteriores para as semanas seguintes.",
                        parameters = new
                        {
                            type = "object",
                            properties = new { } // Sem parâmetros, a lógica pega do contexto do aluno logado
                        }
                    }
                },
            };
        }
    }
}
