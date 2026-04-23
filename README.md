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

### Authentication (`/api/auth`)
| Method | Endpoint | Auth | Description |
|-------|----------|------|-------------|
| POST | `/register` | No | Register new user |
| POST | `/login` | No | User login |
| POST | `/social-login` | No | Social auth (Google, Facebook) |
| POST | `/forgot-password` | No | Request password reset |
| POST | `/reset-password` | No | Reset password with token |
| POST | `/change-password` | Yes | Change password |
| PUT | `/profile` | Yes | Update profile |
| GET | `/me` | Yes | Get current user |

### Products (`/api/products`)
| Method | Endpoint | Auth | Description |
|-------|----------|------|-------------|
| GET | `/` | No | List products (paginated) |
| GET | `/{id}` | No | Get product by ID |
| GET | `/search?q=` | No | Search products |
| GET | `/category/{id}` | No | Products by category |
| POST | `/` | Yes | Create product |
| PUT | `/{id}` | Yes | Update product |
| DELETE | `/{id}` | Yes | Delete product |

### Categories (`/api/categories`)
| Method | Endpoint | Auth | Description |
|-------|----------|------|-------------|
| GET | `/` | No | List categories |
| GET | `/{id}` | No | Get category |
| POST | `/` | Yes | Create category |
| DELETE | `/{id}` | Yes | Delete category |

### Cart (`/api/cart`)
| Method | Endpoint | Auth | Description |
|-------|----------|------|-------------|
| GET | `/` | Yes | Get user cart |
| POST | `/` | Yes | Add to cart |
| PUT | `/{productId}` | Yes | Update quantity |
| DELETE | `/{productId}` | Yes | Remove from cart |
| DELETE | `/` | Yes | Clear cart |

### Orders (`/api/orders`)
| Method | Endpoint | Auth | Description |
|-------|----------|------|-------------|
| GET | `/` | Yes | User orders |
| GET | `/{id}` | Yes | Order details |
| POST | `/` | Yes | Create order |

### Dashboard (`/api/dashboard`)
| Method | Endpoint | Auth | Description |
|-------|----------|------|-------------|
| GET | `/stats` | Yes | Overview stats |
| GET | `/revenue` | Yes | Revenue report |
| GET | `/top-products` | Yes | Best sellers |
| GET | `/low-stock` | Yes | Inventory alerts |
| GET | `/orders-stats` | Yes | Order statistics |
| GET | `/users-stats` | Yes | User statistics |

### Upload (`/api/upload`)
| Method | Endpoint | Auth | Description |
|-------|----------|------|-------------|
| POST | `/image` | Yes | Upload image |
| POST | `/images` | Yes | Upload multiple |
| DELETE | `/` | Yes | Delete image |

## Testing

```bash
# Run all tests
dotnet test
```

**Test Coverage:**
- ProductService - CRUD, search, pagination
- CategoryService - CRUD operations
- CartService - Cart management
- OrderService - Order creation, stock updates
- AuthService - Registration, login, social auth
- DashboardService - Analytics
- FileService - File operations
- EmailService - Email sending

**Total: 60 tests**

## Security

- JWT Bearer authentication
- Password hashing with SHA256 + salt
- Input validation
- SQL injection prevention (EF Core)
- File type/size validation

## License

MIT