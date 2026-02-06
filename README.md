# Loggy

Structured start/end logging with automatic scope completion.

## Why Loggy

Loggy wraps your logs in a consistent start/end envelope with a unique ID, so each operation is easy to trace. The primary pattern is **instantiate a scope and let it auto-end** when it disposes.

## Core Pattern: Auto-End on Dispose

```csharp
using PolyhydraGames.Loggy;
using Microsoft.Extensions.Logging;

public async Task HandleAsync(ILogger logger)
{
    using var loggy = logger.LoggyStart(new { Action = "HandleAsync", RequestId = "abc-123" });

    // ... work ...
    loggy.LogInformation("Doing work");

    await Task.Delay(10);
}
// scope ends automatically here (Dispose -> End)
```

This pattern avoids missing "end" logs when code returns early or throws. The scope always closes.

## Manual End (Optional)

If you want to end early or add end metadata, call `End` explicitly:

```csharp
using var loggy = logger.LoggyStart("ProcessOrder", new { OrderId = 42 });

// ... work ...
loggy.End(new { Status = "Success", Items = 3 });
```

## Structured Context

Any anonymous object passed to `LoggyStart` is serialized in the start log:

```csharp
using var loggy = logger.LoggyStart(new
{
    UserId = user.Id,
    Channel = "Twitch",
    MessageId = message.Id
});
```

## Logging Within the Scope

```csharp
using var loggy = logger.LoggyStart("InboundMessage");
loggy.LogInformation("Message accepted");
loggy.LogWarning("Missing field: {0}", "UserId");
```

## Installation

Reference the project or package in your solution:

```xml
<ProjectReference Include="..\..\..\Loggy\src\PolyhydraGames.Loggy.csproj" />
```

## API Summary

- `ILogger.LoggyStart(object? args = null, string caller = "")`
- `ILogger.LoggyStart(string name, object? args = null)`
- `LoggyScope.End(object? args = null)`
- `LoggyScope.LogInformation(string message, params object?[] args)`
- `LoggyScope.LogWarning(string message)`
- `LoggyScope.LogWarning(Exception ex, string message)`
- `LoggyScope.LogCritical(Exception ex, string message)`

## Project Structure

- `src/` - Source code
- `tests/` - Tests (if/when added)
