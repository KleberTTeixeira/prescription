# Web API (Prescription.Api) - build context: root of the solution folder
#   docker build -f web/backend/Dockerfile -t prescription-api .
# Connection string comes from the environment variable ConnectionStrings__PrescriptionDb.

# ---- Stage 1: build and publish ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Project files first: the restore layer is reused while dependencies do not change.
COPY business/Prescription.Domain/Prescription.Domain.csproj business/Prescription.Domain/
COPY business/Prescription.Application/Prescription.Application.csproj business/Prescription.Application/
COPY data/Prescription.Data.csproj data/
COPY web/backend/Prescription.Api.csproj web/backend/
RUN dotnet restore web/backend/Prescription.Api.csproj

COPY business/ business/
COPY data/ data/
COPY web/backend/ web/backend/
# Local bin/obj folders (copied from the developer machine) must not be reused inside the image.
RUN find . -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
RUN dotnet publish web/backend/Prescription.Api.csproj -c Release -o /app/publish

# ---- Stage 2: runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Time zone data: the API records dates in Brasília time (America/Sao_Paulo).
RUN apt-get update \
    && apt-get install -y --no-install-recommends tzdata \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=5000
EXPOSE 5000

# Non-root user provided by the .NET images.
USER $APP_UID

ENTRYPOINT ["dotnet", "Prescription.Api.dll"]
