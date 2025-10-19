# Project Setup

This guide will walk you through setting up the development environment for the project.

## Prerequisites

- **Docker** installed on your system
- **Node.js** and **npm** installed (for the frontend)
- **.NET SDK 8.0** installed (for the backend)
- **EF Core Global Tool** installed (for the backend)

---

## 1. Docker Setup

Docker is used to manage services and databases for the project.

### Steps:
1. Navigate to the Docker folder:
   ```bash
   cd ./docker
   ```
2. Copy the `.env.template` file to `.env`:
   ```bash
   cp .env.template .env
   ```
3. Start the Docker containers:
   ```bash
   docker compose up -d
   ```

---

## 2. Frontend Setup

The frontend is built using Quasar Framework (Vue 3). Follow these steps to set it up:

### Steps:
1. Navigate to the frontend folder:
   ```bash
   cd ./frontend
   ```
2. Install the required npm packages:
   ```bash
   npm install --legacy-peer-deps
   ```
3. Start the frontend development server:
   ```bash
   npm run dev
   ```

---

## 3. Backend Setup

The backend is built using .NET Core 8.0 and uses Entity Framework (EF) Core for database management.

### Steps:
1. Install **.NET SDK 8.0** if it's not already installed.
2. Install **Entity Framework Core Global Tool**:
   ```bash
   dotnet tool install --global dotnet-ef
   ```
3. Navigate to the backend API folder:
   ```bash
   cd ./backend/src/Api
   ```
4. Update the database schema for the DB:
   ```bash
   dotnet ef database update --context AppDbContext
   dotnet ef database update --context TimeScaleDbContext
   ```
5. Run the backend API in watch mode:
   ```bash
   dotnet watch
   ```
---
You are now ready to develop and run the project.
