# Learnly API

Backend da aplicação **Learnly**, uma plataforma educacional voltada ao gerenciamento inteligente de estudos, eventos acadêmicos, disciplinas e progresso do aluno.

---

## Visão Geral

A **Learnly API** foi desenvolvida em **ASP.NET Core**, seguindo uma arquitetura em camadas e princípios de boas práticas de engenharia de software.

A API centraliza toda a lógica de negócio da plataforma, garantindo organização, escalabilidade e manutenibilidade do sistema.

Principais responsabilidades:

* Gerenciamento de usuários
* Criação e organização de planos de estudo
* Controle de eventos e horários
* Geração de simulados no padrão ENEM
* Monitoramento de progresso do aluno

---

## Tecnologias Utilizadas

* .NET / ASP.NET Core Web API
* C#
* Entity Framework Core
* SQL Server
* Arquitetura em camadas

---

## Estrutura do Projeto

```text
Learnly.API
│
├── Learnly.Api          # Controllers, Program.cs e configurações da API
├── Learnly.Application # DTOs, casos de uso e regras de aplicação
├── Learnly.Domain      # Entidades e regras de domínio
├── Learnly.Repository  # Persistência e configuração do EF Core
├── Learnly.Services    # Serviços de negócio
├── Seeder              # Popular banco com dados iniciais
└── Learnly.sln         # Solução principal
```

---

## Pré-requisitos

Antes de iniciar, certifique-se de possuir:

* .NET SDK (versão compatível com o projeto)
* SQL Server (ou outro banco configurado)
* Git

---

## Configuração do Ambiente

### Clonar o repositório

```bash
git clone https://github.com/joao-hollanda/Learnly.API
cd Learnly.API
```

---

### Configurar o banco de dados

Edite o arquivo:

`Learnly.Api/appsettings.json`

Exemplo:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=LearnlyDB;Trusted_Connection=True;"
}
```

---

### Restaurar dependências

```bash
dotnet restore
```

---

## Migrations e Banco de Dados

Para criar o banco e aplicar as migrations:

```bash
dotnet ef database update
```

---

## Seeder (Dados Iniciais)

O projeto possui um seeder para popular o banco com dados iniciais, facilitando testes e desenvolvimento.

---

## Executando a API

```bash
dotnet run --project Learnly.Api
```

Endereços padrão:

```
https://localhost:5001
http://localhost:5000
```

---

## Documentação da API

Se o Swagger estiver habilitado:

```
https://localhost:5001/swagger
```

---

## Funcionalidades Principais

* Cadastro e autenticação de usuários
* Criação e gerenciamento de planos de estudo
* Organização de eventos e horários
* Simulados no padrão ENEM com correção automática
* Feedback assistido por IA
* Controle de progresso acadêmico
* Chatbot educacional

---

## Arquitetura e Padrões

* Arquitetura em camadas
* Separação clara de responsabilidades
* Domínio isolado da infraestrutura
* Serviços desacoplados
* Preparado para testes automatizados

---

## Testes

Projeto de testes ainda não incluído — seção reservada para evolução futura.

---

## Autor

**João Victor Hollanda**
Desenvolvedor Full Stack em formação
