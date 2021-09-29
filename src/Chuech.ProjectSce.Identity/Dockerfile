#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done
RUN dotnet restore "src/Chuech.ProjectSce.Identity/Chuech.ProjectSce.Identity.csproj" --no-cache
COPY . .
WORKDIR "/src/Chuech.ProjectSce.Identity"
RUN dotnet build "Chuech.ProjectSce.Identity.csproj" --no-restore -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chuech.ProjectSce.Identity.csproj" --no-restore -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chuech.ProjectSce.Identity.dll"]