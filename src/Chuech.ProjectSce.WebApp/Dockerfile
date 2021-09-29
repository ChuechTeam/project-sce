#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM node:current-alpine AS spa 
WORKDIR /app 
ARG using_external_dev_server=false
COPY ["src/Chuech.ProjectSce.WebApp/project-sce-app", "."] 
RUN mkdir dist
RUN if [[ ${using_external_dev_server} = false ]]; then npm ci; fi
RUN if [[ ${using_external_dev_server} = false ]]; then mkdir dist || npm run-script build; fi

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
ENV ContainerBuild=true
WORKDIR /
COPY src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done
RUN dotnet restore "src/Chuech.ProjectSce.WebApp/Chuech.ProjectSce.WebApp.csproj"
COPY . .
WORKDIR "/src/Chuech.ProjectSce.WebApp"
RUN dotnet build "Chuech.ProjectSce.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
ENV ContainerBuild=true
RUN dotnet publish "Chuech.ProjectSce.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=spa /app/dist /app/project-sce-app/dist
ENTRYPOINT ["dotnet", "Chuech.ProjectSce.WebApp.dll"]