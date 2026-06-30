@echo off

podman machine init --memory 6144 --cpus 4 --disk-size 60
podman machine start
podman network create pidde-net
podman run -d --name rabbitmq --network pidde-net -p 5672:5672 -p 15672:15672 --volume rabbitmq_data:/var/lib/rabbitmq rabbitmq:3-management
podman run -d --name sqlserver --network pidde-net -p 1433:1433 -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD=YourStrong@Passw0rd mcr.microsoft.com/mssql/server:2022-latest

echo Waiting for SQL Server to be ready...

:wait_loop
sqlcmd -S localhost,1433 -U sa -P YourStrong@Passw0rd -Q "SELECT 1" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    timeout /t 2 /nobreak >nul
    goto wait_loop
)

echo SQL Server is ready.

sqlcmd -S localhost,1433 -U sa -P YourStrong@Passw0rd -Q "CREATE DATABASE TaskManager"
sqlcmd -S localhost,1433 -U sa -P YourStrong@Passw0rd -i sql\create.sql