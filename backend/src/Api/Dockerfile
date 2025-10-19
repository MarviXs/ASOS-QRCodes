# Stage 1: Build the application
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS builder
ARG TARGETARCH
ARG TARGETOS

# Install dotnet-ef tool
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Install dependencies
RUN apk add --no-cache libc6-compat

# Set the working directory
WORKDIR /app

# Copy csproj and restore dependencies as a distinct layer
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . .

# Publish the application and create migration bundles
RUN dotnet publish -a $TARGETARCH -c Release -o out --no-restore \
    && dotnet ef migrations bundle --target-runtime $TARGETOS-$TARGETARCH --context AppDbContext --output out/appMigration

# Stage 2: Production stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production

# Set the working directory
WORKDIR /app

# Copy the built application from the builder stage
COPY --from=builder /app/out .

# Set the entry point
ENTRYPOINT ["dotnet", "Api.dll"]
