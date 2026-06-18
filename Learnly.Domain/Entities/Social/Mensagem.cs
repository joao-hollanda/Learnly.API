using System;
using System.Text.Json.Serialization;
using Learnly.Domain.Enums;

namespace Learnly.Domain.Entities.Social
{
    public class Mensagem
    {
        public int MensagemId { get; set; }

        public int ConversaId { get; set; }
        [JsonIgnore]
        public Conversa Conversa { get; set; }

        public int RemetenteId { get; set; }
        [JsonIgnore]
        public Usuario Remetente { get; set; }

        public string Texto { get; set; }
        public MensagemTipo Tipo { get; set; }

        public int? AnexoRefId { get; set; }
        public string AnexoPayload { get; set; }

        public DateTime DataEnvio { get; set; }

        public Mensagem()
        {
            Tipo = MensagemTipo.Texto;
            DataEnvio = DateTime.UtcNow;
        }
    }
}
