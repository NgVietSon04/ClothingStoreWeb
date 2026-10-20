FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Trỏ chính xác cấu trúc 2 tầng thư mục trên GitHub
COPY ["ClothingStoreWeb/ClothingStoreWeb/ClothingStoreWeb.csproj", "ClothingStoreWeb/ClothingStoreWeb/"]
RUN dotnet restore "ClothingStoreWeb/ClothingStoreWeb/ClothingStoreWeb.csproj"

COPY . .
WORKDIR "/src/ClothingStoreWeb/ClothingStoreWeb"
RUN dotnet build "ClothingStoreWeb.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ClothingStoreWeb.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ClothingStoreWeb.dll"]