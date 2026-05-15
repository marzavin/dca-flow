# =========================
# FRONTEND BUILD
# =========================
FROM node:22 AS frontend-build

WORKDIR /src/frontend

COPY frontend/package*.json ./
RUN npm install

COPY frontend/. ./

RUN npm run build


# =========================
# BACKEND BUILD
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build

WORKDIR /src

COPY backend/*.slnx ./backend/

COPY backend/src/DCAFlow.Web/DCAFlow.Web.csproj ./backend/src/DCAFlow.Web/
COPY backend/src/DCAFlow.Core/DCAFlow.Core.csproj ./backend/src/DCAFlow.Core/
COPY backend/src/DCAFlow.Data/DCAFlow.Data.csproj ./backend/src/DCAFlow.Data/
COPY backend/src/DCAFlow.Contracts/DCAFlow.Contracts.csproj ./backend/src/DCAFlow.Contracts/

WORKDIR /src/backend

RUN dotnet restore

WORKDIR /src

COPY backend/. ./backend/

COPY --from=frontend-build /src/frontend/dist ./backend/src/DCAFlow.Web/wwwroot

WORKDIR /src/backend/src/DCAFlow.Web

RUN dotnet publish -c Release -o /app/publish


# =========================
# RUNTIME
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=backend-build /app/publish .

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "DCAFlow.Web.dll"]
