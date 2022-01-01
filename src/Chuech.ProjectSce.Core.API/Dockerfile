#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0.101 AS build
WORKDIR /
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done
RUN dotnet restore "src/Chuech.ProjectSce.Core.API/Chuech.ProjectSce.Core.API.csproj" --no-cache
COPY . .
WORKDIR "/src/Chuech.ProjectSce.Core.API"
RUN dotnet build "Chuech.ProjectSce.Core.API.csproj" -c Release -o /app/build /maxcpucount:4

FROM build AS publish
RUN dotnet publish "Chuech.ProjectSce.Core.API.csproj" --no-restore -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chuech.ProjectSce.Core.API.dll"]