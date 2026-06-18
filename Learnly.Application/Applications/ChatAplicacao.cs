using FluentValidation;
using Learnly.Application.DTOs;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities.Social;
using Learnly.Domain.Enums;
using Learnly.Domain.Exceptions.Social;
using Learnly.Repository.Interfaces;

namespace Learnly.Application.Applications
{
    public class ChatAplicacao : IChatAplicacao
    {
        readonly IChatRepositorio _chatRepositorio;
        readonly IGrupoRepositorio _grupoRepositorio;
        readonly IAmizadeRepositorio _amizadeRepositorio;
        readonly IUsuarioRepositorio _usuarioRepositorio;
        readonly IValidator<Mensagem> _validator;

        public ChatAplicacao(
            IChatRepositorio chatRepositorio,
            IGrupoRepositorio grupoRepositorio,
            IAmizadeRepositorio amizadeRepositorio,
            IUsuarioRepositorio usuarioRepositorio,
            IValidator<Mensagem> validator)
        {
            _chatRepositorio = chatRepositorio;
            _grupoRepositorio = grupoRepositorio;
            _amizadeRepositorio = amizadeRepositorio;
            _usuarioRepositorio = usuarioRepositorio;
            _validator = validator;
        }

        public async Task<int> ObterOuCriarConversaDireta(int usuarioId, int amigoId)
        {
            if (amigoId == usuarioId)
                throw new SolicitacaoAmizadeInvalidaException("Você não pode conversar consigo mesmo.");

            var amizade = await _amizadeRepositorio.ObterEntre(usuarioId, amigoId);
            if (amizade == null || amizade.Status != AmizadeStatus.Aceita)
                throw new SolicitacaoAmizadeInvalidaException("Vocês precisam ser amigos para conversar.");

            var existente = await _chatRepositorio.ObterConversaDireta(usuarioId, amigoId);
            if (existente != null)
                return existente.ConversaId;

            var conversa = new Conversa
            {
                Tipo = ConversaTipo.Direta,
                Participantes = new List<ConversaParticipante>
                {
                    new() { UsuarioId = usuarioId },
                    new() { UsuarioId = amigoId }
                }
            };

            await _chatRepositorio.CriarConversa(conversa);
            return conversa.ConversaId;
        }

