# Learnly API

Backend da plataforma **Learnly** — preparatório inteligente para o ENEM com planos de estudo, simulados com correção automática e feedback gerado por IA.

---

## Visão Geral

A Learnly API é construída em **ASP.NET Core (.NET 8)** seguindo **Clean Architecture** com separação clara entre as camadas de API, Application, Domain, Repository e Services. Toda a comunicação com o banco é feita via **Entity Framework Core** com **PostgreSQL** (hospedado no Neon).

O backend centraliza:

- Autenticação via **JWT com HttpOnly Cookie**
- Gerenciamento de usuários, planos de estudo e eventos
- Geração e correção de simulados no padrão ENEM
- Feedback e explicações de erros gerados via **Groq API** (LLaMA 3)
- Chatbot educacional com contexto de mentor

---

## Stack

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 8 / ASP.NET Core |
| Linguagem | C# |
| ORM | Entity Framework Core 8 |
| Banco | PostgreSQL (Neon) |
| IA | Groq API — `llama-3.1-8b-instant`, `llama-3.3-70b-versatile` |
| Auth | JWT Bearer + HttpOnly Cookie |
| Container | Docker |

---

## Estrutura do Projeto

```
Learnly.API/
├── Learnly.Api/           # Controllers, Program.cs, modelos de request/response
├── Learnly.Application/   # Casos de uso (Aplicações) e interfaces
├── Learnly.Domain/        # Entidades, enums e DTOs de domínio
├── Learnly.Repository/    # DbContext, configs EF Core, migrations, repositórios
├── Learnly.Services/      # Serviços externos (Groq IA)
├── Seeder/                # Seed de questões via API pública do ENEM
└── Learnly.sln
```

---

## Pré-requisitos

- [.NET SDK 8.0.417](https://dotnet.microsoft.com/download)
- PostgreSQL ou string de conexão Neon
- Chave de API da [Groq](https://console.groq.com)
- Docker (opcional)

---

## Configuração

### 1. Clone o repositório

```bash
git clone https://github.com/joao-hollanda/Learnly.API
cd Learnly.API
```

### 2. Configure o `appsettings.json`

Crie o arquivo `Learnly.Api/appsettings.json` (ignorado pelo `.gitignore`):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=learnly;Username=...;Password=..."
  },
  "jwt": {
    "secretKey": "sua-chave-secreta-aqui",
    "issuer": "learnly-api",
    "audience": "learnly-frontend"
  },
  "ApiKeys": {
    "GroqIA": "gsk_..."
  }
}
```

### 3. Restaure as dependências

```bash
dotnet restore
```

### 4. Aplique as migrations

```bash
dotnet ef database update --project Learnly.Repository --startup-project Learnly.Api
```

### 5. Execute a API

```bash
dotnet run --project Learnly.Api
```

Swagger disponível em: `http://localhost:5080/swagger`

---

## Seed de Questões

O projeto inclui um seeder que importa questões do ENEM (2009–2023) via [enem.dev](https://api.enem.dev):

```bash
dotnet run --project Seeder
```

> O seeder aplica as migrations automaticamente antes de importar os dados. Só executa se o banco estiver vazio.

---

## Docker

```bash
docker build -t learnly-api .
docker run -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="..." \
  -e jwt__secretKey="..." \
  -e ApiKeys__GroqIA="..." \
  learnly-api
```

---

## Endpoints Principais

### Autenticação
| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/login` | Login — seta cookie JWT |
| `POST` | `/api/login/refresh` | Renova o token |
| `POST` | `/api/login/logout` | Invalida o cookie |
| `GET` | `/api/login/AuthCheck` | Verifica autenticação |
| `GET` | `/api/login/user` | Retorna dados do usuário logado |

### Usuários
| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/usuarios/criar` | Cadastro (público) |
| `PUT` | `/api/usuarios` | Atualiza nome/email |
| `DELETE` | `/api/usuarios/desativar/{id}` | Desativa conta |

### Planos de Estudo
| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/plano` | Cria plano (manual ou gerado por IA) |
| `GET` | `/api/plano` | Lista últimos 5 planos |
| `GET` | `/api/plano/{id}` | Busca plano por ID |
| `PUT` | `/api/plano/{id}/ativar` | Ativa um plano |
| `PUT` | `/api/plano/{id}/desativar` | Desativa um plano |
| `DELETE` | `/api/plano/{id}` | Remove plano |
| `POST` | `/api/plano/{id}/materia` | Adiciona matéria ao plano |
| `PUT` | `/api/plano/lancar-horas` | Lança horas estudadas |
| `GET` | `/api/plano/gerar-resumo` | Resumo de horas totais/concluídas |
| `GET` | `/api/plano/horas/comparacao` | Compara horas hoje vs ontem |
| `GET` | `/api/plano/plano-ativo` | Retorna plano ativo do usuário |

### Simulados
| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/simulado` | Gera novo simulado |
| `PUT` | `/api/simulado/responder/{id}` | Responde e corrige o simulado |
| `GET` | `/api/simulado/{id}` | Busca simulado com questões e respostas |
| `GET` | `/api/simulado/listar` | Lista últimos 5 simulados |
| `GET` | `/api/simulado/contar` | Total de simulados realizados |

### Eventos de Estudo
| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/eventos` | Lista eventos do usuário |
| `POST` | `/api/eventos` | Cria evento |
| `POST` | `/api/eventos/lote` | Cria múltiplos eventos de uma vez |
| `DELETE` | `/api/eventos` | Remove todos os eventos do usuário |

### IA
| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/ia/chat` | Chatbot educacional (mentor ENEM) |

---

## Fluxo do Simulado com IA

```
POST /api/simulado
  → Seleciona questões aleatórias por disciplina

PUT /api/simulado/responder/{id}
  → Corrige respostas
  → Gera feedback geral (GerarFeedbackAsync)
  → Gera explicações por questão errada (GerarExplicacoes)
  → Persiste tudo no banco
  → Retorna nota + desempenho
```

As explicações são geradas em uma única chamada à Groq API com todas as questões erradas, evitando rate limiting.

---

## Segurança

- `userId` extraído sempre do JWT via `BaseController.GetUserId()` — nunca da rota
- Verificação de ownership em todos os endpoints que acessam recursos por ID (`PlanoId`, `SimuladoId`)
- Cookies JWT configurados como `HttpOnly`, `Secure` em produção, `SameSite=None` para cross-origin com o frontend no Vercel
- Token expirado limpa o cookie automaticamente via evento `OnAuthenticationFailed`

---

## CORS

Configurado para aceitar requisições de:
- `https://learnly-edu.vercel.app` (produção)
- `http://localhost:3000` (desenvolvimento)

---

## Autor

**João Victor Hollanda** — [github.com/joao-hollanda](https://github.com/joao-hollanda)
