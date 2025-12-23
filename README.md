# IT Ticket System (ASP.NET Core)

A simple IT Ticket Management System built with ASP.NET Core MVC and PostgreSQL.

---

## Default Login

| Role  | Username | Password |
|-------|----------|----------|
| Admin |  admin   |   1234   |

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


2. Create PostgreSQL database
(sql)
CREATE DATABASE ITTicketSystem_V2;


3.Create appsettings.Development.json
(json)
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ITTicketSystem_V2;Username=postgres;Password=1234"
  }
}


3.Run the project
(bash)
dotnet run


4.Open browser
(arduino)
http://localhost:5000

Database, departments, and default users are created automatically on first run.




## Option 2: Run With Docker (One-command setup)
"https://www.docker.com/products/docker-desktop/"

### Requirements
-Docker Desktop

### Steps
(bash)
docker compose up --build

Open:
(arduino)
http://localhost:8080


Notes:

Docker is optional
Database migrations and seed data run automatically


Author
Sultan Aleidi

