using Learnly.Application.DTOs;
using Learnly.Application.Interfaces;
using Learnly.Domain.Enums;
using Learnly.Domain.Exceptions.Comuns;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Learnly.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatAplicacao _chatAplicacao;

        public ChatHub(IChatAplicacao chatAplicacao)
        {
            _chatAplicacao = chatAplicacao;
        }

        private int? UsuarioId()
        {
            var valor = Context.User?.FindFirst("id")?.Value;
            return int.TryParse(valor, out var id) ? id : null;
        }

        public override async Task OnConnectedAsync()
        {
            var usuarioId = UsuarioId();
            if (usuarioId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{usuarioId}");

                var conversas = await _chatAplicacao.ListarIdsConversas(usuarioId.Value);
                foreach (var conversaId in conversas)
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"conversa-{conversaId}");
            }

            await base.OnConnectedAsync();
        }

        public async Task EntrarConversa(int conversaId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversa-{conversaId}");
        }

        public async Task EnviarMensagem(int conversaId, string texto, int tipo, int? anexoRefId, string anexoPayload)
        {
            var usuarioId = UsuarioId();
            if (usuarioId == null) throw new HubException("Não autenticado.");

            var dto = new NovaMensagemDto
            {
                Texto = texto,
                Tipo = (MensagemTipo)tipo,
                AnexoRefId = anexoRefId,
                AnexoPayload = anexoPayload
            };

            try
            {
                var mensagem = await _chatAplicacao.EnviarMensagem(conversaId, usuarioId.Value, dto);
                await Clients.Group($"conversa-{conversaId}").SendAsync("ReceberMensagem", mensagem);
            }
            catch (DomainException ex)
            {
                throw new HubException(ex.Message);
            }
            catch (ArgumentException ex)
            {
                throw new HubException(ex.Message);
            }
        }

        public async Task MarcarLida(int conversaId)
        {
            var usuarioId = UsuarioId();
            if (usuarioId == null) return;

            try
            {
                await _chatAplicacao.MarcarLida(conversaId, usuarioId.Value);
            }
            catch (DomainException) { }
        }
    }
}
