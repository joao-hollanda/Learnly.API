using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Learnly.Domain.Entities.Planos
{
    public class GrupoEstudo
    {
        public int GrupoId { get; set; }
        public string Chave { get; set; }

        public int CriadorId { get; set; }
        [JsonIgnore]
        public Usuario Criador { get; set; }

        public DateTime DataCriacao { get; set; }

        [JsonIgnore]
        public ICollection<PlanoEstudo> Planos { get; set; }

        public GrupoEstudo()
        {
            DataCriacao = DateTime.UtcNow;
        }
    }
}
