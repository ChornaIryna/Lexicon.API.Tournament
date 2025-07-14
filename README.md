# Tournament API

---

## Overview
**Tournament API**, a robust and scalable backend solution for managing sports tournaments. This API is built with a focus on clean architecture, ensuring maintainability, testability, and separation of concerns. It leverages modern .NET technologies and best practices to provide a reliable foundation for your tournament management needs.

---

## Features

* **Comprehensive Tournament Management:** Create, read, update, and delete tournaments with ease.
* **Role-Based Access Control:** Secure your data with granular permissions for administrators and regular users, ensuring that only authorized personnel can perform specific actions.
* **Clean Architecture:** Designed with a layered approach (Domain, Application, Infrastructure, Presentation) to promote a clear separation of concerns, making the codebase easier to understand, test, and extend.
* **ASP.NET Core Identity:** Provides a secure and robust system for user authentication and authorization.

---

## Getting Started

### Prerequisites

* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or another supported database by Entity Framework Core)

### Installation

1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/ChornaIryna/Lexicon.API.Tournament.git](https://github.com/ChornaIryna/Lexicon.API.Tournament.git)
    cd Lexicon.API.Tournament
    ```

2.  **Update Configuration Settings:**
    Open `appsettings.json` (or `appsettings.Development.json`) and configure your database connection string and JwT:
    ```json
    "ConnectionStrings": {
      "TournamentContext": "Server=(localdb)\\mssqllocaldb;Database=TournamentDb;Trusted_Connection=True;MultipleActiveResultSets=true"
    },
      "DefaultAdminPassword": "<YourStrongAdminPasswordHere>", // Set a strong password for the default admin user and keep it in the secrets file (manage it in Tournament.Api)
      "JwTSettings": {
        "Issuer": "YourIssuer",
        "Audience": "YourAudience", // Ensure this matches your API's base URL
        "Key": "super-secret-key-that-is-at-least-32-characters-long", // **IMPORTANT: Generate a strong, unique secret key** (change it in "..\Tournament.Tests\IntegrationTests\CustomWebApplicationFactory.cs")
        "ExpirationMinutes": 60
      }
    }
    ```
    **Note:** For `DefaultAdminPassword` and `JwTSettings:Key`, make sure to use strong, unique values. **Do not use the placeholder values in a production environment.**

3.  **Run the API:**
    Navigate to the `Lexicon.Api.Tournament.Api` project directory and run the application

    The API will typically run on `https://localhost:7001` (or another port configured in `launchSettings.json`).
