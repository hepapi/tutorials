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
        cd Docker/node
        docker build -t simple-node-app/v1 .
    ```

2. Run the Docker container:
    ```bash
        docker run -p 3000:3000 -d simple-node-app/v1
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

2. Use a smaller base image (e.g., `node:14-alpine`) in your Dockerfile:
    ```Dockerfile
    FROM node:14-alpine
    ```
3. Build and run the optimized image:
    ```bash
        docker image build -t simple-node-app:small-size --file ./Dockerfile-small-size .
        docker run -p 3001:3000 -d simple-node-app:small-size 
    ```

4. Compare image sizes:
    ```bash
        docker images 
    ```
5. Discuss the use of different ports (3000 and 3001).

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

### Killing and Re-run containers
1. List running containers:
    ```bash
    docker ps
    docker ps -a # list all containers, including stopped ones
    ```
2. Remove a container:
    ```bash
    docker rm -f <container-id>
    ```
3. Run the container again and observe data persistence:
    ```bash
    docker run -p 3001:3000 -d simple-node-app:small-size
    ```


## Docker-Compose & Volumes
1. Examine the `docker-compose.yaml` file:
    ```yaml
    version: '3'
    services:
      app:
        image: simple-node-app:v1
        ports:
          - "3000:3000"
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
2. Build the Docker image using a multi-stage Dockerfile:
    ```bash
    docker build -t java-app:v1 .
    ```
3. Observe the `docker-compose.yml` file:
    ```yaml
    version: "3.9"
    services:
        mysql-server:
            image: mysql:8.2
            ports:
            - "3306:3306"
            volumes:
            - mysql-data:/var/lib/mysql

        petclinic:
            image: java-app:v1
            restart: always
            depends_on:
            - mysql-server
            ports:
            - "9090:8080"   
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




