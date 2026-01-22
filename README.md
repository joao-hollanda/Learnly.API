# 🚀 Learnly API – Backend

Backend da aplicação **Learnly**, uma plataforma educacional para gerenciamento de planos de estudo, eventos, disciplinas e progresso do aluno.

---

## 📌 Visão Geral

Esta API foi desenvolvida em **ASP.NET Core** seguindo uma arquitetura em camadas, com foco em organização, escalabilidade e boas práticas de engenharia de software.

Ela é responsável por:

* Gerenciar usuários
* Criar e organizar planos de estudo
* Controlar eventos e horários
* Gerar Simulados estilo ENEM

---

## 🛠️ Tecnologias Utilizadas

* **.NET / ASP.NET Core Web API**
* **C#**
* **Entity Framework Core**
* **SQL Server**
* Arquitetura em camadas

---

## 🧱 Estrutura do Projeto

```
Learnly.API
│
├── Learnly.Api          # Controllers, Program.cs e configuração da API
├── Learnly.Application # DTOs, casos de uso e regras de aplicação
├── Learnly.Domain      # Entidades e regras de domínio
├── Learnly.Repository  # Persistência e configuração do EF Core
├── Learnly.Services    # Serviços de negócio
├── Seeder              # Popular banco com dados iniciais
└── Learnly.sln         # Solução principal
```

---

## ⚙️ Pré-requisitos

Antes de iniciar, certifique-se de ter instalado:

* [.NET SDK](https://dotnet.microsoft.com/) (versão compatível com o projeto)
* SQL Server (ou outro banco configurado)
* Git

---

## 🔧 Configuração do Ambiente

### 1. Clonar o repositório

```bash
git clone https://github.com/joao-hollanda/Learnly.API
cd Learnly.API
```

---

### 2. Configurar o banco de dados

Edite o arquivo:

`Learnly.Api/appsettings.json`

Exemplo:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=LearnlyDB;Trusted_Connection=True;"
}
```

---

### 3. Restaurar dependências

```bash
dotnet restore
```

---

## 🗄️ Migrations e Banco de Dados

Para criar o banco e aplicar as migrations:

```bash
dotnet ef database update
```

> Observação: o comando pode variar dependendo da configuração da solução.

---

## 🌱 Seeder (Dados Iniciais)

O projeto possui um **Seeder** para popular o banco com dados iniciais.

Recomendado executar após criar o banco para facilitar testes e desenvolvimento.

---

## ▶️ Executando a API

Na raiz da solução:

```bash
dotnet run --project Learnly.Api
```

A API ficará disponível em:

```
https://localhost:5001
```

ou

```
http://localhost:5000
```

---

## 📚 Documentação da API

Se o projeto estiver configurado com Swagger, acesse:

```
https://localhost:5001/swagger
```

---

## 📌 Funcionalidades Principais

* Cadastro e autenticação de usuários
* Criação de planos de estudo
* Gerenciamento de eventos
* Simulados com Questões do Enem e Feedback gerado por IA
* Controle de horários e progresso
* Chatbot com Inteligência Artificial

---

## 🔒 Arquitetura e Padrões

* Arquitetura em camadas
* Separação clara de responsabilidades
* Domínio isolado das regras de negócio
* Persistência centralizada

---

## 🧪 Testes

*(Projeto de testes ainda não incluído — seção reservada para expansão futura)*

---

## 👤 Autor

**João Victor Hollanda**
