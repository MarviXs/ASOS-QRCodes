# Stage 1: Build the application
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder
ARG TARGETARCH
ARG TARGETOS

# Install dependencies
RUN apk add --no-cache gcompat

WORKDIR /app

# Copy csproj and restore deps
COPY *.csproj ./
RUN dotnet restore

# Copy rest of the source
COPY . .

# Install dotnet-ef as a LOCAL tool
RUN dotnet new tool-manifest
RUN dotnet tool install dotnet-ef --version 8.*

# Publish + create migration bundle
RUN dotnet publish -a $TARGETARCH -c Release -o out --no-restore && \
    dotnet tool run dotnet-ef migrations bundle \
        --target-runtime $TARGETOS-$TARGETARCH \
        --context AppDbContext \
        --output out/appMigration

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app
COPY --from=builder /app/out .
ENTRYPOINT ["dotnet", "Api.dll"]