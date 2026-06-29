### Shared network
```
podman network create pidde-net
```

### RabbitMQ Container
```
podman run -d --name rabbitmq --network pidde-net -p 5672:5672 -p 15672:15672 --volume rabbitmq_data:/var/lib/rabbitmq rabbitmq:3-management
```

### SQL Server Container
```
podman run -d --name sqlserver --network pidde-net -p 1433:1433 -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=YourStrong@Passw0rd mcr.microsoft.com/mssql/server:2022-latest
```

### Consumer Container
Run the root build.bat file to rebuild and start the consumer container which will continuously poll for messages in the task.open queue.