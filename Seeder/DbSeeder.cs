using System.Net.Http.Json;
using Learnly.Domain.Entities.Simulados;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Repository.Seed
{
    public static class EnemSeeder
    {
        public static async Task SeedAsync(LearnlyContexto contexto)
        {
            if (contexto.Questoes.Any())
            {
                Console.WriteLine("Banco já contém questões. Seed ignorado.");
                return;
            }

            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.enem.dev/v1/")
            };

            for (int ano = 2009; ano <= 2023; ano++)
            {
                Console.WriteLine($"\n=== ENEM {ano} ===");

                int total = 180;
                int limit = 50;

                for (int offset = 0; offset < total; offset += limit)
                {
                    int currentLimit = Math.Min(limit, total - offset);

                    try
                    {
                        var response = await client.GetAsync(
                            $"exams/{ano}/questions?limit={currentLimit}&offset={offset}"
                        );

                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"❌ HTTP {ano} [{offset}-{offset + currentLimit}]");
                            continue;
                        }

                        var result = await response.Content.ReadFromJsonAsync<EnemApiResponse>();

                        if (result?.questions == null || result.questions.Count == 0)
                            continue;

                        foreach (var q in result.questions)
                        {
                            // 🔒 Validações obrigatórias
                            if (string.IsNullOrWhiteSpace(q.title) ||
                                string.IsNullOrWhiteSpace(q.context) ||
                                string.IsNullOrWhiteSpace(q.alternativesIntroduction) ||
                                string.IsNullOrWhiteSpace(q.correctAlternative))
                                continue;

                            if (q.alternatives == null || q.alternatives.Count < 2)
                                continue;

                            var alternativas = new List<Alternativa>();

                            foreach (var alt in q.alternatives)
                            {
                                bool valida =
                                    !string.IsNullOrWhiteSpace(alt.text) ||
                                    !string.IsNullOrWhiteSpace(alt.file);

                                if (!valida || string.IsNullOrWhiteSpace(alt.letter))
                                    continue;

                                alternativas.Add(new Alternativa
                                {
                                    Letra = alt.letter,
                                    Texto = string.IsNullOrWhiteSpace(alt.text) ? null : alt.text.Trim(),
                                    Arquivo = string.IsNullOrWhiteSpace(alt.file) ? null : alt.file.Trim(),
                                    Correta = alt.letter == q.correctAlternative
                                });
                            }

                            if (alternativas.Count < 2)
                                continue;

                            var questao = new Questao
                            {
                                Titulo = q.title.Trim(),
                                Disciplina = q.discipline.Trim(),
                                Lingua = string.IsNullOrWhiteSpace(q.language) ? "pt" : q.language,
                                Ano = q.year,
                                Contexto = q.context.Trim(),
                                IntroducaoAlternativa = q.alternativesIntroduction.Trim(),
                                AlternativaCorreta = q.correctAlternative,
                                Arquivos = q.files != null && q.files.Length > 0
                                    ? string.Join(";", q.files)
                                    : null,
                                Alternativas = alternativas
                            };

                            contexto.Questoes.Add(questao);
                        }

                        await contexto.SaveChangesAsync();
                        Console.WriteLine($"✔ Batch {offset + 1}-{offset + currentLimit}");
                        await Task.Delay(1200);
                    }
                    catch (DbUpdateException ex)
                    {
                        Console.WriteLine($"💥 DB {ano}: {ex.InnerException?.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"🔥 Geral {ano}: {ex.Message}");
                    }
                }
            }

            Console.WriteLine("\nSeed finalizado com sucesso!");
        }
    }

    // ================= DTOs =================

    public class EnemApiResponse
    {
        public MetadataDTO metadata { get; set; }
        public List<QuestaoDTO> questions { get; set; }
    }

    public class MetadataDTO
    {
        public int limit { get; set; }
        public int offset { get; set; }
        public int total { get; set; }
        public bool hasMore { get; set; }
    }

    public class QuestaoDTO
    {
        public string title { get; set; }
        public int index { get; set; }
        public string discipline { get; set; }
        public string language { get; set; }
        public int year { get; set; }
        public string context { get; set; }
        public string[] files { get; set; }
        public string correctAlternative { get; set; }
        public string alternativesIntroduction { get; set; }
        public List<AlternativaDTO> alternatives { get; set; }
    }

    public class AlternativaDTO
    {
        public string letter { get; set; }
        public string? text { get; set; }
        public string? file { get; set; }
        public bool isCorrect { get; set; }
    }
}
