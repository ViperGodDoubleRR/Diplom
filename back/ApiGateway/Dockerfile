FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore "ApiGateway/ApiGateway.csproj"
RUN dotnet publish "ApiGateway/ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends netcat-openbsd \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

COPY docker/wait-for-gateway.sh /wait-for-gateway.sh
RUN sed -i 's/\r$//' /wait-for-gateway.sh && chmod +x /wait-for-gateway.sh
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["/wait-for-gateway.sh", "dotnet", "ApiGateway.dll"]
