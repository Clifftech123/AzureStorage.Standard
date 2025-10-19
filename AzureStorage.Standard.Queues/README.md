
# AzureStorage.Standard.Queues

A simplified, modern .NET client library for Azure Queue Storage with built-in error handling and intuitive APIs for managing queues and messages.

[![NuGet](https://img.shields.io/nuget/v/AzureStorage.Standard.Queues.svg)](https://www.nuget.org/packages/AzureStorage.Standard.Queues/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

 **Simplified API** - Easy-to-use wrapper around Azure.Storage.Queues SDK
 **Queue Management** - Create, delete, and list queues
 **Message Operations** - Send, receive, peek, update, and delete messages
 **Binary Support** - Send/receive both text and binary messages
 **Batch Operations** - Send multiple messages efficiently
 **Visibility Timeout** - Control message processing windows
 **Message TTL** - Configure message expiration
 **Pop Receipts** - Update and delete messages safely
 **Comprehensive Error Handling** - Detailed exception information
 **Extensive Documentation** - Full XML documentation for IntelliSense

## Installation

```bash
dotnet add package AzureStorage.Standard.Queues
```

Or via Package Manager Console:

```powershell
Install-Package AzureStorage.Standard.Queues
```

## Quick Start

### 1. Configure the Client

```csharp
using AzureStorage.Standard.Queues;
using AzureStorage.Standard.Core;

var options = new StorageOptions
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=..."
};

var queueClient = new QueueClient(options);
```

### 2. Create a Queue

```csharp
await queueClient.CreateQueueIfNotExistsAsync("orders");
```

### 3. Send Messages

```csharp
// Send a text message
await queueClient.SendMessageAsync(
    queueName: "orders",
    message: "Process order #12345"
);

// Send with visibility timeout (delay processing)
await queueClient.SendMessageAsync(
    queueName: "orders",
    message: "Delayed order",
    visibilityTimeout: TimeSpan.FromMinutes(5)
);

// Send with time-to-live
await queueClient.SendMessageAsync(
    queueName: "orders",
    message: "Expiring order",
    timeToLive: TimeSpan.FromHours(1)
);
```

### 4. Receive Messages

```csharp
// Receive a single message
var messages = await queueClient.ReceiveMessagesAsync(
    queueName: "orders",
    maxMessages: 1
);

foreach (var message in messages)
{
    Console.WriteLine($"Message: {message.MessageText}");

    // Process the message...

    // Delete after processing
    await queueClient.DeleteMessageAsync(
        queueName: "orders",
        messageId: message.MessageId,
        popReceipt: message.PopReceipt
    );
}
```

### 5. Peek Messages

```csharp
// Preview messages without removing them
var peekedMessages = await queueClient.PeekMessagesAsync(
    queueName: "orders",
    maxMessages: 10
);

foreach (var message in peekedMessages)
{
    Console.WriteLine($"Peeked: {message.MessageText}");
}
```

## Advanced Usage

### Binary Messages

```csharp
// Send binary data
byte[] imageData = File.ReadAllBytes("image.jpg");
await queueClient.SendMessageAsync("images", imageData);

// Receive binary data
var messages = await queueClient.ReceiveMessagesAsync("images");
foreach (var message in messages)
{
    byte[] data = message.MessageBytes;
    // Process binary data...
}
```

### Batch Send Messages

```csharp
var messages = new List<string>
{
    "Order #001",
    "Order #002",
    "Order #003"
};

await queueClient.SendMessagesAsync("orders", messages);
```

### Update Messages (Extend Processing Time)

```csharp
var messages = await queueClient.ReceiveMessagesAsync("orders", maxMessages: 1);
var message = messages.First();

// Need more time to process? Update the visibility timeout
var updatedMessage = await queueClient.UpdateMessageAsync(
    queueName: "orders",
    messageId: message.MessageId,
    popReceipt: message.PopReceipt,
    message: message.MessageText,  // Can also update content
    visibilityTimeout: TimeSpan.FromMinutes(5)  // Extend processing time
);

// Use the new pop receipt for subsequent operations
await queueClient.DeleteMessageAsync(
    queueName: "orders",
    messageId: updatedMessage.MessageId,
    popReceipt: updatedMessage.PopReceipt
);
```

### Receive Multiple Messages

```csharp
// Receive up to 32 messages at once
var messages = await queueClient.ReceiveMessagesAsync(
    queueName: "orders",
    maxMessages: 32,
    visibilityTimeout: TimeSpan.FromMinutes(2)
);
```

### Get Message Count

```csharp
int messageCount = await queueClient.GetMessageCountAsync("orders");
Console.WriteLine($"Approximate message count: {messageCount}");
```

### Clear All Messages

```csharp
// Warning: Permanently deletes all messages!
await queueClient.ClearMessagesAsync("orders");
```

## Queue Management

```csharp
// List all queues
var queues = await queueClient.ListQueuesAsync();
foreach (var queue in queues)
{
    Console.WriteLine($"Queue: {queue}");
}

// Check if queue exists
bool exists = await queueClient.QueueExistsAsync("orders");

// Delete queue
await queueClient.DeleteQueueAsync("orders");
```

## Authentication Options

### Connection String (Development)

```csharp
var options = new StorageOptions
{
    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=myaccount;AccountKey=..."
};
```

### Account Key

```csharp
var options = new StorageOptions
{
    AccountName = "myaccount",
    AccountKey = "your-account-key"
};
```

### Service URI (Managed Identity)

```csharp
var options = new StorageOptions
{
    ServiceUri = new Uri("https://myaccount.queue.core.windows.net")
};
```

## Message Processing Pattern

```csharp
// Reliable message processing pattern
while (true)
{
    var messages = await queueClient.ReceiveMessagesAsync(
        queueName: "orders",
        maxMessages: 10,
        visibilityTimeout: TimeSpan.FromMinutes(2)
    );

    if (!messages.Any())
    {
        await Task.Delay(TimeSpan.FromSeconds(5));
        continue;
    }

    foreach (var message in messages)
    {
        try
        {
            // Process message
            await ProcessOrderAsync(message.MessageText);

            // Delete if successful
            await queueClient.DeleteMessageAsync(
                "orders",
                message.MessageId,
                message.PopReceipt
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to process message: {ex.Message}");
            // Message will become visible again after visibility timeout
        }
    }
}
```

## Best Practices

### 1. Message Size
Messages can be up to **64 KB** in size:
```csharp
// For larger data, store in Blob Storage and send URL
var blobUrl = await UploadToBlobAsync(largeData);
await queueClient.SendMessageAsync("orders", blobUrl);
```

### 2. Visibility Timeout
Set appropriate timeouts based on processing time:
```csharp
// Short-lived tasks
visibilityTimeout: TimeSpan.FromSeconds(30)

// Long-running tasks
visibilityTimeout: TimeSpan.FromMinutes(10)
```

### 3. Message Deduplication
Track processed messages to avoid duplicates:
```csharp
if (message.DequeueCount > 1)
{
    // Message has been processed before - check if already handled
    if (await IsAlreadyProcessedAsync(message.MessageId))
    {
        await queueClient.DeleteMessageAsync("orders", message.MessageId, message.PopReceipt);
        continue;
    }
}
```

### 4. Poison Messages
Handle messages that fail repeatedly:
```csharp
if (message.DequeueCount > 5)
{
    // Move to dead-letter queue or log for investigation
    await queueClient.SendMessageAsync("orders-poison", message.MessageText);
    await queueClient.DeleteMessageAsync("orders", message.MessageId, message.PopReceipt);
}
```

## Queue Naming Rules

- Must be **lowercase**
- 3-63 characters long
- Can contain letters, numbers, and hyphens
- Cannot start or end with a hyphen
- Cannot have consecutive hyphens

```csharp
// Valid names
"orders"
"order-processing-queue"
"queue123"

// Invalid names
"Orders" // uppercase not allowed
"or" // too short
"order--queue" // consecutive hyphens
```

## Supported Frameworks

- .NET Standard 2.0
- .NET Standard 2.1
- .NET 7.0
- .NET 8.0
- .NET 9.0

## Error Handling

```csharp
try
{
    await queueClient.SendMessageAsync("orders", "New order");
}
catch (AzureStorageException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
}
```

## Related Packages

- **[AzureStorage.Standard.Blobs](https://www.nuget.org/packages/AzureStorage.Standard.Blobs/)** - Azure Blob Storage client
- **[AzureStorage.Standard.Tables](https://www.nuget.org/packages/AzureStorage.Standard.Tables/)** - Azure Table Storage client
- **[AzureStorage.Standard.Files](https://www.nuget.org/packages/AzureStorage.Standard.Files/)** - Azure File Share client

## Documentation

For complete documentation, visit the [GitHub repository](https://github.com/Clifftech123/AzureStroage.Standard).

## License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues, questions, or suggestions, please [open an issue](https://github.com/Clifftech123/AzureStroage.Standard/issues) on GitHub.
