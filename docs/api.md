# API Documentation

The BingoGame API is a RESTful Web API built with .NET 8. Interactive documentation is available via Swagger at `/swagger` when the backend is running (e.g., `https://localhost:5001/swagger`).

All endpoints require authentication via Bearer JWT token in the `Authorization` header, except for public ones like guest login. Guest tokens are short-lived (e.g., 15 minutes). Registered users have longer-lived tokens.

Base URL: `https://localhost:5001/api` (adjust port as per setup).

## Authentication (AuthController)

Authentication handles user registration, login, and guest access. Uses JWT for stateless auth.

### POST /auth/register
- **Description**: Registers a new user.
- **Request Body**: [RegisterDto](https://example.com/RegisterDto) (Username, Email, Password)
- **Response**: 201 Created - [UserDto](https://example.com/UserDto) (Id, Username, Email, Role)
- **Auth**: None (public)
- **Notes**: Password hashed with BCrypt. Validates uniqueness.

### POST /auth/login
- **Description**: Logs in a user and returns JWT token.
- **Request Body**: [LoginDto](https://example.com/LoginDto) (UsernameOrEmail, Password)
- **Response**: 200 OK - [TokenDto](https://example.com/TokenDto) (Token, ExpiresIn, User: UserDto)
- **Auth**: None (public)
- **Notes**: Validates credentials. Token includes user claims (Id, Role).

### POST /auth/guest
- **Description**: Generates a short-lived guest token for anonymous play.
- **Request Body**: None
- **Response**: 200 OK - [TokenDto](https://example.com/TokenDto) (Token with guest user info)
- **Auth**: None (public)
- **Notes**: Guest users have limited privileges (e.g., join but not create rooms). Token expires quickly.

**DTOs**:
- RegisterDto: Username (string, required), Email (string, required, valid email), Password (string, min 6 chars).
- LoginDto: UsernameOrEmail (string), Password (string).
- UserDto: Id (Guid), Username (string), Email (string), Role (UserRole: Admin/User/Guest).
- TokenDto: Token (string), ExpiresIn (int, seconds), User (UserDto).

## Rooms (RoomsController)

Manages bingo rooms for multiplayer games. Requires auth.

### POST /rooms
- **Description**: Creates a new bingo room.
- **Request Body**: [RoomCreateDto](https://example.com/RoomCreateDto) (Name, MaxPlayers, BingoType)
- **Response**: 201 Created - [RoomDto](https://example.com/RoomDto) (Id, Name, HostId, MaxPlayers, CurrentPlayers, Status, BingoType)
- **Auth**: Bearer token (User role)
- **Notes**: Only authenticated users can create. Sets host as creator.

### GET /rooms
- **Description**: Lists all active rooms.
- **Response**: 200 OK - [RoomDto[]](https://example.com/RoomDto)
- **Auth**: None (public, but filtered for active)
- **Notes**: Returns rooms with Status != Ended.

### POST /rooms/join
- **Description**: Joins a room with a room code or ID.
- **Request Body**: [JoinRoomDto](https://example.com/JoinRoomDto) (RoomId or RoomCode, PlayerName)
- **Response**: 200 OK - [RoomDto](https://example.com/RoomDto) (updated with joined player)
- **Auth**: Bearer token (optional for guests)
- **Notes**: Generates bingo card for player. Max players enforced.

**DTOs**:
- RoomCreateDto: Name (string, required), MaxPlayers (int, 2-50), BingoType (BingoType: Traditional).
- RoomDto: Id (Guid), Name (string), HostId (Guid), HostUsername (string), MaxPlayers (int), CurrentPlayers (int), Status (GameStatus: Waiting/Started/Paused/Ended), BingoType (enum), CreatedAt (DateTime), Players (UserDto[] simplified).
- JoinRoomDto: RoomId (Guid, optional), RoomCode (string, optional), PlayerName (string, for guests).

## Game (GameController)

Manages game lifecycle within a room. Requires room membership and host privileges for actions like start/draw.

### POST /game/{roomId}/start
- **Description**: Starts the game in a room (host only).
- **Response**: 200 OK - { SessionId: Guid }
- **Auth**: Bearer token (host role in room)
- **Notes**: Validates min players. Initializes game session, generates cards if not done.

### POST /game/{roomId}/draw
- **Description**: Draws the next ball/number (host only).
- **Request Body**: Empty or { ManualBall: int } for manual draw.
- **Response**: 200 OK - { Ball: int, DrawnBalls: int[] }
- **Auth**: Bearer token (host, game started)
- **Notes**: Random draw from 1-75 (Traditional). Broadcasts via SignalR.

### GET /game/{roomId}/drawn
- **Description**: Gets list of drawn balls.
- **Response**: 200 OK - [DrawnBallsDto](https://example.com/DrawnBallsDto) { Drawn: int[], LastDrawn: int, TotalDrawn: int }
- **Auth**: Bearer token (room member)
- **Notes**: For client sync.

### POST /game/{roomId}/pause
- **Description**: Pauses the game (host only).
- **Response**: 200 OK - { Status: Paused }
- **Auth**: Bearer token (host)

### POST /game/{roomId}/end
- **Description**: Ends the game (host only).
- **Response**: 200 OK - { Message: "Game ended" }
- **Auth**: Bearer token (host)
- **Notes**: Calculates winners, updates scores if tracked.

**DTOs**:
- DrawnBallsDto: Drawn (int[]), LastDrawn (int?), TotalDrawn (int).

## SignalR Hub: /gamehub

Real-time communication for game events, chat, and updates. Connect with JWT token (query param `access_token` for WebSocket).

### Methods (Client to Server)

- **JoinRoom(roomId: Guid, playerName: string)**: Joins room, receives initial state (cards, drawn balls). Invokes `RoomJoined(roomDto: RoomDto, card: BingoCardDto)`.
- **SendChat(message: string)**: Sends chat message to room. Invokes `ReceiveChat(chatMessage: ChatMessageDto)` on all clients.
- **MarkNumber(roomId: Guid, number: int)**: Marks number on player's card (validation on server). Invokes `NumberMarked(playerId: Guid, number: int)` and checks for bingo: `BingoAchieved(playerId: Guid)`.

### Server to Client Events

- **RoomJoined(room: RoomDto, card: BingoCardDto)**: Sent on join.
- **ReceiveChat(message: ChatMessageDto)**: Chat messages.
- **NumberDrawn(number: int)**: New ball drawn.
- **GameStarted(sessionId: Guid)**: Game begins.
- **GamePaused() / GameResumed() / GameEnded(winners: Guid[])**: Status changes.
- **PlayerJoined(player: UserDto)** / **PlayerLeft(playerId: Guid)**: Room updates.
- **BingoAchieved(playerId: Guid, card: BingoCardDto)**: Winner notification.

**DTOs**:
- ChatMessageDto: Id (Guid), RoomId (Guid), UserId (Guid), Username (string), Message (string), Timestamp (DateTime).
- BingoCardDto: Id (Guid), PlayerId (Guid), Numbers (int[5,5]), Marked (bool[5,5]).

## Security & Best Practices

- **Auth**: All protected endpoints use [Authorize] attribute. Guest tokens for anonymous.
- **Validation**: FluentValidation for DTOs (e.g., password strength, email format).
- **Error Handling**: Global exception middleware returns ProblemDetails (RFC 7807).
- **Rate Limiting**: Not implemented; add AspNetCore.RateLimiting for production.
- **HTTPS**: Enforced in production.

For setup and deployment, refer to [setup.md](../setup.md) and [deployment.md](../deployment.md).