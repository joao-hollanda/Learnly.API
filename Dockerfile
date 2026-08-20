# Etapa 1 - build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia tudo
COPY . .

# Restaura dependências
RUN dotnet restore

# Testes unitários — se algum falhar, a imagem não é gerada e o deploy não sai
RUN dotnet test Learnly.Tests/Learnly.Tests.csproj -c Release --no-restore

# Publica a API
RUN dotnet publish Learnly.Api/Learnly.Api.csproj -c Release -o out --no-restore

# Etapa 2 - runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Learnly.Api.dll"]