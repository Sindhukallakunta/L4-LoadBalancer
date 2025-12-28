# L4 Load Balancer (TCP)

## Overview

This project implements a basic Layer 4 Load Balancer written in C#.  
It accepts incoming TCP connections from clients and distributes traffic across multiple backend services.

The design focuses on correctness, thread safety, extensibility, and testability, while keeping the implementation simple and easy to reason about.

---

## Key Features

- TCP (Layer 4) load balancing
- Pluggable load balancing strategies (Strategy Pattern)
- TCP-based backend health checks
- Thread-safe backend pool management
- Graceful shutdown using cancellation tokens

---

## Architecture Overview

The system is composed of the following key components:

### Core Components

- TcpLoadBalancer
  Listens for incoming TCP connections and forwards traffic to backend servers.

- ILoadBalancingStrategy 
  Abstraction for backend selection logic.

- RoundRobinStrategy
  Sequentially selects backends in a thread-safe manner.

### Health Checking


- Backend 
  Represents a backend server and tracks health state and active connections.

- BackendPool
  Maintains a thread-safe collection of backends and exposes healthy backends.

- HealthCheckService
  Periodically checks backend availability and updates health state.

- IBackendHealthProbe
  Abstraction for backend health probing.

- TcpBackendHealthProbe 
  TCP-based implementation that determines availability by attempting a TCP connection.

---

## Design Decisions


### Why Strategy Pattern?

Load balancing algorithms are encapsulated behind an interface, allowing strategies to be changed or extended without modifying the load balancer.

### Why Health Probe Abstraction?

Health probing involves network I/O and timeouts, which are difficult to unit test.  
By abstracting the probe, core logic remains testable while infrastructure logic stays isolated.

### Thread Safety

- Backend selection uses atomic operations
- Backend pool access is synchronized
- Health state updates are encapsulated inside backend logic

---

## Running the Project

1. Clone the repository
2. Open the solution in Visual Studio
3. Configure backend servers (e.g., localhost ports)
4. Run the application
5. Clients can connect to the configured listening port

---

## Testing Strategy

- **Unit tested**
  - Backend
  - BackendPool
  - Load balancing strategies
  - HealthCheckService (using mocked probes)


Infrastructure components involving sockets and infinite loops are validated through manual testing rather than unit tests.


---

## Graceful Shutdown

The application supports graceful shutdown using CancellationToken, ensuring:
- Health checks stop cleanly
- Active connections are allowed to complete
- Resources are released correctly

---

