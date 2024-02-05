FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install Node.js
# Reference: https://github.com/nodesource/distributions/blob/master/README.md#deb
RUN curl -SLO https://deb.nodesource.com/nsolid_setup_deb.sh
RUN chmod 500 nsolid_setup_deb.sh
RUN ./nsolid_setup_deb.sh 21
RUN apt-get install nodejs -y

# Build the Application
WORKDIR /src
COPY . .
RUN dotnet restore "src/Pulse.WebApp/Pulse.WebApp.csproj"
RUN dotnet build "src/Pulse.WebApp/Pulse.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Pulse.WebApp/Pulse.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Pulse.WebApp.dll"]