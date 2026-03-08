using Learnly.Domain.Entities;
using Learnly.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Repository
{
    public class UsuarioRepositorio : BaseRepositorio, IUsuarioRepositorio
    {
        public UsuarioRepositorio(LearnlyContexto contexto) : base(contexto)
        {
        }

        public async Task<int> Criar(Usuario usuario)
        {
            _contexto.Usuarios.Add(usuario);
            await _contexto.SaveChangesAsync();

            return usuario.Id;
        }

        public async Task Atualizar(Usuario usuario)
        {
            _contexto.Usuarios.Update(usuario);
            await _contexto.SaveChangesAsync();
        }

        public async Task<Usuario> Obter(int usuarioId, bool ativo)
        {
            return await _contexto.Usuarios
                .Where(u => u.Id == usuarioId)
                .Where(u => u.StatusConta == ativo)
                .FirstOrDefaultAsync();
        }
        public async Task<Usuario> ObterPorNome(string nome)
        {
            return await _contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Nome.ToLower() == nome.ToLower());
        }

        public async Task<Usuario> ObterPorEmail(string email)
        {
            return await _contexto.Usuarios
                .AsNoTracking()
                .Where(u => u.Email == email)
                .Where(u => u.StatusConta == true)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Usuario>> Listar(bool ativo)
        {
            return await _contexto.Usuarios.Where(u => u.StatusConta == ativo).ToListAsync();
        }
    }
}