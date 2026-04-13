# War of Tanks - Technical Justification

## 1. Purpose of the Document

This document explains and justifies the main technical choices made for the **War of Tanks** project. The goal is not only to list the technologies used, but to explain why they were selected for this specific project and why other common alternatives were not retained.

This note complements the other technical documents already present in the repository:

- UML diagrams: `docs/uml/`
- ERD (MongoDB schema): `docs/erd/`
- State machines: `docs/state-machines/`
- JWT implementation guide: `docs/jwt-guide.md`
- Global project presentation: `README.md`

## 2. Project Context

War of Tanks is a **2D top-down RTS game** developed as a school project. The player controls a small team of tanks against an AI-controlled enemy team. The project also includes a web interface and a backend layer to manage authentication, player data, match history, and leaderboard information.

The architecture is split into four main parts:

- **Unity** for gameplay
- **React** for the web interface
- **Go + Gin** for the API
- **MongoDB** for persistence

This separation is justified by the nature of the project: the gameplay layer and the web application layer do not have the same technical needs, so using one single technology for all responsibilities would be less efficient.

## 3. Global Architecture Choice

The project follows a layered architecture:

`React frontend -> Go/Gin API -> MongoDB`

The Unity game is integrated as a **WebGL build embedded in the React application**. This means the game is loaded inside the web app, while both React and Unity communicate with the same backend API.

This architecture offers three main advantages:

- clear separation of responsibilities,
- easier maintenance,
- simpler integration between gameplay, authentication, and persistent data.

## 4. Unity 2020.3.49f1

### Role

Unity is the main engine used to build the game itself. It handles scenes, prefabs, gameplay logic, input, tilemaps, physics, AI systems, and WebGL export. The repository confirms the Unity version in `UNITY/ProjectSettings/ProjectVersion.txt`: **2020.3.49f1**.

### Alternatives considered

- Unity 6 or newer versions


### Justification

Unity 2020.3.49f1 was chosen because it is an **LTS version**, which makes it more stable and less risky for a project with strict deadlines. The project does not need the newest rendering or editor features, so a stable version is more valuable than a recent one.

## 5. Built-in Render Pipeline

### Role

The rendering pipeline defines how the game is visually rendered. The project uses the **Built-in Pipeline**, as stated in the repository documentation.

### Alternatives considered

- URP
- HDRP

### Justification

The game is a simple **2D top-down project**, so the Built-in Pipeline is sufficient. It avoids unnecessary setup and keeps the project lighter, especially for WebGL compatibility.

URP would add more configuration without solving a real problem for this project. HDRP is not appropriate because it targets visually advanced 3D projects, which is outside the scope of War of Tanks.

## 6. C# for Gameplay Programming

### Role

C# is used for all Unity gameplay systems: tank behavior, state machines, AI, navigation, and game flow.

### Alternatives considered

- Unity Visual Scripting only
- Godot Engine with GDScript
- Unreal Engine with C++ or Blueprints

### Justification

C# is the natural choice in Unity and is well suited to the project's structure. The gameplay includes systems such as AI logic, pathfinding, and state management, which are easier to maintain, version, and document in code than in visual scripting alone.

Godot with GDScript was not retained because the team already had experience in Unity and switching engines would have introduced a significant learning curve without any gameplay benefit. Unreal Engine was not considered because it is primarily designed for high-fidelity 3D games — its 2D tooling is limited compared to Unity, and its complexity (C++ build times, heavy editor) would be disproportionate for a 2D top-down school project.

## 7. Go + Gin for the Backend

### Role

The backend is responsible for authentication, player management, match recording, and leaderboard-related data. In the repository, it is implemented in `BACKEND/` using:

- **Go**
- **Gin**
- the MongoDB Go driver

### Alternatives considered

- Node.js + Express
- Python + FastAPI
- PHP + Laravel

### Justification

Go was chosen because it provides:

- strong static typing,
- good performance,
- simple deployment,
- clear and readable code.

For this project, the backend must remain lightweight and reliable. Go fits that need well.

Compared with Node.js, Go offers stronger typing by default and a simpler deployment model. Compared with FastAPI, it is better suited to a small REST backend without Python-specific ecosystem needs. Laravel was not retained because it would add more framework overhead than necessary.

Gin was selected because it adds practical routing and middleware tools while staying lightweight.

## 8. MongoDB

### Role

MongoDB is used to store player accounts, match history, statistics, and leaderboard-related data. It is also present in the root `docker-compose.yml` as the database service.

### Alternatives considered

- PostgreSQL
- MySQL
- SQLite

### Justification

MongoDB was chosen because the project data is naturally **document-oriented**. Match summaries, player stats, and evolving game-related data fit well into flexible JSON-like documents.

