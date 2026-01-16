# MassTransit.RabbitMq.Example

A .NET 8 sample solution that demonstrates using MassTransit with RabbitMQ for message-based communication. It includes:

- A common contracts project with shared messages
- A consumer service that processes messages from RabbitMQ
- A publisher sample that sends messages
- NLog-based logging to console and file

## Solution layout

- `src/MassTransit.RabbitMq.Common` — shared message contracts (e.g., `SampleEvent` with `Id` and `Description`).
- `src/MassTransit.RabbitMq.Consumer` — MassTransit host with configured consumers and RabbitMQ topology.
- `src/MassTransit.RabbitMq.Publisher` — sample publisher app to publish messages.

All projects target `.NET 8` and use C# 12.

## Prerequisites

- .NET SDK 8.0+
- RabbitMQ 3.x (management plugin recommended)
- Optional: Docker

## Quick start

### 1) Start RabbitMQ (Docker)

```bash
docker run -d --name rabbitmq \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:3-management
```

Create the `TEST` vhost and grant `guest` permissions (required by the consumer connection string):

```bash
docker exec rabbitmq rabbitmqctl add_vhost TEST
# grant full permissions to user 'guest' on vhost 'TEST'
docker exec rabbitmq rabbitmqctl set_permissions -p TEST guest ".*" ".*" ".*"
```

RabbitMQ Management UI is available at http://localhost:15672 (user: `guest`, password: `guest`).

### 2) Build the solution

```bash
dotnet restore ./src
dotnet build ./src -c Release
```

### 3) Run the consumer

```bash
dotnet run --project ./src/MassTransit.RabbitMq.Consumer/MassTransit.RabbitMq.Consumer.csproj
```

The consumer uses this connection string (configured in `Program.cs`):

```
amqp://guest:guest@localhost:5672/TEST
```

Ensure the `TEST` vhost exists (see step 1).

### 4) Run the publisher (in another terminal)

```bash
dotnet run --project ./src/MassTransit.RabbitMq.Publisher/MassTransit.RabbitMq.Publisher.csproj
```

## Messaging topology (as configured in the consumer)

- Faults exchange: `_MassTransit.RabbitMq.Example.Faults`
- `SampleEvent`
  - Message exchange: `_MassTransit.RabbitMq.Common.SampleEvent`
  - Queue: `_MassTransit.RabbitMq.Common.SampleEvent.Queue`
- `SampleDelayEvent`
  - Queue: `_MassTransit.RabbitMq.Common.SampleDelayEvent.Queue`
  - Raw JSON deserialization enabled
  - Faults for `SampleDelayEvent` published to `_MassTransit.RabbitMq.Common.SampleDelayEvent.Error`

Consumers configured:

- `SampleEventConsumer` (bound to `_MassTransit.RabbitMq.Common.SampleEvent.Queue`)
- `SampleDelayEventConsumer` (bound to `_MassTransit.RabbitMq.Common.SampleDelayEvent.Queue`)

## Logging

Both console and file logging are enabled via NLog.

- Console layout includes scopes and exceptions.
- File logs are written to `logs/app.log` (within the app working directory) with rolling archives in `logs/archives/`.

## Configuration notes

- The consumer currently configures RabbitMQ settings in code using an in-memory configuration source. To change the connection string, update `RabbitMQ:ConnectionString` in `Program.cs` of `MassTransit.RabbitMq.Consumer`.
- Default value: `amqp://guest:guest@localhost:5672/TEST`.

## Quick testing via RabbitMQ UI

You can manually publish a message to the `SampleEvent` exchange from the RabbitMQ Management UI:

- Exchange: `_MassTransit.RabbitMq.Common.SampleEvent`
- Routing key: (empty)
- Content type: `application/json`
- Payload example:

```json
{
  "Id": "b8c61362-5b41-4f5b-9c8a-7a7a5f9f3c2a",
  "Description": "Hello from UI"
}
```

If bound correctly, the message is delivered to `_MassTransit.RabbitMq.Common.SampleEvent.Queue` and processed by `SampleEventConsumer`.

## Troubleshooting

- Connection errors or `access_refused`: verify the `TEST` vhost exists and that the `guest` user has permissions.
- `PRECONDITION_FAILED` on declare/bind: ensure exchange/queue names match those configured above and that the existing topology is compatible.
- Port conflicts: make sure ports `5672` (AMQP) and `15672` (management) are available.

## License

This repository is for demonstration purposes. Apply a license of your choice for redistribution or contributions.