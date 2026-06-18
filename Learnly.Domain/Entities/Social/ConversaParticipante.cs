using System;
using System.Text.Json.Serialization;

namespace Learnly.Domain.Entities.Social
{
    public class ConversaParticipante
    {
        public int ConversaParticipanteId { get; set; }

        public int ConversaId { get; set; }
        [JsonIgnore]
        public Conversa Conversa { get; set; }

        public int UsuarioId { get; set; }
        [JsonIgnore]
        public Usuario Usuario { get; set; }

        public DateTime? UltimaLeitura { get; set; }
    }
}
