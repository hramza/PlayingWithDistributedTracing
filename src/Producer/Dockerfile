#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Producer/Producer.csproj", "Producer/"]
COPY ["src/SearchEngine.Shared/SearchEngine.Shared.csproj", "SearchEngine.Shared/"]
RUN dotnet restore "Producer/Producer.csproj"
COPY ["src/Producer/.", "Producer/"]
COPY ["src/SearchEngine.Shared/.", "SearchEngine.Shared/"]
WORKDIR "/src/Producer"

FROM build AS publish
RUN dotnet publish "Producer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Producer.dll"]