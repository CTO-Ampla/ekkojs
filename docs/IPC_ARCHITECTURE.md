# EkkoJS IPC (Inter-Process Communication) Architecture

## Overview

EkkoJS IPC enables bi-directional communication between EkkoJS applications and external processes using a pub/sub messaging system with JSON serialization. This allows EkkoJS to interact with services, databases, APIs, and other applications seamlessly across platforms.

## Architecture Components

### 1. **Transport Layer**
- **Named Pipes** (Windows/Linux/macOS)
- **Unix Domain Sockets** (Linux/macOS)
- **TCP Sockets** (Cross-platform fallback)
- **Message Queues** (Platform-specific optimizations)

### 2. **Message Protocol**
```json
{
  "messageId": "uuid-v4",
  "type": "request|response|event|subscribe|unsubscribe",
  "channel": "channel-name",
  "action": "method-name",
  "data": { ... },
  "timestamp": "ISO-8601",
  "correlationId": "uuid-v4"
}
```

### 3. **Communication Patterns**

#### **Request/Response**
```javascript
// EkkoJS Client
import service from 'ipc:database-service';
const result = await service.getUserById(123);
```

#### **Pub/Sub Events**
```javascript
// EkkoJS Subscriber
import events from 'ipc:notification-service';
events.subscribe('user.created', (data) => {
    console.log('New user:', data);
});

// EkkoJS Publisher
events.publish('order.completed', { orderId: 456, total: 99.99 });
```

### 4. **Service Discovery**
- Registry-based service discovery
- Configuration file mappings
- Dynamic service registration
- Health check mechanisms

## Protocol Specifications

### Message Types

| Type | Description | Direction |
|------|-------------|-----------|
| `request` | Method call request | Client → Service |
| `response` | Method call response | Service → Client |
| `event` | Event notification | Service → Subscribers |
| `subscribe` | Subscribe to channel | Client → Service |
| `unsubscribe` | Unsubscribe from channel | Client → Service |

### Channel Naming Convention
- `service.method` - RPC calls
- `events.category` - Event channels
- `broadcast.global` - Global broadcasts

### Error Handling
```json
{
  "messageId": "uuid",
  "type": "response",
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable message",
    "details": { ... }
  }
}
```

## Service Registration Format

Services expose their capabilities via `.ekko.ipc.json` files:

```json
{
  "service": {
    "name": "database-service",
    "version": "1.0.0",
    "description": "Database access service",
    "transport": {
      "type": "namedpipe",
      "address": "ekko-db-service",
      "port": null
    }
  },
  "methods": {
    "getUserById": {
      "description": "Get user by ID",
      "parameters": [
        { "name": "id", "type": "number", "required": true }
      ],
      "returns": {
        "type": "object",
        "schema": {
          "id": "number",
          "name": "string",
          "email": "string"
        }
      }
    },
    "createUser": {
      "description": "Create new user",
      "parameters": [
        { "name": "userData", "type": "object", "required": true }
      ],
      "returns": { "type": "object" }
    }
  },
  "events": {
    "user.created": {
      "description": "User created event",
      "schema": {
        "id": "number",
        "name": "string",
        "timestamp": "string"
      }
    },
    "user.updated": {
      "description": "User updated event",
      "schema": {
        "id": "number",
        "changes": "object",
        "timestamp": "string"
      }
    }
  },
  "channels": {
    "notifications": {
      "description": "General notifications",
      "type": "broadcast"
    },
    "user-events": {
      "description": "User-related events",
      "type": "topic"
    }
  }
}
```

## Implementation Strategy

### Phase 1: Core Infrastructure
1. Message serialization/deserialization
2. Transport layer abstraction
3. Connection management
4. Basic request/response

### Phase 2: Pub/Sub System
1. Channel management
2. Subscription handling
3. Event broadcasting
4. Message routing

### Phase 3: Service Discovery
1. Service registry
2. Configuration management
3. Health monitoring
4. Auto-reconnection

### Phase 4: Advanced Features
1. Message persistence
2. Load balancing
3. Circuit breakers
4. Monitoring/metrics

## Security Considerations

### Authentication
- Token-based authentication
- Certificate validation
- Process identity verification

### Authorization
- Role-based access control
- Method-level permissions
- Channel access control

### Data Protection
- Message encryption (optional)
- Secure transport channels
- Input validation and sanitization

## Performance Optimizations

### Connection Pooling
- Reuse connections across requests
- Configurable pool sizes
- Connection lifecycle management

### Message Batching
- Batch multiple requests
- Reduce transport overhead
- Configurable batch sizes

### Caching
- Response caching
- Service metadata caching
- Connection state caching

## Cross-Platform Compatibility

### Windows
- Named Pipes (Primary)
- TCP Sockets (Fallback)
- Windows Services integration

### Linux/macOS
- Unix Domain Sockets (Primary)
- Named Pipes via filesystem (Secondary)
- TCP Sockets (Fallback)
- Systemd/Launchd integration

## Monitoring and Debugging

### Logging
- Structured logging (JSON)
- Configurable log levels
- Performance metrics
- Error tracking

### Debugging Tools
- Message tracing
- Connection monitoring
- Performance profiling
- Health dashboards

This architecture provides a robust foundation for inter-process communication while maintaining simplicity and cross-platform compatibility.