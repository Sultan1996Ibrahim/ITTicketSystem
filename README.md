\# IT Ticket System



ASP.NET Core MVC Ticketing System for academic use.



\## Requirements

\- .NET SDK 8 or newer

\- PostgreSQL

\- Visual Studio 2022 or VS Code



---



\## Setup Instructions (Students)



\### 1) Clone the repository

```bash

git clone https://github.com/Sultan1996Ibrahim/ITTicketSystem.git

cd ITTicketSystem





Create a PostgreSQL database (empty), for example:
ITTicketSystem\_DB




Then copy the example config:
copy appsettings.Development.example.json appsettings.Development.json







Edit appsettings.Development.json and set your connection string:

"ConnectionStrings": {

&nbsp; "DefaultConnection": "Host=localhost;Database=ITTicketSystem\_DB;Username=postgres;Password=YOUR\_PASSWORD"

}







Run the application:

dotnet run





The application will:



Apply migrations automatically



Seed departments



Seed default users






Default Accounts

Role	 Username	Password

Admin	  admin	          1234



Additional users and managers are seeded automatically.



Notes



Database is NOT included (created automatically)



Uploaded files are stored locally and ignored by Git



This project is for educational purposes






Author



Sultan Aleidi



