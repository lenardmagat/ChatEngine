# ChatEngine

A real-time chat backend built with ASP.NET Core and SignalR, designed as the messaging layer for a peer-to-peer bartering platform. On top of standard direct messaging and conversation history, chat rooms carry structured **trade offers** — proposing, countering, accepting, or declining an item-for-item trade — as first-class message types alongside text and images.

## Features

- **Real-time messaging over SignalR** — a single authenticated hub (`/ChatHub`) handles connect/disconnect, direct messaging, chat initialization, and conversation loading.
- **CQRS via MediatR** — each hub action dispatches a command (`SendMessageCommand`, `InitializeChatCommand`, `LoadConversationCommand`, etc.) to a dedicated handler that talks to the database.
- **Trade offers embedded in chat** — a `TradeOffer` model supports proposing, countering (with parent/child offer chaining), accepting, declining, cancelling, and completing trades, surfaced as special `ChatMessage` types (`OfferProposed`, `OfferCountered`, `OfferAccepted`, etc.).
- **1:1 and group chat rooms** — `ChatRoom` supports both private DMs and named group chats, with per-participant state (mute, last-read timestamp) via `RoomParticipant`.
- **Soft deletes and message edit history** — both chat rooms and individual messages support soft deletion and edited/deleted timestamps rather than hard removal.
- **JWT auth over SignalR** — the hub accepts a JWT passed as an `access_token` query parameter (SignalR's standard workaround, since WebSocket clients can't set an `Authorization` header) alongside normal bearer-token auth for the REST endpoints.
- **Hashid-obfuscated IDs** — user, room, and message IDs are exposed to clients as context-scoped Hashids (`HashContext.User`, `HashContext.Room`, etc.) rather than raw database integers.
- **Auto-join on connect** — users are automatically subscribed to their personal notification group (`UsersNotification_{id}`) when they connect, so they receive new-message pushes even outside an open chat room.
- **Structured error handling** — a global exception filter plus a consistent `Result`-style response wrapper across both REST and SignalR error paths.
- **Dockerized** — ships with a `Dockerfile` and `docker-compose.yml` to run the API and a Postgres 16 database together.

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core (.NET 10) |
| Real-time transport | SignalR (WebSockets) |
| Database | PostgreSQL via Npgsql + EF Core |
| CQRS / mediator | MediatR |
| Auth | JWT Bearer tokens, BCrypt.Net for password hashing |
| ID obfuscation | Hashids.net |
| Logging | Serilog (console + rolling file sinks) |
| Containerization | Docker / Docker Compose |

## Project Structure

```
ChatEngine/
├── DTOs/                       # Request/response contracts
├── Hub/ChatHub/                # SignalR hub (AppHub) — connect, disconnect, messaging
├── Infastructure/
│   ├── DataBase/                # EF Core DbContext + design-time factory
│   ├── Middleware/               # App configuration, DI wiring, exception handling
│   └── Security/                 # JWT/hashing/Hashids implementation, error handling
├── Migrations/                  # EF Core migrations
├── Models/                      # EF Core entities (User, ChatRoom, ChatMessage, RoomParticipant, TradeOffer)
├── Routers/
│   ├── AccountRouters.cs/        # REST: register, login
│   └── ForTestingRouters.cs/     # REST: debug/testing endpoints
├── Services/
│   ├── EventHandlers/            # MediatR command handlers (send message, init chat, load conversation, auto-join)
│   ├── AccountServices.cs
│   └── SystemEvents.cs           # MediatR command definitions
├── Dockerfile
├── docker-compose.yml
└── Program.cs                    # App startup/bootstrap
```

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (for local, non-Docker runs)
- Docker & Docker Compose (recommended)

### Configuration

The app reads configuration from environment variables (or a `.env` file via DotNetEnv). Create a `.env` file in the project root matching the variables referenced in `docker-compose.yml`:

```env
JWT_SECRET_KEY=your-secret-signing-key
JWT_ISSUER=ChatEngine
JWT_AUDIENCE=ChatEngineUsers
HASHID_SALT=your-hashids-salt
Hashids__MinHashLength=8
```

> The default Postgres credentials in `docker-compose.yml` (`lenard_admin` / `super_secure_password123`) are placeholders for local development only — replace them before deploying anywhere public.

### Run with Docker (recommended)

```bash
git clone https://github.com/lenardmagat/ChatEngine.git
cd ChatEngine
docker compose up --build
```

This starts a Postgres 16 container and the API together, mapped to `localhost:5000`.

### Run locally without Docker

```bash
dotnet restore
dotnet run
```

Pending EF Core migrations are applied automatically on startup.

## API Overview

### REST

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/API/Account/Create` | Register a new account |
| `POST` | `/API/Account/Login` | Log in and receive a JWT |
| `GET` | `/API/Account/get/{AccountId}` | Fetch an account's hashed ID (testing/debug) |

### SignalR Hub — `/ChatHub`

| Method | Direction | Description |
|---|---|---|
| `OnConnectedAsync` | server event | Auto-joins the user's personal notification group |
| `InitializeChat` | client → server | Opens or creates a room with another user, joins the room group |
| `DirectMessage` | client → server | Sends a message to a room or user, broadcasts to room members and the recipient's notification group |
| `LoadChats` | client → server | Loads the caller's recent conversations |
| `ChatDisconnect` | client → server | Leaves a specific chat room group |

Connect with a JWT passed as an `access_token` query parameter, e.g. `wss://.../ChatHub?access_token=<jwt>`.

## Status

This is an active work in progress and the messaging layer for a larger peer-to-peer bartering platform — expect incomplete validation, missing tests, and evolving conventions as features are added. Trade-offer negotiation logic (accept/counter/decline handlers) is scaffolded in the data model but not yet fully wired into the hub.

## License

No license specified yet — all rights reserved by default until one is added.
