# BTR Portal Web

Vue 3 frontend for BTR Portal. Milestone 7 delivers authentication, layout shell, and dashboard KPI home.

## Prerequisites

- Node.js 20+
- `btr.portal.api` running locally (default `http://localhost:5050`)

## Setup

```powershell
cd src/j05-btr-distrib/btr.portal.web
npm install
copy .env.example .env.development
```

## Development

```powershell
npm run dev
```

Open `http://localhost:5173`.

## Build

```powershell
npm run build
npm run preview
```

## Environment

| Variable | Description |
| --- | --- |
| `VITE_API_BASE_URL` | Base URL for `btr.portal.api` (no trailing slash). Local dev: API host only (e.g. `http://localhost:5050`). Production IIS: include the API application path (e.g. `http://server:8080/api`). Set before `npm run build` to the hostname clients use, not `localhost` unless users browse via localhost. |
