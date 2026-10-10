# Students API

A RESTful Students management API built with **ASP.NET Core** and **SQL Server**, using a layered architecture, stored procedures, and secure JWT authentication with refresh tokens.

## Features

- Full CRUD operations for students
- Extra endpoints: passed students and average grade
- JWT authentication: access token + refresh token (7-day lifetime, rotation on refresh)
- Refresh tokens stored as BCrypt hashes, never in plain text
- Passwords hashed with BCrypt, the original password is never stored
- Authorization policy: a student can access their own data, an admin can access all
- Data access through SQL Server stored procedures

## Architecture

| Layer | Project | Responsibility |
|-------|---------|----------------|
| API | `StudentAPIproject` | Controllers (`stuAPIController`, `AuthController`), authorization handlers |
| Business | `Stu_Bussniss` | Business logic (`clsStudent`) |
| Data Access | `Stu_DataAccess` | DTOs and database access (`StuDataAccess`) |

## Tech Stack

C# · ASP.NET Core · SQL Server · ADO.NET · JWT · BCrypt.Net

## Getting Started

1. Clone the repo:
```bash
   git clone https://github.com/marwaammed9/StudentsAPI.git
```
2. Create the `StudentsDB` database in SQL Server and run the table and stored procedure scripts.
3. Copy `appsettings.example.json` to `appsettings.json` and fill in your own connection string and JWT settings.
4. Run the API:
```bash
   dotnet run --project StudentAPIproject
```

## Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/auth/login` | Log in, returns access + refresh token |
| POST | `/api/auth/refresh` | Get a new token pair |
| GET | `/api/students` | List all students |
| GET | `/api/students/{id}` | Get a student by Id |
| POST | `/api/students` | Add a student |
| PUT | `/api/students/{id}` | Update a student |
| DELETE | `/api/students/{id}` | Delete a student |

## Related

A WinForms desktop client for this API: **StudentDesktopApp** (login, dashboard, add/edit forms, automatic token refresh).

## Author

**Marwa Mohammed Basalah**, [GitHub](https://github.com/marwaammed9) · [LinkedIn](https://www.linkedin.com/in/marwa-mohammed-2718a0270)