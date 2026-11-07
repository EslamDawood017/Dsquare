# Loyalty Points Award API

A thread-safe, idempotent RESTful API for awarding loyalty points to customer wallets, built with **.NET 8.0 LTS** using **Clean Architecture**.

---

## Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Prerequisites & Technology Stack](#prerequisites--technology-stack)
- [Getting Started](#getting-started)
  - [Build](#build)
  - [Run](#run)
  - [Run Tests](#run-tests)
- [API Specification](#api-specification)
  - [POST /awards](#post-awards)
  - [GET /wallets/{walletId}](#get-walletswalletid)
  - [Error Response Format](#error-response-format)
- [Seed Data](#seed-data)
- [Key Engineering Decisions](#key-engineering-decisions)
  - [1. Idempotency & Conflict Detection](#1-idempotency--conflict-detection)
  - [2. Atomicity & Thread Safety](#2-atomicity--thread-safety)
  - [3. Clean Separation of Concerns](#3-clean-separation-of-concerns)
  - [4. Result Pattern over Exceptions](#4-result-pattern-over-exceptions)
  - [5. Validation Strategy](#5-validation-strategy)
  - [6. AutoMapper Configuration](#6-automapper-configuration)
- [Automated Test Suite](#automated-test-suite)
- [Production Considerations](#production-considerations)

---

## Overview

The Loyalty Points Award API enables external source systems (partners, POS, mobile apps) to award loyalty points to customer wallets with the following core guarantees:

- **Idempotency**: Network retries with the same `(sourceId, externalTransactionId)` are safe and will not award points multiple times.
- **Conflict Detection**: Any retry attempting to change transaction parameters (`points` or `walletId`) is rejected with `409 Conflict`.
- **Atomicity**: Wallet balance updates and award transaction records are executed as a single indivisible unit, safe against concurrent race conditions.
- **Strict HTTP Status Semantics**: Correctly distinguishes `201 Created` (new award), `200 OK` (idempotent retry), `400 Bad Request` (invalid input), `404 Not Found` (unknown wallet), and `409 Conflict` (identity mismatch).

---

## Architecture

The solution follows **Clean Architecture** principles, enforcing strict dependency direction inward toward the Domain:

```
[ LoyaltyHub.API ] ──> [ LoyaltyHub.Application ] ──> [ LoyaltyHub.Domain ]
         │                       ▲
         ▼                       │
[ LoyaltyHub.Infrastructure ] ─────┘
```

- **Domain**: Pure business entities (`Wallet`, `Award`) and value objects with no external dependencies or framework attributes.
- **Application**: Business rules, service interfaces (`IAwardService`, `IWalletService`), repository interfaces (`IAwardRepository`, `IWalletRepository`), DTOs, AutoMapper profiles, and the `Result<T>` pattern.
- **Infrastructure**: In-memory data repositories (`WalletRepository`, `AwardRepository`) implementing the application interfaces.
- **API**: ASP.NET Core controllers, route configurations, Swagger/OpenAPI documentation, and centralized model validation filters.
- **Tests**: Comprehensive unit and concurrency test suite covering domain rules, repositories, services, and multi-threaded race conditions.

---

## Project Structure

```
c:\Users\Admin\Desktop\Dsquare\
├── LoyaltyHub.sln
├── README.md
├── Directory.Build.props
│
├── LoyaltyHub.Domain/
│   ├── Entities/
│   │   ├── Award.cs
│   │   └── Wallet.cs
│   └── ValueObjects/
│       └── AwardIdentity.cs
│
├── LoyaltyHub.Application/
│   ├── Common/
│   │   ├── ApplicationError.cs
│   │   ├── ErrorType.cs
│   │   └── Result.cs
│   ├── DTOs/
│   │   ├── AwardRequestDto.cs
│   │   ├── AwardResponseDto.cs
│   │   └── WalletResponseDto.cs
│   ├── Errors/
│   │   └── ApplicationErrors.cs
│   ├── Interfaces/
│   │   ├── Repositories/
│   │   │   ├── IAwardRepository.cs
│   │   │   └── IWalletRepository.cs
│   │   └── Services/
│   │       ├── IAwardService.cs
│   │       └── IWalletService.cs
│   ├── Mappings/
│   │   └── MappingProfile.cs
│   └── Services/
│       ├── AwardService.cs
│       └── WalletService.cs
│
├── LoyaltyHub.Infrastructure/
│   └── Repositories/
│       ├── AwardRepository.cs
│       └── WalletRepository.cs
│
├── LoyaltyHub.API/
│   ├── Controllers/
│   │   ├── AwardsController.cs
│   │   └── WalletsController.cs
│   ├── Program.cs
│   └── appsettings.json
│
└── LoyaltyHub.Tests/
    ├── Application/
    │   ├── AwardServiceTests.cs
    │   └── WalletServiceTests.cs
    ├── Domain/
    │   ├── AwardIdentityTests.cs
    │   ├── AwardTests.cs
    │   └── WalletTests.cs
    ├── Infrastructure/
    │   ├── AwardRepositoryTests.cs
    │   └── WalletRepositoryTests.cs
    └── TestConfiguration.cs
```

---

## Prerequisites & Technology Stack

- **.NET 8.0 SDK** (LTS)
- **C# 12**
- **AutoMapper 16.3+** for object-to-object mapping
- **xUnit 2.5+** & **Fluent Assertions** for automated testing
- **Swashbuckle / Swagger** for interactive API documentation

---

## Getting Started

### Build
Build the entire solution from the root folder:

```bash
dotnet build LoyaltyHub.sln
```

### Run
Launch the Web API:

```bash
dotnet run --project LoyaltyHub.API\LoyaltyHub.API.csproj
```

The API will start listening at:
- **HTTP**: `http://localhost:5143`
- **Swagger UI**: `http://localhost:5143/swagger`

### Run Tests
Execute the automated test suite:

```bash
dotnet test LoyaltyHub.Tests\LoyaltyHub.Tests.csproj
```

### Postman Collection
A ready-to-import Postman Collection is included at the solution root:
- **File**: `LoyaltyHub.postman_collection.json`
- Contains all 12 pre-configured scenarios (initial balance, first-time award, idempotent retries, conflicts, unknown wallets, and validation errors).
- All requests include embedded `pm.test(...)` assertions for automated testing using the **Postman Collection Runner**.
- The `{{baseUrl}}` collection variable is pre-set to `http://localhost:5143`.

---

## API Specification

### POST /awards
Processes a loyalty points award request for a specific wallet.

#### Request Headers
- `Content-Type: application/json`

#### Request Body
```json
{
  "sourceId": "pos_terminal_01",
  "externalTransactionId": "tx_20261107_9876",
  "walletId": "W100",
  "points": 50
}
```

| Field | Type | Rules |
|---|---|---|
| `sourceId` | `string` | Required, non-empty |
| `externalTransactionId` | `string` | Required, non-empty |
| `walletId` | `string` | Required, non-empty |
| `points` | `int` | Required, integer between `1` and `1000` |

#### Responses

- **`201 Created`** — First-time award processed successfully. Returns the created award record and includes the `Location` header.
  ```json
  {
    "awardId": "8f2a1b3c4d5e46b7a8c9d0e1f2a3b4c5",
    "sourceId": "pos_terminal_01",
    "externalTransactionId": "tx_20261107_9876",
    "walletId": "W100",
    "points": 50
  }
  ```

- **`200 OK`** — Idempotent retry. The request matches an already-processed award with identical parameters. Returns the original award without duplicating points.
  ```json
  {
    "awardId": "8f2a1b3c4d5e46b7a8c9d0e1f2a3b4c5",
    "sourceId": "pos_terminal_01",
    "externalTransactionId": "tx_20261107_9876",
    "walletId": "W100",
    "points": 50
  }
  ```

- **`400 Bad Request`** — Structural validation failure (e.g. missing required field, negative points, or malformed JSON).
  ```json
  {
    "code": "INVALID_REQUEST",
    "message": "The request body is malformed or contains invalid fields.",
    "details": [
      "points must be an integer between 1 and 1000."
    ]
  }
  ```

- **`404 Not Found`** — Target wallet does not exist. (Note: this failure does not consume the transaction identity, allowing safe subsequent retry).
  ```json
  {
    "code": "WALLET_NOT_FOUND",
    "message": "Wallet 'W999' was not found.",
    "details": null
  }
  ```

- **`409 Conflict`** — Conflicting retry. An award with the same `(sourceId, externalTransactionId)` already exists, but with different `points` or `walletId`.
  ```json
  {
    "code": "AWARD_CONFLICT",
    "message": "An award with the specified identity already exists with different parameters.",
    "details": null
  }
  ```

---

### GET /wallets/{walletId}
Retrieves current wallet details and point balance.

#### Request
```http
GET /wallets/W100 HTTP/1.1
```

#### Responses

- **`200 OK`** — Wallet found.
  ```json
  {
    "walletId": "W100",
    "balance": 150
  }
  ```

- **`404 Not Found`** — Wallet does not exist.
  ```json
  {
    "code": "WALLET_NOT_FOUND",
    "message": "Wallet 'W999' was not found.",
    "details": null
  }
  ```

- **`400 Bad Request`** — Empty or whitespace `walletId`.

---

### Error Response Format
All failure responses across both endpoints follow a uniform, standardized JSON contract:

```json
{
  "code": "ERROR_CODE",
  "message": "Human-readable summary of the error.",
  "details": ["Optional list of granular validation messages"]
}
```

---

## Seed Data

On application startup, `WalletRepository` initializes two in-memory customer wallets:

| Wallet ID | Initial Balance | Purpose |
|---|---|---|
| `W100` | `100` points | Standard test wallet for successful awards and retries |
| `W200` | `50` points | Secondary test wallet for conflict verification |

---

## Key Engineering Decisions

### 1. Idempotency & Conflict Detection
- **Identity Definition**: A transaction is uniquely identified by the composite key `(sourceId, externalTransactionId)`.
- **Matching Retry (`200 OK`)**: If an award with the same identity exists and both `walletId` and `points` match exactly, the system returns the existing award without altering balances.
- **Conflicting Retry (`409 Conflict`)**: If the identity matches but `points` or `walletId` differ, it is rejected with a conflict error.
- **Unconsumed Identity on Failure**: If a request fails because the wallet does not exist (`404`), the identity is **not** persisted. The client can safely retry the same transaction against a valid wallet.

### 2. Atomicity & Thread Safety
- **Single Lock in Service**: The critical atomic boundary is the composite transaction:
  1. Check existing award $\to$ 2. Look up wallet $\to$ 3. Update wallet balance $\to$ 4. Store award record.
- Locking individual repository methods does not guarantee atomicity across both tables. Therefore, synchronization is held by `AwardService` via a single `private static readonly object _lock`.
- **Pure Synchronous Repositories**: Because all data is in-memory and executes under `lock (_lock)` (where C# forbids `await`), repositories are purely synchronous. This eliminates fake `Task.FromResult(...)` wrappers and removes double-locking overhead.

### 3. Clean Separation of Concerns
- **Domain Entities**: Contain purely data properties and zero business logic or validation.
- **Repositories**: Pure data access against internal collections. No validation or business rules exist in repository classes.
- **Services**: Contain all domain orchestrations, business rules, idempotency decisions, and coordination.
- **Controllers**: Thin presentation adapters that translate HTTP requests to service calls and map `Result<T>` to HTTP status codes.

### 4. Result Pattern over Exceptions
- The application uses `Result<T>` and `ApplicationError` for expected business outcomes (not found, conflict, validation).
- Exceptions are reserved strictly for unexpected runtime failures (out of memory, critical bugs).
- This avoids expensive exception stack-trace overhead in high-throughput payment and award flows.

### 5. Validation Strategy
- **DTO Layer**: Structural constraints (`[Required(AllowEmptyStrings = false)]`, `[Range(1, 1000)]`) are enforced on `AwardRequestDto`.
- **Centralized ModelState Handling**: Configured via `InvalidModelStateResponseFactory` in `Program.cs` so model binding errors format into the unified `{Code, Message, Details}` JSON shape instead of ASP.NET's default `ValidationProblemDetails`.
- **Service Layer**: Handles business validations (e.g. wallet presence, identity conflicts).

### 6. AutoMapper Configuration
- `MappingProfile` centralizes mapping rules between domain entities and DTOs:
  - Generates unique GUIDs (`AwardId`) and UTC timestamps on entity creation.
  - Automatically flags `IsNewAward = true` for created awards, which the controller uses to issue `201 Created` versus `200 OK`.
  - The `[JsonIgnore]` attribute on `AwardResponseDto.IsNewAward` prevents internal routing flags from leaking into the public API contract.

---

## Automated Test Suite

The test project (`LoyaltyHub.Tests`) contains **49 comprehensive unit and concurrency tests**:

1. **Domain Tests**: Verifies entity property mutability and record equality semantics (`AwardIdentity`).
2. **Repository Tests**: Verifies seed initialization, CRUD operations, and defensive copy immutability.
3. **Application & Validation Tests**: Validates null guards, empty strings, point boundary limits (0, negative, >1000), and wallet queries.
4. **Idempotency & Conflict Tests**: Verifies matching retry behavior, points mismatch conflicts, and wallet mismatch conflicts.
5. **Multi-threaded Concurrency Tests**:
   - `ConcurrentMatchingRequests`: 20 concurrent threads submitting the exact same award request yield **exactly 1 creation (201)** and **19 matching retries (200)**, with the wallet credited only once.
   - `ConcurrentDistinctAwards`: 10 concurrent threads awarding points to the same wallet accumulate accurately without lost updates.
   - `ConcurrentRace_SameIdentityDifferentPoints`: 2 racing threads with the same identity but different points yield **exactly 1 success** and **1 conflict (409)**.

---

## Production Considerations

For deployment in a distributed, multi-instance production environment:

1. **Distributed Locking / Database Transactions**: Replace in-memory `lock` with PostgreSQL/SQL Server `SERIALIZABLE` transactions or Redis distributed locks (Redlock).
2. **Database Unique Constraints**: Add a unique composite index on `(source_id, external_transaction_id)` at the database level to ensure idempotency across multiple backend pods.
3. **Outbox Pattern**: If emitting events (e.g., Kafka / RabbitMQ) when points are awarded, persist outbox events within the same atomic database transaction.

