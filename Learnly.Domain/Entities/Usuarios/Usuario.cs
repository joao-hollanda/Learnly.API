namespace Learnly.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public DateOnly DataCriacao { get; set; }
        public bool StatusConta { get; set; }
        public List<PlanoEstudo> PlanoEstudo { get; set; }

        public List<Simulado> Simulados { get; set; }

        // public PlanoAssinatura Plano { get; set; } 

        public Usuario()
        {
            StatusConta = true;
            DataCriacao = DateOnly.FromDateTime(DateTime.UtcNow);
            // Plano = PlanoAssinatura.Gratuito;
        }

        public void Desativar()
        {
            StatusConta = false;
        }
        public void Reativar()
        {
            StatusConta = true;
        }
    }
}