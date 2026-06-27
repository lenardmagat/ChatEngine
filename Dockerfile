# 1. Use the official .NET 10 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy your actual project file and restore dependencies
COPY ["WebApplication1.csproj", "./"]
RUN dotnet restore "./WebApplication1.csproj"

# Copy the rest of the source code and build it
COPY . .
RUN dotnet build "WebApplication1.csproj" -c Release -o /app/build

# 2. Publish the compiled binaries
FROM build AS publish
RUN dotnet publish "WebApplication1.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 3. Build the final runtime image using .NET 10 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WebApplication1.dll"]