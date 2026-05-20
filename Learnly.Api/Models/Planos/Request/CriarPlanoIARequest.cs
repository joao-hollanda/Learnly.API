namespace Learnly.Api.Models.Planos.Request
{
    public class CriarPlanoIARequest
    {
        public string Titulo { get; set; }
        public string Objetivo { get; set; }
        public int HorasPorSemana { get; set; }
    }
}