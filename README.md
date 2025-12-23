# IT Ticket System (ASP.NET Core)

A simple IT Ticket Management System built with ASP.NET Core MVC and PostgreSQL.

---

## Default Login

| Role  | Username | Password |
|------|---------|----------|
| Admin | admin | 1234 |

---

## Option 1: Run Without Docker (Recommended for learning)

### Requirements
- .NET 8 SDK
- PostgreSQL
- Visual Studio / VS Code

### Steps

1. Clone the repository
```bash
git clone https://github.com/Sultan1996Ibrahim/ITTicketSystem.git
cd ITTicketSystem
Create PostgreSQL database

sql
Copy code
CREATE DATABASE ITTicketSystem_V2;
Update connection string in appsettings.Development.json

json
Copy code
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=ITTicketSystem_V2;Username=postgres;Password=1234"
}
Run the project

bash
Copy code
dotnet run
Open browser

arduino
Copy code
http://localhost:5000
Database, departments, and default users will be created automatically on first run.

Option 2: Run With Docker (One-command setup)
Requirements
Docker Desktop

Steps
bash
Copy code
docker compose up --build
Open:

arduino
Copy code
http://localhost:8080


Notes
Database migrations and seed data run automatically.

Docker is optional and not required for development.

Author
Sultan