using Learnly.Application.DTOs;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Interfaces.Repositories;

namespace Learnly.Application.Services
{
    public class EventoEstudoAplicacao : IEventoEstudoAplicacao
    {
        private readonly IEventoEstudoRepositorio _repositorio;

        public EventoEstudoAplicacao(IEventoEstudoRepositorio repositorio)
        {
            _repositorio = repositorio;
        }

        public async Task<List<EventoEstudo>> Listar(int usuarioId)
        {
            return await _repositorio.ObterPorUsuario(usuarioId);
        }

        public async Task Criar(string titulo, DateTime inicio, DateTime fim, int usuarioId)
        {
            if (fim <= inicio)
                throw new ArgumentException("Data final deve ser maior que a inicial.");

            var evento = new EventoEstudo
            {
                Titulo = titulo,
                Inicio = inicio,
                Fim = fim,
                UsuarioId = usuarioId
            };

            await _repositorio.Adicionar(evento);
        }

        public async Task Remover(int eventoId)
        {
            await _repositorio.Remover(eventoId);
        }

        public async Task CriarEmLote(
            int usuarioId,
            List<CriarEventoEstudoDto> eventosDto
        )
        {
            if (eventosDto == null || !eventosDto.Any())
                throw new ArgumentException("Lista de eventos vazia.");

            var eventos = eventosDto.Select(e =>
            {
                if (e.Fim <= e.Inicio)
                    throw new ArgumentException("Evento com horário inválido.");

                return new EventoEstudo
                {
                    Titulo = e.Titulo,
                    Inicio = e.Inicio,
                    Fim = e.Fim,
                    UsuarioId = usuarioId
                };
            }).ToList();

            await _repositorio.AdicionarEmLote(eventos);
        }


    }
}
