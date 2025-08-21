# --- Stage 1: Build ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

# Set the working directory inside the container
WORKDIR /src

# Copy and restore the project file first (for better caching)
COPY InMemoryCache.csproj ./
RUN dotnet restore

# Copy the entire project and publish it
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# --- Stage 2: Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Set working directory inside runtime container
WORKDIR /app

# Expose the default HTTP port
EXPOSE 8080
EXPOSE 8081

# Copy the build output from the build stage
COPY --from=build-env /app/publish .

# Run the application
ENTRYPOINT ["dotnet", "InMemoryCache.dll"]




#docker build -t inmemorycache-app .
#docker run -d \
  #-p 8080:8080 \
  #--name inmemorycache-container \
  #-e ASPNETCORE_URLS=http://+:8080 \
  #inmemorycache-app

