# Use the official .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Set the working directory
WORKDIR /app

# Copy the project file and restore any dependencies
COPY MyApp.csproj ./
RUN dotnet restore

# Copy the rest of the application
COPY . ./

# Build the application
RUN dotnet publish -c release -o /app

# Set the working directory to the output folder
WORKDIR /app

# Expose the application port
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "MyApp.dll"]
