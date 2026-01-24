# GastroDesk - Restaurant Management System

A WPF desktop application for restaurant management built with MVVM architecture and Entity Framework Core.

## Features

- **User Authentication**: Login system with role-based access (Admin/Manager/Waiter)
- **Menu Management**: CRUD operations for dishes and categories
- **Order Management**: Create, modify, and track customer orders
- **Daily Revenue View**: View sales statistics and order history
- **Reports**: Generate daily/weekly PDF reports
- **Data Export/Import**: Export menu to JSON/XML, import from files

## Technologies

| Category | Technology |
|----------|-----------|
| UI | WPF (.NET 8), XAML |
| Architecture | MVVM (Model-View-ViewModel) |
| ORM | Entity Framework Core 8 |
| Database | SQLite |
| PDF Generation | QuestPDF |
| Testing | xUnit |


## Database

SQLite database 

## Project Structure

```
GastroDesk/
├── Models/           # Entity classes and enums
├── ViewModels/       # MVVM ViewModels
├── Views/            # XAML views
├── Services/         # Business logic services
├── Data/             # EF Core DbContext
├── Commands/         # ICommand implementations
├── Converters/       # XAML value converters
└── Helpers/          # Utility classes
```

## Design Patterns

1. **Singleton Pattern** (Creational): `DbContextFactory` - Single instance for database context creation
2. **Command Pattern** (Behavioral): `RelayCommand`, `AsyncRelayCommand` - MVVM command binding

### Used environment

- .NET 8 SDK
- Visual Studio 2022

### Running the Application

```bash
cd GastroDesk
dotnet run
```

### Default Credentials

- **Username**: admin
- **Password**: admin123

### Running Tests

```bash
dotnet test
```

## Use Cases

1. **UC1: User Authentication** - Login with username/password
2. **UC2: Menu Management** - Create, edit, delete dishes and categories
3. **UC3: Order Management** - Create orders, add items, complete/cancel orders
4. **UC4: Daily Revenue View** - View daily sales and statistics
5. **UC5: Menu Export** - Export menu to JSON/XML format
6. **UC6: Reports** - Generate PDF reports (daily/weekly)

