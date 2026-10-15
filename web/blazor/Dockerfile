# Blazor (Prescription.Blazor) - build context: root of the solution folder
#   docker build -f web/blazor/Dockerfile -t prescription-blazor .
# The Web API address comes from the environment variable ApiBaseUrl.

# ---- Stage 1: build and publish ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Project file first: the restore layer is reused while dependencies do not change.
COPY web/blazor/Prescription.Blazor.csproj web/blazor/
RUN dotnet restore web/blazor/Prescription.Blazor.csproj

COPY web/blazor/ web/blazor/
# Local bin/obj folders (copied from the developer machine) must not be reused inside the image.
RUN find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
RUN dotnet publish web/blazor/Prescription.Blazor.csproj -c Release -o /app/publish

# ---- Stage 2: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# Time zone data: "today" is taken in Brasília time (America/Sao_Paulo).
RUN apt-get update \
    && apt-get install -y --no-install-recommends tzdata \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=5100
EXPOSE 5100

# Non-root user provided by the .NET images.
USER $APP_UID

ENTRYPOINT ["dotnet", "Prescription.Blazor.dll"]
