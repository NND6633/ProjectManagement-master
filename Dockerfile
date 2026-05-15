FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY ProjectManagement/*.csproj ./ProjectManagement/
RUN dotnet restore

COPY ProjectManagement/. ./ProjectManagement/
WORKDIR /app/ProjectManagement
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/ProjectManagement/out ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ProjectManagement.dll"]