A relational database such as PostgreSQL or MySQL would also work, but it would introduce more rigidity than necessary for a project that may evolve quickly during development. SQLite was not retained because the project is designed as a multi-service web application, not a local-only prototype.

## 9. React + TypeScript

### Role

The frontend in `FRONTEND/` is responsible for the web interface: login, navigation, and future pages such as leaderboard and match history. It also hosts the Unity WebGL build inside the browser application.

### Alternatives considered

- Vue
- Angular
- plain JavaScript

### Justification

React was chosen because it is well adapted to component-based interfaces and integrates cleanly with an embedded Unity WebGL page. It also benefits from a large ecosystem and good team familiarity.

TypeScript was added because the frontend exchanges structured data with the backend. It reduces errors by checking types at compile time, which is useful for auth flows, player data, and match responses. Plain JavaScript would be faster to start with, but riskier once the application grows.

## 10. Vite

### Role

Vite is the frontend build and development tool used in `FRONTEND/package.json`.

### Alternatives considered

- Create React App
- Webpack
- Next.js

### Justification

Vite was chosen because it offers a fast and simple frontend workflow. For a student project, quick startup, fast reload, and minimal configuration are more important than advanced framework features.

Create React App is older and slower. Webpack would require more configuration. Next.js was not necessary because the project does not require server-side rendering.

## 11. Tailwind CSS

### Role

The frontend dependencies show the use of **Tailwind CSS 4** for styling the web interface.

### Alternatives considered

- plain CSS only
- CSS Modules
- Bootstrap

### Justification

Tailwind was chosen because it allows fast interface construction while keeping design flexibility. It is useful for building authentication pages and future dashboard-style pages without introducing a heavy visual framework.

Bootstrap would impose a more generic look, while plain CSS alone would slow down iteration.

## 12. JWT Authentication

### Role

The project uses JWT-based authentication with:

- an access token,
- a refresh token,
- authenticated calls from React,
- authenticated calls from the Unity WebGL build.

### Alternatives considered

- server-side sessions
- cookie-only authentication

### Justification

JWT is a good fit because the project has two client contexts: the React application and the Unity WebGL runtime. A token-based approach makes it easier for both to communicate with the same backend API.

The access-token and refresh-token strategy is also consistent with the project's backend documentation and supports a clean separation between login, protected requests, and session renewal. The full implementation details and token lifetime decisions are documented in `docs/jwt-guide.md`.

## 13. Unity WebGL Embedded in React

### Role

The selected deployment model is to export the Unity game as a **WebGL build** and load it inside the React application, on a route dedicated to gameplay.

### Alternatives considered

- deploy Unity separately
- host the game and the frontend on different origins
- keep the game outside the web app

### Justification

Embedding Unity in React simplifies the project significantly:

- one web entry point,
- easier authentication flow,
- no unnecessary cross-origin complexity,
- direct data exchange between React and Unity through a JavaScript bridge.

This choice is especially important because the player must be able to authenticate in the web app and then launch the game without switching to a different system.

## 14. Docker + Docker Compose

### Role

Docker and Docker Compose are used to run the backend environment in a reproducible way. The root `docker-compose.yml` already defines:

- a backend service,
- a MongoDB service,
- networking between them,
- persistent MongoDB storage.

### Alternatives considered

- manual installation on each machine
- isolated Docker commands without Compose
- no containerization

### Justification

Docker was chosen to avoid environment differences between team members and evaluators. It guarantees a more consistent runtime and reduces setup friction.

Docker Compose was retained because the backend depends on MongoDB. Since the project includes more than one service, Compose is the simplest way to start the stack in one command.

## 15. Deployment Strategy

### Role

The project documentation identifies **Render** as the intended deployment platform for the web application and backend services.

### Alternatives considered

- VPS deployment
- multi-provider deployment
- local-only delivery

### Justification

Render is a pragmatic choice for a school project because it simplifies deployment and demonstration. The goal is not to design a complex infrastructure, but to provide a clear and reproducible delivery path for evaluation.

## 16. Final Conclusion

The War of Tanks stack was chosen with three priorities in mind:

- **stability**
- **simplicity**
- **coherence between systems**

Each major technology answers a concrete project need:

- **Unity 2020.3.49f1** for stable 2D game development and WebGL export,
- **Built-in Pipeline** for a lightweight 2D rendering setup,
- **C#** for maintainable gameplay systems,
- **Go + Gin** for a simple and efficient REST API,
- **MongoDB** for flexible persistence,
- **React + TypeScript** for a robust web interface,
- **Vite** for fast frontend development,
- **Tailwind CSS** for efficient styling,
- **JWT** for authentication across React and Unity,
- **Docker Compose** for reproducible execution,
- **Unity WebGL embedded in React** for clean integration and simpler delivery.

These choices are not presented as universal best practices in all contexts. They are justified because they are the most suitable for the concrete needs, constraints, and scale of this project.
