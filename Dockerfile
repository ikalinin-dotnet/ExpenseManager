FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for layer caching
COPY ExpenseManager.sln Directory.Build.props ./
COPY src/ExpenseManager.Domain/ExpenseManager.Domain.csproj src/ExpenseManager.Domain/
COPY src/ExpenseManager.Application/ExpenseManager.Application.csproj src/ExpenseManager.Application/
COPY src/ExpenseManager.Infrastructure/ExpenseManager.Infrastructure.csproj src/ExpenseManager.Infrastructure/
COPY src/ExpenseManager.API/ExpenseManager.API.csproj src/ExpenseManager.API/
COPY tests/ExpenseManager.Domain.Tests/ExpenseManager.Domain.Tests.csproj tests/ExpenseManager.Domain.Tests/
COPY tests/ExpenseManager.Application.Tests/ExpenseManager.Application.Tests.csproj tests/ExpenseManager.Application.Tests/
COPY tests/ExpenseManager.API.Tests/ExpenseManager.API.Tests.csproj tests/ExpenseManager.API.Tests/

RUN dotnet restore

# Copy everything and publish
COPY . .
RUN dotnet publish src/ExpenseManager.API/ExpenseManager.API.csproj -c Release -o /app/publish --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ExpenseManager.API.dll"]
