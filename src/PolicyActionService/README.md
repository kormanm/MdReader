# PolicyActionService

RESTful API service for managing policies and their associated actions, built with .NET Core 9.0.

## Quick Start

### Run Locally
```bash
cd src/PolicyActionService
dotnet run
```

Service will be available at http://localhost:5100

### Run with Docker
```bash
# Build and run
docker-compose up -d

# Stop
docker-compose down
```

## Documentation

See [docs/POLICY_ACTION_SERVICE.md](../../docs/POLICY_ACTION_SERVICE.md) for complete documentation.

## API Endpoints

- `GET /api/health` - Health check
- `GET /api/policies` - List all policies
- `POST /api/policies` - Create policy
- `PUT /api/policies/{id}` - Update policy
- `DELETE /api/policies/{id}` - Delete policy
- `POST /api/actions/execute` - Execute action
- `GET /api/actions` - List all actions

## Testing

```bash
# Health check
curl http://localhost:5100/api/health

# Create a policy
curl -X POST http://localhost:5100/api/policies \
  -H "Content-Type: application/json" \
  -d '{"name": "Test Policy", "description": "Test", "isActive": true}'
```
