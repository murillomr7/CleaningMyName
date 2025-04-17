# CleaningMyName API
=====================

The CleaningMyName Project is a financial recovery application written in .NET 7

The application provides a simple implementation for people that's need to pay the bills. 
Also, this project implements the most common used technologies looking for the best way to develop great applications with .NET


A RESTful API built with .NET 7 following best practices.

## Features

- Clean Architecture design
- CQRS pattern with MediatR
- JWT authentication and authorization
- Role-based access control
- SQL Server database with Entity Framework Core
- Comprehensive health checks
- Swagger API documentation
- Unit and integration testing
- Docker support

## Getting Started

### Prerequisites

- .NET 7 SDK
- Docker and Docker Compose (for containerized deployment)
- SQL Server (or use the provided Docker Compose for a containerized version)

### Installation

1. Clone the repository:

```bash
git clone https://github.com/yourusername/CleaningMyName.git
cd CleaningMyName


CleaningMyName/
├── src/
│   ├── CleaningMyName.Domain/          # Core entities and business rules 
│   ├── CleaningMyName.Application/     # Business logic and use cases 
│   ├── CleaningMyName.Infrastructure/  # External concerns like DB, identity 
│   └── CleaningMyName.Api/             # API controllers and presentation 
├── Dockerfile                           # Docker build instructions 
├── docker-compose.yml                   # Docker compose configuration
├── .dockerignore                        # Files to ignore in Docker context
│
├── test/
│   ├── CleaningMyName.UnitTests/       # Unit tests 
│   └── CleaningMyName.IntegrationTests/ # Integration tests 
└── CleaningMyName.sln                   # Solution file