# Expense Management System

A simple but functional expense management system built with ASP.NET Core 7.0 and Entity Framework Core. This project showcases C# development skills and demonstrates key .NET features.

## Features

- User authentication and role-based authorization
- Expense submission, editing, and deletion
- Expense approval workflow for managers
- Basic reporting and analytics
- Clean and responsive UI using Bootstrap

## Technologies Used

- **ASP.NET Core 7.0**: Modern, cross-platform framework for building web applications
- **Entity Framework Core**: ORM for data access
- **SQL Server**: Database for storing application data
- **Identity Framework**: Authentication and authorization
- **Bootstrap 5**: Frontend styling
- **LINQ**: Data querying
- **C# 10**: Latest language features

## Getting Started

### Prerequisites

- .NET 7.0 SDK
- Visual Studio 2022 or VS Code
- SQL Server or SQL Server Express LocalDB

### Setup Instructions

1. Clone the repository
   ```
   git clone https://github.com/yourusername/expense-management-system.git
   ```

2. Navigate to the project directory
   ```
   cd expense-management-system
   ```

3. Restore dependencies
   ```
   dotnet restore
   ```

4. Apply database migrations
   ```
   dotnet ef database update
   ```

5. Run the application
   ```
   dotnet run
   ```

6. Open your browser and navigate to `https://localhost:5001`

### Default Accounts

The system seeds an admin account on first run:
- Email: admin@example.com
- Password: Admin123!

## Project Structure

- **Models**: Domain entities (Expense, Category, ApplicationUser)
- **Controllers**: Business logic and request handling
- **Views**: UI templates for rendering HTML
- **Data**: Database context and configurations
- **wwwroot**: Static assets (CSS, JS, images)

## Future Enhancements

- Receipt uploads with file storage
- Email notifications
- Advanced reporting with charts and exports
- API endpoints for mobile integration

## License

This project is licensed under the MIT License - see the LICENSE file for details.