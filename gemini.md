# Gemini Context & Guidelines

This document provides context and guidelines for AI assistants (such as Gemini) and contributors working on the `collaborative-software-development` repository.

## 1. Project Architecture

-   **Frontend**: Windows Presentation Foundation (WPF) application. Uses .NET 10.
-   **Backend**: Containerised services running in Docker.

## 2. Language and Localisation

-   **British English**: All documentation, comments, variable names (where appropriate), and UI strings must use British English (e.g., `Colour`, `Initialise`, `Behaviour`).

## 3. Coding Standards

-   **Documentation**:
    -   **No XML Comments**: Do NOT use XML-style documentation comments (e.g., `/// <summary>`). Use standard single-line (`//`) comments.
    -   Use comments sparingly.
-   **C# / .NET**:
    -   Follow standard C# naming conventions (PascalCase for methods/types, camelCase for local variables).
    -   Use Async/Await patterns correctly to avoid deadlocks, especially on the UI thread.

## 4. WPF & Frontend Guidelines

-   **MVVM Pattern**: Strictly follow the Model-View-ViewModel (MVVM) design pattern. Logic should reside in ViewModels, not in code-behind (`.xaml.cs`) files.
-   **Data Binding**: Prefer data binding over direct UI manipulation.
-   **Threading**: Ensure all UI updates are marshaled to the Dispatcher/UI thread.
-   **XAML**: Keep XAML clean and use Styles/Templates for reusable UI components.

## 5. Backend & Docker Guidelines

-   **Containerisation**:
    -   Ensure Dockerfiles are optimised for layer caching.
    -   Use environment variables for secure configuration; never hardcode secrets or connection strings.
    -   Use web.config file for non secure configurations.
-   **Communication**: Define clear API contracts between the WPF client and the Dockerised backend services.

## 6. General Best Practices

-   **Database Connection Management**: When using `IDbConnection` (especially with Dapper), always ensure the connection is in an `Open` state before calling `BeginTransaction()`. While Dapper's `ExecuteAsync`/`QueryAsync` methods can often implicitly open a closed connection, `BeginTransaction()` requires an already open connection and will throw an `InvalidOperationException` if the connection is not open.
-   **Clean Code**: Adhere to SOLID principles and DRY (Don't Repeat Yourself).
-   **Error Handling**: Implement robust error handling, especially for network requests between the WPF client and backend containers.
-   **Git Best Practices**: Do not commit code, just edit it.
