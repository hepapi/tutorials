# Use the official .NET SDK image
FROM mcr.microsoft.com/dotnet/sdk:10.0

# Set the working directory
WORKDIR /src

# Copy the project file and restore any dependencies
COPY MyApp.csproj ./
RUN dotnet restore

# Copy the rest of the application
COPY . ./

# Build the application
RUN dotnet publish -c release -o /publish

# Set the working directory to the publish output
WORKDIR /publish

# Expose the application port
EXPOSE 8080

# Run the application
ENTRYPOINT ["dotnet", "MyApp.dll"]
