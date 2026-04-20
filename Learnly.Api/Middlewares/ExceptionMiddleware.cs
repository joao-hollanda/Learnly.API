using System.Net;
using System.Text.Json;
using FluentValidation;
using Learnly.Domain.Exceptions.Autenticacao;
using Learnly.Domain.Exceptions.Comuns;
using Learnly.Domain.Exceptions.Eventos;
using Learnly.Domain.Exceptions.Planos;
using Learnly.Domain.Exceptions.Simulados;
using Learnly.Domain.Exceptions.Usuarios;

namespace Learnly.Api.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            if (exception is ValidationException ve)
            {
                var erros = ve.Errors.Select(e => e.ErrorMessage).ToArray();
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                var validationBody = JsonSerializer.Serialize(new
                {
                    success = false,
                    data = (object)null,
                    errors = erros
                }, _jsonOptions);
                await context.Response.WriteAsync(validationBody);
                return;
            }

            var (statusCode, mensagem) = exception switch
            {
                CredenciaisInvalidasException e  => (HttpStatusCode.Unauthorized,        e.Message),
                TokenInvalidoException e          => (HttpStatusCode.Unauthorized,        e.Message),

                UsuarioNaoEncontradoException e   => (HttpStatusCode.NotFound,            e.Message),
                EmailJaCadastradoException e      => (HttpStatusCode.Conflict,            e.Message),
                SenhaInvalidaException e          => (HttpStatusCode.BadRequest,          e.Message),
                UsuarioInativoException e         => (HttpStatusCode.Forbidden,           e.Message),

                PlanoNaoEncontradoException e     => (HttpStatusCode.NotFound,            e.Message),
                LimitePlanosAtingidoException e   => (HttpStatusCode.UnprocessableEntity, e.Message),
                MateriaJaAdicionadaException e    => (HttpStatusCode.Conflict,            e.Message),
                MateriaNaoEncontradaException e   => (HttpStatusCode.NotFound,            e.Message),
                MateriaDoPlanoNaoEncontradaException e => (HttpStatusCode.NotFound,       e.Message),
                HorasExcedemTotalException e      => (HttpStatusCode.BadRequest,          e.Message),

                SimuladoNaoEncontradoException e  => (HttpStatusCode.NotFound,            e.Message),
                RespostasNaoInformadasException e => (HttpStatusCode.BadRequest,          e.Message),
                QuestaoNaoEncontradaException e   => (HttpStatusCode.NotFound,            e.Message),
                AlternativaNaoEncontradaException e => (HttpStatusCode.NotFound,          e.Message),

                EventoNaoEncontradoException e    => (HttpStatusCode.NotFound,            e.Message),
                EventoDataInvalidaException e     => (HttpStatusCode.BadRequest,          e.Message),

                NaoEncontradoException e          => (HttpStatusCode.NotFound,            e.Message),
                RegraDeNegocioException e         => (HttpStatusCode.UnprocessableEntity, e.Message),
                DomainException e                 => (HttpStatusCode.BadRequest,          e.Message),

                ArgumentException e               => (HttpStatusCode.BadRequest,          e.Message),

                _                                 => (HttpStatusCode.InternalServerError, "Ocorreu um erro interno. Tente novamente mais tarde.")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
                _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new
            {
                success = false,
                data = (object)null,
                errors = new[] { mensagem }
            }, _jsonOptions);
            await context.Response.WriteAsync(body);
        }
    }
}
