using System;
using System.Collections.Generic;

namespace Learnly.Application.DTOs
{
    public class GrupoDto
    {
        public int GrupoId { get; set; }
        public int ConversaId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Chave { get; set; }
        public int CriadorId { get; set; }
        public int TotalMembros { get; set; }
        public bool SouAdmin { get; set; }
        public DateTime DataCriacao { get; set; }
    }

    public class MembroGrupoDto
    {
        public int UsuarioId { get; set; }
        public string Nome { get; set; }
        public string Papel { get; set; }
    }

    public class GrupoDetalheDto
    {
        public int GrupoId { get; set; }
        public int ConversaId { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Chave { get; set; }
        public int CriadorId { get; set; }
        public bool SouAdmin { get; set; }
        public List<MembroGrupoDto> Membros { get; set; } = new();
    }
}
