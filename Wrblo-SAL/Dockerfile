# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
# For more information, please see https://aka.ms/containercompat

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
ENV CONTAINERIZED=true
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Wrblo-SAL/Wrblo.SAL.Api.csproj", "Wrblo-SAL/"]
COPY ["Wrblo.SAL.Core/Wrblo.SAL.Core.csproj", "Wrblo.SAL.Core/"]
COPY ["Wrblo.SAL.Shared/Wrblo.SAL.Shared.csproj", "Wrblo.SAL.Shared/"]
COPY ["Wrblo.SAL.Test/Wrblo.SAL.Test.csproj", "Wrblo.SAL.Test/"]
RUN dotnet restore "Wrblo-SAL/Wrblo.SAL.Api.csproj"
COPY . .
WORKDIR /src/Wrblo-SAL
RUN dotnet build "Wrblo.SAL.Api.csproj" -c "$BUILD_CONFIGURATION" --no-restore -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Wrblo.SAL.Api.csproj" -c "$BUILD_CONFIGURATION" --no-restore -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Wrblo.SAL.Api.dll"]