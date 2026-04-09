# Java Example

This sample is a Spring Boot Petclinic application.

## Run Without Docker

### Required Tools
- Java 17
- Maven

Alternative:
- Java 17
- Maven Wrapper (`./mvnw` or `mvnw.cmd`) is already included in this folder, so a separate Maven installation is optional.

### Start Locally
Using Maven Wrapper on macOS/Linux:
```bash
cd Docker/example-projects/java
./mvnw spring-boot:run
```

Using Maven Wrapper on Windows:
```powershell
cd Docker/example-projects/java
.\mvnw.cmd spring-boot:run
```

Open http://localhost:8080

## Notes
- The app uses the H2 in-memory database by default, so it can run locally without MySQL.
- The Docker image switches to the `mysql` Spring profile, but local demo does not need that complexity.
- First startup can take longer because Maven downloads dependencies.

## Useful Commands
Build a jar:
```bash
./mvnw clean package
```

Run the packaged jar:
```bash
java -jar target/spring-petclinic-3.2.0-SNAPSHOT.jar
```
