# Changelog

All notable changes to the AzureStorage.Standard library will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-10-19

### 🎉 Initial Release

The first stable release of AzureStorage.Standard - a modern, simplified .NET client library for Azure Storage services.

### Added

#### AzureStorage.Standard.Blobs
- ✅ Upload blobs (text, binary, and stream-based)
- ✅ Download blobs with automatic memory management
- ✅ Delete blobs and containers
- ✅ List blobs with prefix filtering
- ✅ Check blob and container existence
- ✅ Get blob properties and metadata
- ✅ Set blob metadata
- ✅ Copy blobs within and across containers
- ✅ Built-in retry policies using Polly
- ✅ Comprehensive error handling with AzureStorageException
- ✅ Support for block blobs, append blobs, and page blobs
- ✅ Blob leasing operations
- ✅ Batch operations for bulk uploads/downloads

#### AzureStorage.Standard.Queues
- ✅ Send messages (text and binary)
- ✅ Receive messages with configurable visibility timeout
- ✅ Peek messages without removal
- ✅ Update message content and visibility
- ✅ Delete messages using pop receipts
- ✅ Batch send operations
- ✅ Queue management (create, delete, list, exists)
- ✅ Get message count
- ✅ Clear all messages
- ✅ Dequeue count tracking for poison message detection
- ✅ Message TTL configuration (up to 7 days)

#### AzureStorage.Standard.Tables
- ✅ Create, read, update, delete (CRUD) operations for entities
- ✅ Upsert operations (insert or update)
- ✅ Query entities with filtering
- ✅ Batch operations (up to 100 entities)
- ✅ Table management (create, delete, list, exists)
- ✅ Strongly-typed entity support
- ✅ Dynamic entity properties
- ✅ Pagination support for large result sets

#### AzureStorage.Standard.Files
- ✅ Upload files to Azure File Shares
- ✅ Download files with stream support
- ✅ Delete files and directories
- ✅ List files and directories
- ✅ Create and manage file shares
- ✅ Create directory hierarchies
- ✅ Get file properties and metadata
- ✅ Set file metadata
- ✅ File and directory existence checks
- ✅ Support for large files (up to 4 TiB)

#### AzureStorage.Standard.Core
- ✅ Shared StorageOptions configuration model
- ✅ RetryOptions for configurable retry policies
- ✅ AzureStorageException for unified error handling
- ✅ Domain models for all storage types
- ✅ Interface abstractions for dependency injection
- ✅ Support for connection strings and Azure Identity

### Technical Features

- 🎯 Multi-targeting: .NET Standard 2.1, .NET 8.0, .NET 9.0
- 📦 NuGet Central Package Management
- 🔄 Automatic semantic versioning using MinVer
- 🧪 Comprehensive unit and integration tests
- 📝 Full XML documentation with IntelliSense support
- 🔒 Source Link for debugging support
- 🚀 CI/CD pipeline with GitHub Actions
- 📊 Code quality analysis with CodeQL
- 🤖 Automated dependency updates with Dependabot
- 📄 MIT License

### Documentation

- ✅ Comprehensive README for each package
- ✅ Code examples and usage guides
- ✅ API documentation
- ✅ Testing guide
- ✅ Release process documentation

### Package Information

All packages are available on NuGet.org:
- [AzureStorage.Standard.Blobs](https://www.nuget.org/packages/AzureStorage.Standard.Blobs)
- [AzureStorage.Standard.Queues](https://www.nuget.org/packages/AzureStorage.Standard.Queues)
- [AzureStorage.Standard.Tables](https://www.nuget.org/packages/AzureStorage.Standard.Tables)
- [AzureStorage.Standard.Files](https://www.nuget.org/packages/AzureStorage.Standard.Files)
- [AzureStorage.Standard.Core](https://www.nuget.org/packages/AzureStorage.Standard.Core)

### Breaking Changes

None - this is the initial release.

### Known Issues

None at this time. Please report issues at: https://github.com/Clifftech123/AzureStorage.Standard/issues

### Migration Guide

This is the first release, so no migration is needed. For new projects:

```bash
# Install the packages you need
dotnet add package AzureStorage.Standard.Blobs
dotnet add package AzureStorage.Standard.Queues
dotnet add package AzureStorage.Standard.Tables
dotnet add package AzureStorage.Standard.Files
```

The Core package is automatically installed as a dependency.

---

## [0.0.0-alpha.0.8] - 2025-10-19

### Alpha Release

Pre-release version for testing before v1.0.0. All features included in v1.0.0.

---

## Release Links

- [GitHub Releases](https://github.com/Clifftech123/AzureStorage.Standard/releases)
- [NuGet Gallery](https://www.nuget.org/packages?q=AzureStorage.Standard)
- [Documentation](https://github.com/Clifftech123/AzureStorage.Standard)

## Support

- 📧 Issues: https://github.com/Clifftech123/AzureStorage.Standard/issues
- 💬 Discussions: https://github.com/Clifftech123/AzureStorage.Standard/discussions
- 📖 Documentation: https://github.com/Clifftech123/AzureStorage.Standard/wiki
