using FluentValidation;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities.Redacoes;
using Learnly.Domain.Exceptions.Comuns;
using Learnly.Domain.Exceptions.Redacoes;
using Learnly.Repository.Interfaces;
using Newtonsoft.Json;

namespace Learnly.Application.Applications
{
    public class RedacaoAplicacao : IRedacaoAplicacao
    {
        private readonly IRedacaoRepositorio _redacaoRepositorio;
        private readonly IRedacaoIAService _redacaoIAService;
        private readonly IValidator<Redacao> _validator;

        public RedacaoAplicacao(
            IRedacaoRepositorio redacaoRepositorio,
            IRedacaoIAService redacaoIAService,
            IValidator<Redacao> validator)
        {
            _redacaoRepositorio = redacaoRepositorio;
            _redacaoIAService = redacaoIAService;
            _validator = validator;
        }

        public async Task<string> Transcrever(byte[] imagem, string mimeType)
        {
            if (imagem == null || imagem.Length == 0)
                throw new RegraDeNegocioException("Envie uma imagem da redação.");

            return await _redacaoIAService.TranscreverImagemAsync(imagem, mimeType);
        }

        public async Task<string> GerarTema()
        {
            return await _redacaoIAService.GerarTemaAsync();
        }

        public async Task<Redacao> Corrigir(int usuarioId, string tema, string texto)
        {
            var redacao = new Redacao
            {
                UsuarioId = usuarioId,
                Tema = tema,
                Texto = texto,
                Data = DateTime.UtcNow
            };

            await _validator.ValidateAndThrowAsync(redacao);

            var correcao = await _redacaoIAService.CorrigirAsync(tema, texto);

            redacao.NotaC1 = NotaDaCompetencia(correcao, 1);
            redacao.NotaC2 = NotaDaCompetencia(correcao, 2);
            redacao.NotaC3 = NotaDaCompetencia(correcao, 3);
            redacao.NotaC4 = NotaDaCompetencia(correcao, 4);
            redacao.NotaC5 = NotaDaCompetencia(correcao, 5);
            redacao.NotaFinal = redacao.NotaC1 + redacao.NotaC2 + redacao.NotaC3 + redacao.NotaC4 + redacao.NotaC5;
            redacao.ComentariosJson = JsonConvert.SerializeObject(correcao);

            redacao.RedacaoId = await _redacaoRepositorio.Criar(redacao);

            return redacao;
        }

        public async Task<List<Redacao>> Listar(int usuarioId)
        {
            return await _redacaoRepositorio.Listar(usuarioId);
        }

        public async Task<Redacao> Obter(int redacaoId, int usuarioId)
        {
            var redacao = await _redacaoRepositorio.Obter(redacaoId)
                ?? throw new RedacaoNaoEncontradaException(redacaoId);

            if (redacao.UsuarioId != usuarioId)
                throw new RedacaoNaoAutorizadaException();

            return redacao;
        }

        private static int NotaDaCompetencia(CorrecaoRedacao correcao, int numero) =>
            correcao.Competencias.FirstOrDefault(c => c.Numero == numero)?.Nota ?? 0;
    }
}
