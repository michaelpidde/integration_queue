@echo off
podman build -t taskconsumer -f PiddeCorp.TaskConsumer/Dockerfile .
podman stop taskconsumer
podman rm taskconsumer
podman run -d --name taskconsumer --network pidde-net -e ASPNETCORE_ENVIRONMENT=Production taskconsumer
