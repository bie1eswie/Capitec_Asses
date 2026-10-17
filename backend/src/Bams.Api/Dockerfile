FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY src/Bams.Api/Bams.Api.csproj src/Bams.Api/
COPY src/Bams.Application/Bams.Application.csproj src/Bams.Application/
COPY src/Bams.Infrastructure/Bams.Infrastructure.csproj src/Bams.Infrastructure/
COPY src/Bams.Domain/Bams.Domain.csproj src/Bams.Domain/
RUN dotnet restore src/Bams.Api/Bams.Api.csproj

# copy everything else and publish
COPY . .
RUN dotnet publish src/Bams.Api/Bams.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5100
EXPOSE 5100
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bams.Api.dll"]
