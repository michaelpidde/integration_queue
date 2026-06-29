FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first for layer caching on restore
COPY ["PiddeCorp.TaskConsumer/PiddeCorp.TaskConsumer.csproj", "PiddeCorp.TaskConsumer/"]
COPY ["PiddeCorp.RabbitMQ/PiddeCorp.RabbitMQ.csproj", "PiddeCorp.RabbitMQ/"]
COPY ["PiddeCorp.Integrations.Contracts/PiddeCorp.Integrations.Contracts.csproj", "PiddeCorp.Integrations.Contracts/"]

RUN dotnet restore "PiddeCorp.TaskConsumer/PiddeCorp.TaskConsumer.csproj"

# Copy everything else
COPY . .

RUN dotnet publish "PiddeCorp.TaskConsumer/PiddeCorp.TaskConsumer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PiddeCorp.TaskConsumer.dll"]