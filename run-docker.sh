#!/bin/bash

echo "Starting SQL Server and Redis containers..."
docker-compose up -d sqlserver redis

echo "Waiting for SQL Server to start (30 seconds)..."
sleep 30

echo "Starting development environment..."
# For just running services:
docker-compose up -d cleaningmyname.api

# For development, use the API running locally but databases in Docker:
echo "SQL Server and Redis are now running. You can start your API locally."
echo "Connection strings are:"
echo "SQL Server: Server=localhost,1433;Database=CleaningMyNameDb;User Id=sa;Password=Strong_Password123!;TrustServerCertificate=True;"
echo "Redis: localhost:6379"
