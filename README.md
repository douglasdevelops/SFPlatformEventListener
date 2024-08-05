# CometD Client for Salesforce Platform Events

This repository contains a C# application that uses CometD to connect to Salesforce Platform Events. It demonstrates how to authenticate using OAuth, establish a CometD connection, and listen to Salesforce events.

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [Configuration](#configuration)
- [Usage](#usage)
- [Code Overview](#code-overview)
- [License](#license)

## Features

- OAuth authentication with Salesforce
- CometD connection to Salesforce
- Listening to Salesforce Platform Events
- Simple console output for received events

## Installation

1. **Clone the repository:**

    ```bash
    git clone https://github.com/yourusername/cometd-salesforce-client.git
    cd cometd-salesforce-client
    ```

2. **Install dependencies:**

   Ensure you have .NET Core SDK installed. You can download it from [here](https://dotnet.microsoft.com/download).

3. **Build the project:**

    ```bash
    dotnet build
    ```

## Configuration

1. **Update the `App.config` file** with your Salesforce credentials and CometD channel settings:

    ```xml
    <appSettings>
        <add key="clientId" value="YOUR_CLIENT_ID" />
        <add key="clientSecret" value="YOUR_CLIENT_SECRET" />
        <add key="username" value="YOUR_SALESFORCE_USERNAME" />
        <add key="password" value="YOUR_SALESFORCE_PASSWORD" />
        <add key="securityToken" value="YOUR_SALESFORCE_SECURITY_TOKEN" />
        <add key="channel" value="YOUR_COMETD_CHANNEL" />
    </appSettings>
    ```

## Usage

1. **Run the application:**

    ```bash
    dotnet run
    ```

2. **The application will authenticate with Salesforce, establish a CometD connection, and listen for events on the specified channel.**

   - **Received events** will be printed to the console.

## Code Overview

### `OAuthTokenResponse`

A class to hold OAuth token response data.

### `OAuthExtension`

An extension to handle OAuth token injection in CometD messages.

### `MessageListener`

A class to listen for Platform Events and trigger an event when a message is received.

### `PlatformEventReciever`

A class designed to receive messages and trigger the `PlatformEventReceived` event when a message is received.

- **Methods:**
  - `ReceiveMessage(string message)`: 
    - **Description:** Accepts a message as a string and triggers the `PlatformEventReceived` event with the provided message.
    - **Parameters:** 
      - `message`: The message string to be processed.
    - **Usage Example:** 

      ```csharp
      var receiver = new PlatformEventReciever();
      receiver.PlatformEventReceived += (sender, e) => {
          Console.WriteLine(e.Message);
      };
      receiver.ReceiveMessage("Test Event Message");
      ```

### `PlatformEventReceivedEventArgs`

A class used for providing event data when a platform event is received.

- **Properties:**
  - `Message`: 
    - **Description:** Contains the received event message.
    - **Type:** `string`
    - **Usage Example:**

      ```csharp
      var eventArgs = new PlatformEventReceivedEventArgs("Test Event Message");
      Console.WriteLine(eventArgs.Message);
      ```

### `Program`

The main entry point of the application, which handles authentication, CometD connection, and event listening.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
