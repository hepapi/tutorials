# Docker Hands-On Training

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
        cd docker/example-projects/node
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
        docker iamge list
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
        docker image build -t simple-node-app:small-size --file ./Dockerfile-small-size.yaml .
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
        cd docker/example-projects/dotnet
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
        docker build -t dotnet:multi-stage -f dotnet-multistage-dockerfile  .
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

## Common Docker Commands

### Pulling Images from Registry

1. List images in your local environment:
    ```bash
    docker images
    ```
2. Pull an image from Docker Hub:
    ```bash
    docker pull nginx:stable-perl
    docker images | grep -i nginx
    ```
3. Try pulling a private image:
    ```bash
    docker pull <yourusername>/sample-app:latest # You may get an access denied error, try again after docker login
    ```

### Logging into Container Image Registry
1. Docker Hub login:
    ```bash
    docker login
    docker login -u <username> -p <password>
    ```
2. Pull the private image again after logging in:
    ```bash
    docker pull <yourusername>/sample-app:latest
    ```
3. Login to another registry (e.g., Nexus):
    ```bash
    docker login nexus_url:port
    ```

### Pushing Docker Image to Registry
1. Build and tag your image with your registry account:
    ```bash
    docker build -t <yourusername>/simple-node-app:v1 .
    docker push <yourusername>/simple-node-app:v1

    docker build -t <yourusername>/simple-node-app:small-size --file ./Dockerfile-small-size .
    docker push <yourusername>/simple-node-app:small-size
    ```
2. Check the new images and tags on your Docker Hub account.

### Killing  containers
1. List running containers:
    ```bash
    docker ps
    docker ps -a # list all containers, including stopped ones
    ```
2. Remove a container:
    ```bash
    docker rm -f <container-id>
    ```

## Docker-Compose & Volumes
- go to example node project
    ```bash
         cd docker/example-projects/node
    ```
1. create  `docker-compose.yaml` file:
    ```yaml
    version: '3.8'
    services:
        app:
            image: simple-node-app:small-size
            ports:
            - "3000:3000"
            volumes:
            - app-data:/usr/src/app/data
    volumes:
        app-data:
    ```
2. Start services using Docker Compose:
    ```bash
    docker-compose up
    docker-compose up -d # detach mode
    ```
3. Discuss multi-container applications and using Docker Compose for orchestration.

### Create Multiple Containers with Docker-Compose
1. Navigate to the Java app `petclinic`:
    ```bash
    cd ../java
    ```
2. Examine the dockerfile and Build the Docker image using a multi-stage Dockerfile:
    ```bash
    docker build -t java-app:v1 .
    ```
3. Observe the `docker-compose.yml` file:
    ```yaml
    version: "3.9"
    services:
        mysql-server:
            image: mysql:8.2
            environment:
                MYSQL_ROOT_PASSWORD: 
                MYSQL_ALLOW_EMPTY_PASSWORD: true
                MYSQL_USER: petclinic
                MYSQL_PASSWORD: petclinic
                MYSQL_DATABASE: petclinic
            ports:
            - "3306:3306"
            networks:
            - petnet
            volumes:
            - mysql-data:/var/lib/mysql
        petclinic:
            image: java-app:v1
            restart: always
            depends_on:
            - mysql-server
            ports:
            - "9090:8080"
            networks:
            - petnet
    networks:
        petnet:
    volumes:
        mysql-data:
    ```
4. Start the services using Docker Compose:
    ```bash
    docker-compose up
    ```
5. Open your browser and go to http://localhost:9090

## Other Useful Docker Commands

### Docker Exec
1. List running services and check their status:
    ```bash
    docker ps
    ```
2. Run commands inside a container from outside:
    ```bash
    docker exec <container-id> pwd
    docker exec <container-id> ls -al
    docker exec -it <container-id> sh
    ```
### Docker Logs
1. View container logs:
    ```bash
    docker logs <container_id>
    docker logs -f <container_id> # follow live logs
    docker logs --since 1h <container_id> # logs from the last hour
    docker logs --tail 100 <container_id> # last 100 log entries
    ```




