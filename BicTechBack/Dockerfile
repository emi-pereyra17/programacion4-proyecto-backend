# =========================
# Etapa 1: Build
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar todos los proyectos de producción
COPY src/Application/*.csproj ./src/Application/
COPY src/Infrastructure/*.csproj ./src/Infrastructure/
COPY src/Domain/*.csproj ./src/Domain/
COPY src/Presentation/*.csproj ./src/Presentation/
COPY BicTechBack.sln ./

# Restaurar dependencias solo de Presentation (evita errores de tests)
RUN dotnet restore src/Presentation/Presentation.csproj

# Copiar todo el código
COPY . .

# Publicar solo Presentation
RUN dotnet publish src/Presentation/Presentation.csproj -c Release -o /app

# =========================
# Etapa 2: Runtime
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app .

ENTRYPOINT ["dotnet", "Presentation.dll"]


