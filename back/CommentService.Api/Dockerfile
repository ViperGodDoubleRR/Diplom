FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "CommentService.Api/CommentService.Api.csproj"
RUN dotnet publish "CommentService.Api/CommentService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends netcat-openbsd \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV WAIT_MINIO=true

COPY docker/wait-for-deps.sh /wait-for-deps.sh
RUN sed -i 's/\r$//' /wait-for-deps.sh && chmod +x /wait-for-deps.sh
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["/wait-for-deps.sh", "dotnet", "CommentService.Api.dll"]
