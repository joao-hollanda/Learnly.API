using System;
using System.Collections.Generic;
using Learnly.Domain.Enums;

namespace Learnly.Application.DTOs
{
    public class ConversaResumoDto
    {
        public int ConversaId { get; set; }
        public ConversaTipo Tipo { get; set; }
        public string Titulo { get; set; }
        public int? OutroUsuarioId { get; set; }
        public int? GrupoId { get; set; }
        public string UltimaMensagem { get; set; }
        public DateTime? UltimaMensagemData { get; set; }
        public int NaoLidas { get; set; }
    }

    public class ConversaDetalheDto
    {
        public int ConversaId { get; set; }
        public ConversaTipo Tipo { get; set; }
        public string Titulo { get; set; }
        public int? OutroUsuarioId { get; set; }
        public int? GrupoId { get; set; }
        public List<MembroGrupoDto> Membros { get; set; } = new();
    }

    public class MensagemDto
    {
        public int MensagemId { get; set; }
        public int ConversaId { get; set; }
        public int RemetenteId { get; set; }
        public string RemetenteNome { get; set; }
        public string Texto { get; set; }
        public MensagemTipo Tipo { get; set; }
        public int? AnexoRefId { get; set; }
        public string AnexoPayload { get; set; }
        public DateTime DataEnvio { get; set; }
    }

    public class NovaMensagemDto
    {
        public string Texto { get; set; }
        public MensagemTipo Tipo { get; set; }
        public int? AnexoRefId { get; set; }
        public string AnexoPayload { get; set; }
    }
}
