@echo off
podman build -t taskconsumer -f PiddeCorp.TaskConsumer/Dockerfile .
podman stop taskconsumer
podman rm taskconsumer
podman run -d --name taskconsumer --network pidde-net -e ASPNETCORE_ENVIRONMENT=Production taskconsumer

podman run -d --name taskconsumer2 --network pidde-net -e ASPNETCORE_ENVIRONMENT=Production taskconsumer
podman run -d --name taskconsumer3 --network pidde-net -e ASPNETCORE_ENVIRONMENT=Production taskconsumer
podman run -d --name taskconsumer4 --network pidde-net -e ASPNETCORE_ENVIRONMENT=Production taskconsumer
podman run -d --name taskconsumer5 --network pidde-net -e ASPNETCORE_ENVIRONMENT=Production taskconsumer
podman run -d --name taskconsumer6 --network pidde-net -e ASPNETCORE_ENVIRONMENT=Production taskconsumer
podman run -d --name taskconsumer7 --network pidde-net -e ASPNETCORE_ENVIRONMENT=Production taskconsumer
podman run -d --name taskconsumer8 --network pidde-net -e ASPNETCORE_ENVIRONMENT=Production taskconsumer
podman run -d --name taskconsumer9 --network pidde-net -e ASPNETCORE_ENVIRONMENT=Production taskconsumer
podman run -d --name taskconsumer10 --network pidde-net -e ASPNETCORE_ENVIRONMENT=Production taskconsumer
