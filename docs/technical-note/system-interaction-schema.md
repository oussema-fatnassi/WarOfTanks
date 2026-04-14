# War of Tanks — System Interaction Schema

This document describes how all systems communicate at runtime.

---

## Architecture Diagram

```mermaid
flowchart TD
    subgraph Browser["Browser (Client)"]
        React["React App\n(Vite + TypeScript)"]
        Unity["Unity WebGL Build\n(embedded on /play)"]
    end

    subgraph Render_Backend["Render — Backend Service"]
        API["Go + Gin\nREST API\n:8080"]
    end

    subgraph Render_DB["Database"]
        MongoDB["MongoDB\n(Atlas or Docker)"]
    end

    User(["User"]) -->|"HTTPS"| React

    React -->|"JSLib bridge\n(passes JWT on game start)"| Unity
    React -->|"HTTPS + Axios\nAuthorization: Bearer token"| API
    Unity -->|"HTTPS + UnityWebRequest\nAuthorization: Bearer token"| API
    API -->|"MongoDB Go Driver"| MongoDB
```

---

## Interactions Explained

### 1. User → React App
The user accesses the web interface via a browser over HTTPS. The React app is served as a static build (HTML, JS, CSS) from Render.

### 2. React App → Unity WebGL
The Unity game is not a separate deployment. It is loaded as a static WebGL build from `public/unity-build/` inside the React app. When the user navigates to `/play`, the React component initialises the Unity loader and mounts the game canvas.

Once the game is loaded, React passes the player's JWT access token into Unity via the **Unity JSLib bridge** — a JavaScript interop layer that allows the host web page to call functions inside the running Unity instance. This gives Unity the token it needs to make authenticated API calls.

### 3. React App → Go API
All API calls from the React frontend (login, register, leaderboard, match history) go over HTTPS to the Go + Gin backend. Protected routes include the JWT access token in the `Authorization: Bearer <token>` header. Token refresh uses an httpOnly cookie carrying the refresh token.

### 4. Unity WebGL → Go API
During gameplay, Unity makes HTTP calls to the same Go API (e.g. recording a match result at the end of a game). These calls use `UnityWebRequest` and include the JWT access token received from React via the JSLib bridge.

### 5. Go API → MongoDB
The backend reads and writes all persistent data (player accounts, match history, statistics, leaderboard) through the official MongoDB Go driver. The connection string is provided via the `MONGO_URI` environment variable.

---

## Authentication Flow (Sequence)

```mermaid
sequenceDiagram
    actor User
    participant React
    participant Unity
    participant API as Go API
    participant DB as MongoDB

    User->>React: Opens /login
    React->>API: POST /auth/login
    API->>DB: Find player by username
    DB-->>API: Player document
    API-->>React: Access token (1h) + Refresh token (7d, httpOnly cookie)

    User->>React: Navigates to /play
    React->>Unity: JSLib bridge — passes access token
    Unity->>API: POST /matches (Authorization: Bearer token)
    API->>DB: Save match result
    DB-->>API: OK
    API-->>Unity: 200 OK
```

---

## Deployment Overview

| Component | Platform | Notes |
|---|---|---|
| React App + Unity WebGL | Render (static site) | Single deployment — Unity build in `public/unity-build/` |
| Go + Gin API | Render (web service) | Docker container |
| MongoDB | MongoDB Atlas or Docker | Connection via `MONGO_URI` env variable |
