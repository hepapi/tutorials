# Docker Hands-On Training - Session 1

## Pre-requirements
- Install Docker Desktop
- Create an account on Docker Hub
    - https://hub.docker.com/ # Click sign up in the upper right corner

## Installation of Docker Desktop
- For Windows: https://docs.docker.com/desktop/install/windows-install/
- For Mac: https://docs.docker.com/desktop/install/mac-install/

## Basic Node.js Application

### Building and Running Docker Images

1. Build a Docker image:
    ```bash
        cd Docker/example-projects/node
        docker build -t simple-node-app:v1 .
    ```

2. Run the Docker container:
    ```bash
        docker run -p 3000:3000 -d simple-node-app:v1
    ```

3. Open your browser and go to http://localhost:3000

### Inspecting Docker Images

1. List all Docker images and inspect their sizes:
    ```bash
        docker image ls
        docker image list
        docker images
    ```

### Reducing Docker Image Size

1. Search for optimized Node.js images on Docker Hub:

    a. Use a smaller base image (e.g., `node:14-alpine`) in your Dockerfile:
    ```Dockerfile-small-size
    FROM node:14-alpine

    # Create app directory
    WORKDIR /usr/src/app

    # Install app dependencies
    COPY package.json ./
    RUN npm install

    # Bundle app source
    COPY . .

    # Create data directory
    RUN mkdir -p /usr/src/app/data

    # Expose port and start application
    EXPOSE 3000
    CMD ["node", "app.js"]
    ```

    b. Build and run the optimized image:
    ```bash
        docker build -t simple-node-app:small-size -f Dockerfile-small-size .
        docker run -p 3001:3000 -d simple-node-app:small-size 
    ```

    c. Compare image sizes:
    ```bash
        docker images 
    ```
    d. Discuss the use of different ports (3000 and 3001).

2. Use Multi-Stage Dockerfile

    a. go to  example application
    ```bash
        cd Docker/example-projects/dotnet
        # build image with single stage dockerfile
        docker build -t dotnet:single-stage .
        # run container with dotnet:single-stage image
        docker run -p 8080:8080 -d dotnet:single-stage
        # go to http://localhost:8080 
        # inspect docker image size
        docker images | grep -i dotnet
    ```
    b.  crate a new multistage dockerfile 
    ```dotnet-multistage-dockerfile
        # Build Stage
        FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
        WORKDIR /source

        # Copy the project file and restore as distinct layers
        COPY MyApp.csproj .
        RUN dotnet restore

        # Copy the remaining source code and build the application
        COPY . .
        RUN dotnet publish -c release -o /app

        # Runtime Stage
        FROM mcr.microsoft.com/dotnet/aspnet:6.0
        WORKDIR /app

        # Copy only the necessary files from the build stage
        COPY --from=build /app .

        # Expose the application port
        EXPOSE 8080

        # Run the application
        ENTRYPOINT ["dotnet", "MyApp.dll"]
    ```
    c. Build and run docker image
    ```bash
        # build docker image with multisatge dockerfile
        docker build -t dotnet:multi-stage -f dotnet-multistage-dockerfile .
        # run container with dotnet:multi-stage image
        docker run -p 8090:8080 -d dotnet:multi-stage
        # go to http://localhost:8090 
        # inspect docker image size
        docker images | grep -i dotnet
    ```
    d. Talk about the advantages of using a multi-stage Dockerfile. Could there be benefits beyond just reducing size?

## Docker Volumes

- When a Docker container stops, its data is lost. To keep data even after the container dies, you should use a volume. Volumes store data on the host, ensuring it's preserved across container restarts.

- run docker container without volume

    ```bash
        docker run -d \
        -p 3000:3000 \
        --name app \
        simple-node-app:small-size
    ```
- Enter some data and kill container with "docker rm -f app" command.

- Re-run container and see that you cannot access old data

- run docker container with volume
```bash
    docker run -d \
    -p 3000:3000 \
    --name app \
    -v app-data:/usr/src/app/data \
    simple-node-app:small-size
```

- Enter some data and kill container with "docker rm -f app" command.

- Re-run container and see that you can access old data



