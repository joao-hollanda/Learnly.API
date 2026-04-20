using FluentValidation;
using Learnly.Application.DTOs;
using Learnly.Application.Interfaces;
using Learnly.Domain.Entities;
using Learnly.Domain.Interfaces.Repositories;

namespace Learnly.Application.Services
{
    public class EventoEstudoAplicacao : IEventoEstudoAplicacao
    {
        private readonly IEventoEstudoRepositorio _repositorio;
        private readonly IValidator<EventoEstudo> _eventoValidator;
        private readonly IValidator<CriarEventoEstudoDto> _dtoValidator;

        public EventoEstudoAplicacao(
            IEventoEstudoRepositorio repositorio,
            IValidator<EventoEstudo> eventoValidator,
            IValidator<CriarEventoEstudoDto> dtoValidator)
        {
            _repositorio = repositorio;
            _eventoValidator = eventoValidator;
            _dtoValidator = dtoValidator;
        }

        public async Task<List<EventoEstudo>> Listar(int usuarioId)
        {
            return await _repositorio.ObterPorUsuario(usuarioId);
        }

        public async Task Criar(string titulo, DateTime inicio, DateTime fim, int usuarioId)
        {
            var evento = new EventoEstudo
            {
                Titulo = titulo,
                Inicio = inicio,
                Fim = fim,
                UsuarioId = usuarioId
            };

            await _eventoValidator.ValidateAndThrowAsync(evento);
            await _repositorio.Adicionar(evento);
        }

        public async Task Remover(int eventoId)
        {
            await _repositorio.Remover(eventoId);
        }

        public async Task CriarEmLote(int usuarioId, List<CriarEventoEstudoDto> eventosDto)
        {
            if (eventosDto == null || !eventosDto.Any())
                throw new ArgumentException("Lista de eventos vazia.");

            foreach (var dto in eventosDto)
                await _dtoValidator.ValidateAndThrowAsync(dto);

            var eventos = eventosDto.Select(e => new EventoEstudo
            {
                Titulo = e.Titulo,
                Inicio = e.Inicio,
                Fim = e.Fim,
                UsuarioId = usuarioId
            }).ToList();

            await _repositorio.AdicionarEmLote(eventos);
        }
    }
}
