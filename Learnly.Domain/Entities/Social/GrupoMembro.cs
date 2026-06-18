using System;
using System.Text.Json.Serialization;
using Learnly.Domain.Enums;

namespace Learnly.Domain.Entities.Social
{
    public class GrupoMembro
    {
        public int GrupoMembroId { get; set; }

        public int GrupoId { get; set; }
        [JsonIgnore]
        public Grupo Grupo { get; set; }

        public int UsuarioId { get; set; }
        [JsonIgnore]
        public Usuario Usuario { get; set; }

        public GrupoPapel Papel { get; set; }
        public DateTime DataEntrada { get; set; }
        public DateTime? UltimaLeitura { get; set; }

        public GrupoMembro()
        {
            Papel = GrupoPapel.Membro;
            DataEntrada = DateTime.UtcNow;
        }
    }
}