        public async Task<ConversaDetalheDto> ObterConversa(int conversaId, int usuarioId)
        {
            var conversa = await ValidarAcesso(conversaId, usuarioId);

            if (conversa.Tipo == ConversaTipo.Direta)
            {
                var participantes = await _chatRepositorio.ListarParticipantes(conversaId);
                var outro = participantes.FirstOrDefault(p => p.UsuarioId != usuarioId);

                return new ConversaDetalheDto
                {
                    ConversaId = conversaId,
                    Tipo = ConversaTipo.Direta,
                    Titulo = outro?.Usuario?.Nome,
                    OutroUsuarioId = outro?.UsuarioId
                };
            }

            var grupo = await _grupoRepositorio.Obter(conversa.GrupoId!.Value)
                ?? throw new GrupoNaoEncontradoException();

            return new ConversaDetalheDto
            {
                ConversaId = conversaId,
                Tipo = ConversaTipo.Grupo,
                Titulo = grupo.Nome,
                GrupoId = grupo.GrupoId,
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

        public async Task<List<MensagemDto>> ListarMensagens(int conversaId, int usuarioId, int? antesDeId, int limite)
        {
            await ValidarAcesso(conversaId, usuarioId);

            var mensagens = await _chatRepositorio.ListarMensagens(conversaId, antesDeId, limite > 0 ? limite : 30);
            return mensagens.Select(MapearMensagem).ToList();
        }

        public async Task<MensagemDto> EnviarMensagem(int conversaId, int remetenteId, NovaMensagemDto dto)
        {
            var conversa = await ValidarAcesso(conversaId, remetenteId);

            var texto = dto.Texto?.Trim();
            if (dto.Tipo == MensagemTipo.Texto && string.IsNullOrEmpty(texto))
                throw new ArgumentException("A mensagem não pode estar vazia.");

            var mensagem = new Mensagem
            {
                ConversaId = conversaId,
                RemetenteId = remetenteId,
                Texto = texto,
                Tipo = dto.Tipo,
                AnexoRefId = dto.AnexoRefId,
                AnexoPayload = dto.AnexoPayload
            };

            await _validator.ValidateAndThrowAsync(mensagem);

            await _chatRepositorio.AdicionarMensagem(mensagem);

            conversa.UltimaMensagemData = mensagem.DataEnvio;
            await _chatRepositorio.AtualizarConversa(conversa);

            await MarcarLida(conversaId, remetenteId);

            var remetente = await _usuarioRepositorio.Obter(remetenteId, true);

            return new MensagemDto
            {
                MensagemId = mensagem.MensagemId,
                ConversaId = conversaId,
                RemetenteId = remetenteId,
                RemetenteNome = remetente?.Nome,
                Texto = mensagem.Texto,
                Tipo = mensagem.Tipo,
                AnexoRefId = mensagem.AnexoRefId,
                AnexoPayload = mensagem.AnexoPayload,
                DataEnvio = mensagem.DataEnvio
            };
        }

        public async Task MarcarLida(int conversaId, int usuarioId)
        {
            var conversa = await _chatRepositorio.ObterConversa(conversaId)
                ?? throw new ConversaNaoEncontradaException();

            if (conversa.Tipo == ConversaTipo.Direta)
            {
                var participante = await _chatRepositorio.ObterParticipante(conversaId, usuarioId);
                if (participante == null) return;

                participante.UltimaLeitura = DateTime.UtcNow;
                await _chatRepositorio.AtualizarParticipante(participante);
            }
            else if (conversa.GrupoId.HasValue)
            {
                var membro = await _grupoRepositorio.ObterMembro(conversa.GrupoId.Value, usuarioId);
                if (membro == null) return;

                membro.UltimaLeitura = DateTime.UtcNow;
                await _grupoRepositorio.AtualizarMembro(membro);
            }
        }

        public async Task<List<ConversaResumoDto>> ListarConversas(int usuarioId)
        {
            var resultado = new List<ConversaResumoDto>();

            var diretas = await _chatRepositorio.ListarConversasDiretas(usuarioId);
            foreach (var conversa in diretas)
            {
                var outro = conversa.Participantes.FirstOrDefault(p => p.UsuarioId != usuarioId);
                var eu = conversa.Participantes.FirstOrDefault(p => p.UsuarioId == usuarioId);

                var ultima = await _chatRepositorio.ObterUltimaMensagem(conversa.ConversaId);
                var naoLidas = await _chatRepositorio.ContarNaoLidas(conversa.ConversaId, usuarioId, eu?.UltimaLeitura);

                resultado.Add(new ConversaResumoDto
                {
                    ConversaId = conversa.ConversaId,
                    Tipo = ConversaTipo.Direta,
                    Titulo = outro?.Usuario?.Nome,
                    OutroUsuarioId = outro?.UsuarioId,
                    UltimaMensagem = ResumirMensagem(ultima),
                    UltimaMensagemData = ultima?.DataEnvio,
                    NaoLidas = naoLidas
                });
            }

            var grupos = await _grupoRepositorio.ListarPorUsuario(usuarioId);
            foreach (var grupo in grupos)
            {
                var conversa = await _chatRepositorio.ObterConversaDoGrupo(grupo.GrupoId);
                if (conversa == null) continue;

                var membro = grupo.Membros.FirstOrDefault(m => m.UsuarioId == usuarioId);
                var ultima = await _chatRepositorio.ObterUltimaMensagem(conversa.ConversaId);
                var naoLidas = await _chatRepositorio.ContarNaoLidas(conversa.ConversaId, usuarioId, membro?.UltimaLeitura);

                resultado.Add(new ConversaResumoDto
                {
                    ConversaId = conversa.ConversaId,
                    Tipo = ConversaTipo.Grupo,
                    Titulo = grupo.Nome,
                    GrupoId = grupo.GrupoId,
                    UltimaMensagem = ResumirMensagem(ultima),
                    UltimaMensagemData = ultima?.DataEnvio,
                    NaoLidas = naoLidas
                });
            }

            return resultado
                .OrderByDescending(c => c.UltimaMensagemData ?? DateTime.MinValue)
                .ToList();
        }

        public async Task<List<int>> ListarIdsConversas(int usuarioId)
        {
            var ids = new List<int>();

            var diretas = await _chatRepositorio.ListarConversasDiretas(usuarioId);
            ids.AddRange(diretas.Select(c => c.ConversaId));

            var grupos = await _grupoRepositorio.ListarPorUsuario(usuarioId);
            foreach (var grupo in grupos)
            {
                var conversa = await _chatRepositorio.ObterConversaDoGrupo(grupo.GrupoId);
                if (conversa != null) ids.Add(conversa.ConversaId);
            }

            return ids.Distinct().ToList();
        }

        public async Task<List<int>> ListarDestinatarios(int conversaId, int excetoUsuarioId)
        {
            var conversa = await _chatRepositorio.ObterConversa(conversaId);
            if (conversa == null) return new List<int>();

            if (conversa.Tipo == ConversaTipo.Direta)
            {
                var participantes = await _chatRepositorio.ListarParticipantes(conversaId);
                return participantes
                    .Select(p => p.UsuarioId)
                    .Where(id => id != excetoUsuarioId)
                    .ToList();
            }

            if (!conversa.GrupoId.HasValue) return new List<int>();

            var grupo = await _grupoRepositorio.Obter(conversa.GrupoId.Value);
            return grupo == null
                ? new List<int>()
                : grupo.Membros.Select(m => m.UsuarioId).Where(id => id != excetoUsuarioId).ToList();
        }

        private async Task<Conversa> ValidarAcesso(int conversaId, int usuarioId)
        {
            var conversa = await _chatRepositorio.ObterConversa(conversaId)
                ?? throw new ConversaNaoEncontradaException();

            if (conversa.Tipo == ConversaTipo.Direta)
            {
                if (!conversa.Participantes.Any(p => p.UsuarioId == usuarioId))
                    throw new AcessoConversaNegadoException();
            }
            else
            {
                if (!conversa.GrupoId.HasValue ||
                    !await _grupoRepositorio.EhMembro(conversa.GrupoId.Value, usuarioId))
                    throw new AcessoConversaNegadoException();
            }

            return conversa;
        }

        private static MensagemDto MapearMensagem(Mensagem m) => new()
        {
            MensagemId = m.MensagemId,
            ConversaId = m.ConversaId,
            RemetenteId = m.RemetenteId,
            RemetenteNome = m.Remetente?.Nome,
            Texto = m.Texto,
            Tipo = m.Tipo,
            AnexoRefId = m.AnexoRefId,
            AnexoPayload = m.AnexoPayload,
            DataEnvio = m.DataEnvio
        };

        private static string ResumirMensagem(Mensagem m)
        {
            if (m == null) return null;

            return m.Tipo switch
            {
                MensagemTipo.Plano => "Compartilhou um plano de estudo",
                MensagemTipo.Simulado => "Compartilhou um simulado",
                MensagemTipo.SessaoIA => "Compartilhou uma conversa com a IA",
                _ => m.Texto
            };
        }
    }
}
