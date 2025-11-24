# PolicyActionService

A RESTful API service built with .NET Core 9.0 for managing policies and their associated actions.

## Overview

PolicyActionService provides a comprehensive API for:
- Creating, reading, updating, and deleting policies
- Executing actions associated with policies
- Tracking action execution status and results
- Health monitoring

## Technology Stack

- **Framework**: .NET 9.0
- **API Type**: RESTful Web API
- **Architecture**: Clean Architecture with Controllers, Services, and Models
- **Language**: C# 12

## Project Structure

```
PolicyActionService/
├── Controllers/
│   ├── PoliciesController.cs    # Policy management endpoints
│   ├── ActionsController.cs     # Action execution endpoints
│   └── HealthController.cs      # Health check endpoint
├── Services/
│   ├── IPolicyService.cs        # Policy service interface
│   ├── PolicyService.cs         # Policy service implementation
│   ├── IActionService.cs        # Action service interface
│   └── ActionService.cs         # Action service implementation
├── Models/
│   ├── Policy.cs                # Policy entity model
│   └── PolicyAction.cs          # Action entity model
└── Program.cs                   # Application entry point
```

## API Endpoints

### Policies

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/policies | Get all policies |
| GET | /api/policies/{id} | Get policy by ID |
| POST | /api/policies | Create new policy |
| PUT | /api/policies/{id} | Update existing policy |
| DELETE | /api/policies/{id} | Delete policy |

### Actions

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/actions | Get all actions |
| GET | /api/actions/{id} | Get action by ID |
| GET | /api/actions/policy/{policyId} | Get actions for a policy |
| POST | /api/actions/execute | Execute a new action |
| PATCH | /api/actions/{id}/status | Update action status |

### Health

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/health | Check service health |

## Running Locally

### Prerequisites
- .NET 9.0 SDK

### Build and Run

```bash
cd src/PolicyActionService
dotnet restore
dotnet build
dotnet run
```

The service will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- OpenAPI: http://localhost:5000/openapi/v1.json (in Development mode)

## Running with Docker

### Build Docker Image

```bash
docker build -f Dockerfile.policyactionservice -t policy-action-service .
```

### Run with Docker Compose

```bash
docker-compose up -d
```

The service will be available at:
- HTTP: http://localhost:5000
- HTTPS: http://localhost:5001

### Stop Service

```bash
docker-compose down
```

## Example Usage

### Create a Policy

```bash
curl -X POST http://localhost:5000/api/policies \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Backup Policy",
    "description": "Daily backup policy",
    "isActive": true
  }'
```

### Execute an Action

```bash
curl -X POST http://localhost:5000/api/actions/execute \
  -H "Content-Type: application/json" \
  -d '{
    "policyId": 1,
    "actionType": "backup",
    "actionData": "{ \"target\": \"/data\" }"
  }'
```

### Health Check

```bash
curl http://localhost:5000/api/health
```

## Configuration

Configuration is managed through `appsettings.json` and `appsettings.Development.json`.

Key settings:
- **Logging**: Configure log levels for different components
- **Kestrel Endpoints**: HTTP/HTTPS URLs and ports
- **CORS**: Cross-Origin Resource Sharing settings

## Development

The service uses:
- In-memory storage (suitable for development/testing)
- Singleton services for data persistence during runtime
- Comprehensive logging
- OpenAPI/Swagger documentation (in Development mode)

## Future Enhancements

- Database integration (SQL Server, PostgreSQL, etc.)
- Authentication and authorization
- Action scheduling and queuing
- Event-driven architecture with message queues
- Metrics and monitoring integration
- Policy versioning
