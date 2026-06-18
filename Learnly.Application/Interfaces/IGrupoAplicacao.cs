using Learnly.Application.DTOs;

namespace Learnly.Application.Interfaces
{
    public interface IGrupoAplicacao
    {
        Task<GrupoDto> Criar(int criadorId, string nome, string descricao);
        Task<GrupoDto> Entrar(int usuarioId, string chave);
        Task Sair(int grupoId, int usuarioId);
        Task<List<GrupoDto>> ListarMeus(int usuarioId);
        Task<GrupoDetalheDto> Obter(int grupoId, int usuarioId);
    }
}
