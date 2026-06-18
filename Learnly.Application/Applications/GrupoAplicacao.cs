using System.Security.Cryptography;
using FluentValidation;
using Learnly.Application.DTOs;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities.Social;
using Learnly.Domain.Enums;
using Learnly.Domain.Exceptions.Social;
using Learnly.Repository.Interfaces;

namespace Learnly.Application.Applications
{
    public class GrupoAplicacao : IGrupoAplicacao
    {
        readonly IGrupoRepositorio _grupoRepositorio;
        readonly IChatRepositorio _chatRepositorio;
        readonly IValidator<Grupo> _validator;

        public GrupoAplicacao(
            IGrupoRepositorio grupoRepositorio,
            IChatRepositorio chatRepositorio,
            IValidator<Grupo> validator)
        {
            _grupoRepositorio = grupoRepositorio;
            _chatRepositorio = chatRepositorio;
            _validator = validator;
        }

        public async Task<GrupoDto> Criar(int criadorId, string nome, string descricao)
        {
            var grupo = new Grupo
            {
                Nome = nome?.Trim(),
                Descricao = descricao?.Trim(),
                CriadorId = criadorId,
                Chave = await GerarChaveUnica()
            };

            await _validator.ValidateAndThrowAsync(grupo);

            await _grupoRepositorio.Criar(grupo);

            await _grupoRepositorio.AdicionarMembro(new GrupoMembro
            {
                GrupoId = grupo.GrupoId,
                UsuarioId = criadorId,
                Papel = GrupoPapel.Admin
            });

            var conversa = new Conversa
            {
                Tipo = ConversaTipo.Grupo,
                GrupoId = grupo.GrupoId
            };
            await _chatRepositorio.CriarConversa(conversa);

            return new GrupoDto
            {
                GrupoId = grupo.GrupoId,
                ConversaId = conversa.ConversaId,
                Nome = grupo.Nome,
                Descricao = grupo.Descricao,
                Chave = grupo.Chave,
                CriadorId = criadorId,
                TotalMembros = 1,
                SouAdmin = true,
                DataCriacao = grupo.DataCriacao
            };
        }

        public async Task<GrupoDto> Entrar(int usuarioId, string chave)
        {
            var grupo = await _grupoRepositorio.ObterPorChave(chave?.Trim().ToUpperInvariant())
                ?? throw new ChaveGrupoInvalidaException();

            if (await _grupoRepositorio.EhMembro(grupo.GrupoId, usuarioId))
                throw new JaMembroDoGrupoException();

            await _grupoRepositorio.AdicionarMembro(new GrupoMembro
            {
                GrupoId = grupo.GrupoId,
                UsuarioId = usuarioId,
                Papel = GrupoPapel.Membro
            });

            var conversa = await _chatRepositorio.ObterConversaDoGrupo(grupo.GrupoId);

            return new GrupoDto
            {
                GrupoId = grupo.GrupoId,
                ConversaId = conversa?.ConversaId ?? 0,
                Nome = grupo.Nome,
                Descricao = grupo.Descricao,
                Chave = grupo.Chave,
                CriadorId = grupo.CriadorId,
                TotalMembros = (grupo.Membros?.Count ?? 0) + 1,
                SouAdmin = false,
                DataCriacao = grupo.DataCriacao
            };
        }

        public async Task Sair(int grupoId, int usuarioId)
        {
            var membro = await _grupoRepositorio.ObterMembro(grupoId, usuarioId)
                ?? throw new NaoMembroDoGrupoException();

            await _grupoRepositorio.RemoverMembro(membro);
        }

        public async Task<List<GrupoDto>> ListarMeus(int usuarioId)
        {
            var grupos = await _grupoRepositorio.ListarPorUsuario(usuarioId);
            var resultado = new List<GrupoDto>();

            foreach (var grupo in grupos)
            {
                var conversa = await _chatRepositorio.ObterConversaDoGrupo(grupo.GrupoId);
                var souAdmin = grupo.Membros.Any(m => m.UsuarioId == usuarioId && m.Papel == GrupoPapel.Admin);

                resultado.Add(new GrupoDto
                {
                    GrupoId = grupo.GrupoId,
                    ConversaId = conversa?.ConversaId ?? 0,
                    Nome = grupo.Nome,
                    Descricao = grupo.Descricao,
                    Chave = grupo.Chave,
                    CriadorId = grupo.CriadorId,
                    TotalMembros = grupo.Membros.Count,
                    SouAdmin = souAdmin,
                    DataCriacao = grupo.DataCriacao
                });
            }

            return resultado;
        }

        public async Task<GrupoDetalheDto> Obter(int grupoId, int usuarioId)
        {
            var grupo = await _grupoRepositorio.Obter(grupoId)
                ?? throw new GrupoNaoEncontradoException(grupoId);

            if (!grupo.Membros.Any(m => m.UsuarioId == usuarioId))
                throw new NaoMembroDoGrupoException();

            var conversa = await _chatRepositorio.ObterConversaDoGrupo(grupoId);
            var souAdmin = grupo.Membros.Any(m => m.UsuarioId == usuarioId && m.Papel == GrupoPapel.Admin);

            return new GrupoDetalheDto
            {
                GrupoId = grupo.GrupoId,
                ConversaId = conversa?.ConversaId ?? 0,
                Nome = grupo.Nome,
                Descricao = grupo.Descricao,
                Chave = grupo.Chave,
                CriadorId = grupo.CriadorId,
                SouAdmin = souAdmin,
                Membros = grupo.Membros
                    .Select(m => new MembroGrupoDto
                    {
                        UsuarioId = m.UsuarioId,
                        Nome = m.Usuario?.Nome,
                        Papel = m.Papel.ToString()
                    })
                    .OrderBy(m => m.Nome)
                    .ToList()
            };
        }

        private async Task<string> GerarChaveUnica()
        {
            string chave;
            do
            {
                chave = GerarChave();
            } while (await _grupoRepositorio.ChaveExiste(chave));

            return chave;
        }

        private static string GerarChave()
        {
            const string alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var caracteres = new char[6];

            for (int i = 0; i < caracteres.Length; i++)
                caracteres[i] = alfabeto[RandomNumberGenerator.GetInt32(alfabeto.Length)];

            return new string(caracteres);
        }
    }
}
