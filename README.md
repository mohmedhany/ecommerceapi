# E-Commerce API

A RESTful API for an e-commerce platform built with ASP.NET Core, Entity Framework Core, and JWT authentication.

## Features

- **User Authentication** - Register, login, social login, password reset with JWT tokens
- **Product Management** - CRUD operations, search, category filtering, pagination
- **Shopping Cart** - Add, update, remove items, quantity management
- **Order Processing** - Create orders from cart, order history, stock reduction
- **Dashboard Analytics** - Revenue reports, top products, user stats, inventory alerts
- **File Upload** - Product image upload with validation
- **Email Notifications** - Welcome emails, order confirmations, password reset

## Tech Stack

- **.NET 10** - Runtime
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM with SQL Server
- **JWT** - Authentication
- **xUnit** - Testing
- **Moq** - Mocking

## Project Structure

```
Ecommerce_API/
├── ECommerceApi/           # Main API project
│   ├── Controllers/       # API endpoints
│   ├── Services/         # Business logic
│   ├── Entities/        # Database models
│   ├── DTOs/           # Data transfer objects
│   └── Data/            # DbContext
└── ECommerceApi.Tests/   # Unit tests
```

## Getting Started

### Prerequisites

- .NET 10 SDK
- SQL Server (or SQL Server Express)

### Configuration

Update `appsettings.json` with your connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ECommerce;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### Run

```bash
# Restore packages
dotnet restore

# Run the API
dotnet run --project Ecommerce_API/ECommerceApi

# Run tests
dotnet test
```

## API Endpoints

### Authentication
| Method | Endpoint | Description |
|-------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | User login |
| POST | `/api/auth/social-login` | Social auth (Google, Facebook) |
| POST | `/api/auth/forgot-password` | Request password reset |
| POST | `/api/auth/reset-password` | Reset password |
| POST | `/api/auth/change-password` | Change password |

### Products
| Method | Endpoint | Description |
|-------|----------|-------------|
| GET | `/api/products` | List products (paginated) |
| GET | `/api/products/{id}` | Get product by ID |
| GET | `/api/products/search?q=` | Search products |
| GET | `/api/products/category/{id}` | Products by category |
| POST | `/api/products` | Create product |
| PUT | `/api/products/{id}` | Update product |
| DELETE | `/api/products/{id}` | Delete product |

### Categories
| Method | Endpoint | Description |
|-------|----------|-------------|
| GET | `/api/categories` | List categories |
| GET | `/api/categories/{id}` | Get category |
| POST | `/api/categories` | Create category |
| DELETE | `/api/categories/{id}` | Delete category |

### Cart
| Method | Endpoint | Description |
|-------|----------|-------------|
| GET | `/api/cart` | Get user cart |
| POST | `/api/cart` | Add to cart |
| PUT | `/api/cart/{productId}` | Update quantity |
| DELETE | `/api/cart/{productId}` | Remove from cart |
| DELETE | `/api/cart` | Clear cart |

### Orders
| Method | Endpoint | Description |
|-------|----------|-------------|
| GET | `/api/orders` | User orders |
| GET | `/api/orders/{id}` | Order details |
| POST | `/api/orders` | Create order |

### Dashboard (Admin)
| Method | Endpoint | Description |
|-------|----------|-------------|
| GET | `/api/dashboard/stats` | Overview stats |
| GET | `/api/dashboard/revenue` | Revenue report |
| GET | `/api/dashboard/top-products` | Best sellers |
| GET | `/api/dashboard/low-stock` | Inventory alerts |

### Upload
| Method | Endpoint | Description |
|-------|----------|-------------|
| POST | `/api/upload/image` | Upload image |

## Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Test Coverage:
- **ProductService** - CRUD, search, pagination
- **CategoryService** - CRUD operations
- **CartService** - Cart management
- **OrderService** - Order creation, stock updates
- **AuthService** - Registration, login, social auth
- **DashboardService** - Analytics
- **FileService** - File operations
- **EmailService** - Email sending

**Total: 60 tests**

## Security

- JWT Bearer authentication
- Password hashing with SHA256 + salt
- Input validation
- SQL injection prevention (EF Core)
- File type/size validation

## License

MIT