using Learnly.Application.DTOs;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities.Social;
using Learnly.Domain.Enums;
using Learnly.Domain.Exceptions.Social;
using Learnly.Repository.Interfaces;

namespace Learnly.Application.Applications
{
    public class AmizadeAplicacao : IAmizadeAplicacao
    {
        readonly IAmizadeRepositorio _amizadeRepositorio;
        readonly IUsuarioRepositorio _usuarioRepositorio;

        public AmizadeAplicacao(
            IAmizadeRepositorio amizadeRepositorio,
            IUsuarioRepositorio usuarioRepositorio)
        {
            _amizadeRepositorio = amizadeRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
        }

        public async Task<SolicitacaoAmizadeDto> EnviarSolicitacao(int solicitanteId, string emailOuNome)
        {
            var termo = emailOuNome?.Trim();
            if (string.IsNullOrEmpty(termo))
                throw new SolicitacaoAmizadeInvalidaException("Informe o e-mail ou nome do usuário.");

            var alvo = await _usuarioRepositorio.ObterPorEmail(termo)
                ?? await _usuarioRepositorio.ObterPorNome(termo);

            if (alvo == null)
                throw new SolicitacaoAmizadeInvalidaException("Nenhum usuário encontrado com esse e-mail ou nome.");

            if (alvo.Id == solicitanteId)
                throw new SolicitacaoAmizadeInvalidaException("Você não pode adicionar a si mesmo.");

            var existente = await _amizadeRepositorio.ObterEntre(solicitanteId, alvo.Id);
            if (existente != null)
            {
                if (existente.Status == AmizadeStatus.Aceita)
                    throw new SolicitacaoAmizadeInvalidaException("Vocês já são amigos.");

                throw new SolicitacaoAmizadeInvalidaException("Já existe uma solicitação pendente entre vocês.");
            }

            var amizade = new Amizade
            {
                SolicitanteId = solicitanteId,
                DestinatarioId = alvo.Id
            };

            await _amizadeRepositorio.Criar(amizade);

            return new SolicitacaoAmizadeDto
            {
                AmizadeId = amizade.AmizadeId,
                UsuarioId = alvo.Id,
                Nome = alvo.Nome,
                Email = alvo.Email,
                DataSolicitacao = amizade.DataSolicitacao
            };
        }

        public async Task Aceitar(int amizadeId, int usuarioId)
        {
            var amizade = await _amizadeRepositorio.Obter(amizadeId)
                ?? throw new AmizadeNaoEncontradaException();

            if (amizade.DestinatarioId != usuarioId)
                throw new AmizadeNaoEncontradaException();

            if (amizade.Status != AmizadeStatus.Pendente)
                throw new SolicitacaoAmizadeInvalidaException("Esta solicitação já foi respondida.");

            amizade.Aceitar();
            await _amizadeRepositorio.Atualizar(amizade);
        }

        public async Task Recusar(int amizadeId, int usuarioId)
        {
            var amizade = await _amizadeRepositorio.Obter(amizadeId)
                ?? throw new AmizadeNaoEncontradaException();

            if (amizade.DestinatarioId != usuarioId)
                throw new AmizadeNaoEncontradaException();

            await _amizadeRepositorio.Remover(amizade);
        }

        public async Task Remover(int amizadeId, int usuarioId)
        {
            var amizade = await _amizadeRepositorio.Obter(amizadeId)
                ?? throw new AmizadeNaoEncontradaException();

            if (amizade.SolicitanteId != usuarioId && amizade.DestinatarioId != usuarioId)
                throw new AmizadeNaoEncontradaException();

            await _amizadeRepositorio.Remover(amizade);
        }

        public async Task<List<AmigoDto>> ListarAmigos(int usuarioId)
        {
            var amizades = await _amizadeRepositorio.ListarAceitas(usuarioId);

            return amizades
                .Select(a =>
                {
                    var outro = a.SolicitanteId == usuarioId ? a.Destinatario : a.Solicitante;
                    return new AmigoDto
                    {
                        AmizadeId = a.AmizadeId,
                        UsuarioId = outro.Id,
                        Nome = outro.Nome,
                        Email = outro.Email
                    };
                })
                .OrderBy(a => a.Nome)
                .ToList();
        }

        public async Task<List<SolicitacaoAmizadeDto>> ListarPendentesRecebidas(int usuarioId)
        {
            var amizades = await _amizadeRepositorio.ListarPendentesRecebidas(usuarioId);

            return amizades.Select(a => new SolicitacaoAmizadeDto
            {
                AmizadeId = a.AmizadeId,
                UsuarioId = a.Solicitante.Id,
                Nome = a.Solicitante.Nome,
                Email = a.Solicitante.Email,
                DataSolicitacao = a.DataSolicitacao
            }).ToList();
        }

        public async Task<List<SolicitacaoAmizadeDto>> ListarPendentesEnviadas(int usuarioId)
        {
            var amizades = await _amizadeRepositorio.ListarPendentesEnviadas(usuarioId);

            return amizades.Select(a => new SolicitacaoAmizadeDto
            {
                AmizadeId = a.AmizadeId,
                UsuarioId = a.Destinatario.Id,
                Nome = a.Destinatario.Nome,
                Email = a.Destinatario.Email,
                DataSolicitacao = a.DataSolicitacao
            }).ToList();
        }
    }
}
