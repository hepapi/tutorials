# Docker Hands-On Training - Session 2

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
    docker login -u <username>
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
    cd Docker/example-projects/node
    docker build -t <yourusername>/simple-node-app:v1 .
    docker push <yourusername>/simple-node-app:v1

    docker build -t <yourusername>/simple-node-app:small-size -f Dockerfile-small-size .
    docker push <yourusername>/simple-node-app:small-size
    ```
2. Check the new images and tags on your Docker Hub account.

### Killing Containers
1. List running containers:
    ```bash
    docker ps
    docker ps -a # list all containers, including stopped ones
    ```
2. Remove a container:
    ```bash
    docker rm -f <container-id>
    ```

## Docker Compose

### Single Container with Compose
1. Go to the example Node.js project:
    ```bash
    cd Docker/example-projects/node
    ```
2. Create a `docker-compose.yaml` file:
    ```yaml
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
3. Start the service with Docker Compose:
    ```bash
    docker compose up
    docker compose up -d # detach mode
    ```
4. Discuss how Compose helps define and run containerized applications.

### Multiple Containers with Compose
1. Navigate to the Java app `petclinic`:
    ```bash
    cd ../java
    ```
2. Create a `compose.yaml` file:
    ```yaml
    services:
      mysql-server:
        image: mysql:8.2
        environment:
          MYSQL_ROOT_PASSWORD_FILE: /run/secrets/mysql_root_password
          MYSQL_USER: petclinic
          MYSQL_PASSWORD_FILE: /run/secrets/mysql_password
          MYSQL_DATABASE: petclinic
        ports:
          - "3306:3306"
        networks:
          - petnet
        volumes:
          - mysql-data:/var/lib/mysql
        secrets:
          - mysql_root_password
          - mysql_password
        healthcheck:
          test:
            - CMD-SHELL
            - mysqladmin ping -h 127.0.0.1 -uroot -p"$$(cat /run/secrets/mysql_root_password)" --silent
          interval: 5s
          timeout: 3s
          retries: 20
          start_period: 30s

      petclinic:
        build:
          context: .
        image: java-app:v1
        restart: always
        depends_on:
          mysql-server:
            condition: service_healthy
        command: >
          sh -c 'export MYSQL_PASS="$$(cat /run/secrets/mysql_password)" && exec java -jar app.jar'
        ports:
          - "9090:8080"
        networks:
          - petnet
        secrets:
          - mysql_password

    networks:
      petnet:

    volumes:
      mysql-data:

    secrets:
      mysql_root_password:
        file: ./mysql_root_password.txt
      mysql_password:
        file: ./mysql_password.txt
    ```
3. Build the Petclinic image with Compose:
    ```bash
    docker compose build petclinic
    ```
4. Start the services:
    ```bash
    docker compose up -d
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
