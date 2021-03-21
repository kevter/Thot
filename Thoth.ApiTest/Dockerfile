FROM mcr.microsoft.com/dotnet/core/runtime-deps:3.1-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY . .
RUN dotnet build "Thoth.ApiTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Thoth.ApiTest.csproj" --runtime linux-musl-x64  -c Release -o /app/publish /p:Version=1.0.0 -p:PublishTrimmed=true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./Thoth.ApiTest"]