using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Learnly.Domain.Enums;

namespace Learnly.Domain.Entities.Social
{
    public class Conversa
    {
        public int ConversaId { get; set; }
        public ConversaTipo Tipo { get; set; }

        public int? GrupoId { get; set; }
        [JsonIgnore]
        public Grupo Grupo { get; set; }

        public DateTime DataCriacao { get; set; }
        public DateTime UltimaMensagemData { get; set; }

        [JsonIgnore]
        public ICollection<ConversaParticipante> Participantes { get; set; }
        [JsonIgnore]
        public ICollection<Mensagem> Mensagens { get; set; }

        public Conversa()
        {
            DataCriacao = DateTime.UtcNow;
            UltimaMensagemData = DateTime.UtcNow;
        }
    }
}
