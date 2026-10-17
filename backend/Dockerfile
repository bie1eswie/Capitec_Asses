FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore Bams.slnx
RUN dotnet publish src/Bams.Api/Bams.Api.csproj -c Release -o /out --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /out .
USER $APP_UID
EXPOSE 5100
ENTRYPOINT ["dotnet","Bams.Api.dll"]
