# IT Ticket System (ASP.NET Core)

A simple IT Ticket Management System built with ASP.NET Core MVC and PostgreSQL.


---


### Requirements
- .NET 10 SDK
- PostgreSQL
- Visual Studio / VS Code
- Git


### Steps

Clone the repository:

git clone https://github.com/Sultan1996Ibrahim/ITTicketSystem.git
cd ITTicketSystem


Create PostgreSQL database: 

(Copy code)
CREATE DATABASE ITTicketSystem_V2;



Update connection string in appsettings.Development.json:

(Copy code)
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=ITTicketSystem_V2;Username=postgres;Password=1234"
}




Run the project:


(Copy code)
dotnet restore
dotnet ef database update
dotnet run


Open browser:

(Copy code)
http://localhost:5000


Database, departments, and default users will be created automatically on first run.





---

## Default Login

| Role  | Username | Password |
|------|---------|----------|
| Admin | admin | 1234 |

---





Author
Sultan Aleidi