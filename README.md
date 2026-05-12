# Tiny URL API

REST API for Tiny URL generation and redirection.

Built using ASP.NET Core 8 and Entity Framework Core.

---

## Features

- Create Tiny URL
- Get Public URL
- Redirect to original URL(Navigating from Homepage)
- Redirect to original URL(Copy to Clipboard and paste it in new tab) 
- Delete URL 
- Search URL

---

## Tech Stack

- ASP.NET Core 8 Web API
- Entity Framework Core
- SQL Server
- Swagger

---

## Prerequisites

Install:

- .NET 8 SDK

Verify installation:

```bash
dotnet --version
```

---

## Run Project Locally

Restore packages:

```bash
dotnet restore
```

Apply migrations:

```bash
dotnet ef database update
```

Run application:

```bash
dotnet run
```

API runs on:

```txt
https://localhost:7125
```

---

## Database

Database used:

```txt
SQL Server
```

Database file:

```txt
TinyUrlDb
```

---

## API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/shortenUrl` | Create tiny URL |
| GET | `/api/urls` | Get public URLs |
| GET | `api/{shortCode}` | Redirect to original URL(Navigating from Homepage) |
| GET | `{shortCode}` | Redirect to original URL(Copy to Clipboard and paste it in new tab) |
| DELETE | `/api/delete/{id}` | Delete URL |
| SEARCH | `/api/search` | Search URL |

---

## Entity Framework Migration

Create migration:

```bash
dotnet ef migrations add MigrationName
```

Update database:

```bash
dotnet ef database update
```

---

## Swagger

Swagger URL:

```txt
https://localhost:7125/swagger
```

---

## Deployment

Backend deployed using:

- Azure App Service

---

## Live API

```txt
https://tinyurl-api-f7d6avd9b7dtana3.centralindia-01.azurewebsites.net/swagger/index.html
```
