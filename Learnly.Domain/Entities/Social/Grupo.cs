using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Learnly.Domain.Entities.Social
{
    public class Grupo
    {
        public int GrupoId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Chave { get; set; }

        public int CriadorId { get; set; }
        [JsonIgnore]
        public Usuario Criador { get; set; }

        public DateTime DataCriacao { get; set; }

        [JsonIgnore]
        public ICollection<GrupoMembro> Membros { get; set; }

        public Grupo()
        {
            DataCriacao = DateTime.UtcNow;
        }
    }
}
