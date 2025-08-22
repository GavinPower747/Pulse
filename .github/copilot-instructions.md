# Pulse - AI Coding Agent Instructions

## Project Overview
Pulse is a microblogging platform using .NET 8, Blazor SSR, HTMX, and event-driven architecture. It follows a modular monolith pattern with strict module boundaries and message-based communication.

## Architecture & Module System

### Module Structure (Critical Pattern)
Each module in `src/Modules/` follows strict boundaries:
- **Contracts/**: Public interfaces and DTOs - ONLY these can be referenced by other modules
- **Domain/**: Internal implementation (API endpoints, services, data) - NEVER reference from other modules
- **UI/**: Blazor components and static assets
- **Tests/**: Unit and integration tests

Module registration pattern in `Domain/Module.cs` using Autofac:
```csharp
public class PostsModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PostQueryService>().As<IPostQueryService>().SingleInstance();
        builder.RegisterDbContext<PostsContext>(Configuration.Database.ConnectionString);
    }
}
```

### API Patterns
- Use **REPR pattern** (Request-Endpoint-Response) in `Domain/API/`
- Route registration via extension methods: `app.MapPostRoutes()`
- All endpoints require authorization by default
- Example endpoint structure:
```csharp
group.MapPost("/", async ([FromForm] CreatePostRequest request, [FromServices] CreatePostEndpoint handler) => 
    await handler.Handle(request));
```

## Frontend Architecture

### Blazor SSR + HTMX Pattern
- Use **Blazor SSR only** - no WebAssembly or Server-side Blazor
- HTMX handles server state: `hx-post="/api/post" hx-swap="outerHTML"`
- Client state via custom framework controllers in `wwwroot/js/src/controllers/`
- Form preservation: use `hx-preserve` on form inputs

### Custom Frontend Framework
Pulse uses a custom JavaScript framework for client-side interactivity built around the Controller pattern:

#### Controller System
- **Base Controller Class**: All controllers extend `Controller` from `framework/controller.js`
- **Auto-registration**: Controllers auto-load via `data-controller="controller-name"` attributes
- **Naming Convention**: `data-controller="post-form"` → `PostFormController` in `controllers/post-form-controller.js`
- **Lifecycle Methods**: Override `connect()` for initialization, `disconnect()` for cleanup

#### Component System
- **Components**: Child elements marked with `data-component="component-name"` become properties
- **Auto-mapping**: `data-component="post-box"` becomes `this.postBox` in controller
- **Proxy Access**: Components proxy to underlying DOM elements for direct manipulation

#### Data Attributes
- **Controller Data**: Use `data-js-property-name` for controller-specific data
- **Auto-conversion**: `data-js-post-length` becomes `this.postLength` property
- **Reactive**: Changes to data attributes trigger `propertyNameChanged(newValue, oldValue)` methods

#### Event System
- **Custom Events**: Use `this.dispatch(eventName, { detail, bubbles, target })` for custom events
- **Event Subscription**: Use `this.subscribe(eventName, callback, { target })` for listening
- **Event Constants**: Define events in `consts/events.js` (e.g., `PostCreatedEvent`)

#### Framework Usage Examples:
```javascript
// Controller example
export default class PostFormController extends Controller {
    connect() {
        // Access components as properties
        this.postBox.addEventListener("input", this._handleInput.bind(this));
        
        // Access data attributes as properties
        const maxLength = parseInt(this.postMaxLength);
    }
    
    // Reactive data attribute changes
    postLengthChanged(newValue, oldValue) {
        this.updateLengthIndicator(newValue);
    }
}
```

```html
<!-- Blazor component usage -->
<form data-controller="post-form" 
      data-js-post-max-length="280"
      data-js-post-length="0">
    <textarea data-component="post-box" name="content"></textarea>
    <svg data-component="length-indicator"></svg>
</form>
```

### Styling & Assets
- TailwindCSS with custom Pulse color palette
- Module-specific assets in `UI/wwwroot/`
- Global assets in `Pulse.WebApp/wwwroot/`
- Build CSS: `npm run css:build` in WebApp directory

## Event-Driven Communication

### Message Bus Pattern
Modules communicate via RabbitMQ messages, never direct references:
```csharp
// Publishing
await _bus.Publish(new AddPostToTimelineCommand(followerId, postId, createdAt), token);

// Consuming
internal class UserFollowedConsumer : IConsumer<UserFollowedEvent>
{
    public async Task Consume(UserFollowedEvent evt, CancellationToken token) { }
}
```

Register consumers in module: `builder.RegisterConsumer<UserFollowedConsumer, UserFollowedEvent>()`

## Development Workflow

### Local Development
1. **Dependencies**: `docker compose -f docker/compose-dependencies.yml up -d` (PostgreSQL, RabbitMQ, Keycloak, MinIO)
2. **Run app**: Use VS Code task "Run Pulse Application" or `dotnet run` in `src/Pulse.WebApp/`
3. **Database per module**: Each module has its own DbContext and connection string

### Authentication
- Keycloak OIDC (admin/admin at localhost:8080)
- Access current user: `@inject IdentityProvider` → `GetCurrentUser()`
- All API routes require authorization unless explicitly marked otherwise

### Testing Patterns
- Use `Pulse.Tests.Util` for shared test infrastructure
- `RabbitMqFixture` for message bus testing
- Fakers for test data generation in each module's `Tests/Fakers/`
- Run tests: `dotnet test` or use VS Code test explorer

## Key Conventions
- **Module isolation**: Never reference another module's Domain assembly
- **Database ownership**: Each module owns its data - no shared entities
- **Static typing**: Use records for DTOs and value objects
- **Async patterns**: All I/O operations must be async with CancellationToken
- **Error handling**: Return appropriate result types, don't throw exceptions for business logic

## External Integrations
- **Storage**: MinIO S3-compatible blob storage for attachments
- **Auth**: Keycloak realm configuration in `build/keycloak/realms/`
- **Messaging**: RabbitMQ with exchange/queue setup via `AmqpDIExtensions`
- **Data**: PostgreSQL with separate databases per module
