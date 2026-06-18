using System;
using System.Text.Json.Serialization;
using Learnly.Domain.Enums;

namespace Learnly.Domain.Entities.Social
{
    public class Amizade
    {
        public int AmizadeId { get; set; }

        public int SolicitanteId { get; set; }
        [JsonIgnore]
        public Usuario Solicitante { get; set; }

        public int DestinatarioId { get; set; }
        [JsonIgnore]
        public Usuario Destinatario { get; set; }

        public AmizadeStatus Status { get; set; }
        public DateTime DataSolicitacao { get; set; }
        public DateTime? DataResposta { get; set; }

        public Amizade()
        {
            Status = AmizadeStatus.Pendente;
            DataSolicitacao = DateTime.UtcNow;
        }

        public void Aceitar()
        {
            Status = AmizadeStatus.Aceita;
            DataResposta = DateTime.UtcNow;
        }

        public void Recusar()
        {
            Status = AmizadeStatus.Recusada;
            DataResposta = DateTime.UtcNow;
        }
    }
}
