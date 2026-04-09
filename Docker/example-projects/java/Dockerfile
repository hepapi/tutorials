# syntax=docker/dockerfile:1.7

FROM maven:3.9.5-amazoncorretto-17 AS builder
WORKDIR /app

COPY pom.xml ./
RUN --mount=type=cache,target=/root/.m2 \
    mvn -B -DskipTests dependency:go-offline

COPY src ./src
RUN --mount=type=cache,target=/root/.m2 \
    mvn -B -DskipTests package

FROM amazoncorretto:17-alpine3.18
WORKDIR /app
COPY --from=builder /app/target/spring-petclinic-3.2.0-SNAPSHOT.jar app.jar
ENV SPRING_PROFILES_ACTIVE mysql
EXPOSE 8080
CMD ["java", "-jar", "app.jar"]
