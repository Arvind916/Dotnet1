FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source
COPY NuGet.Config ./
COPY src/Fieldnotes/Fieldnotes.csproj src/Fieldnotes/
RUN dotnet restore src/Fieldnotes/Fieldnotes.csproj
COPY src/Fieldnotes/ src/Fieldnotes/
RUN dotnet publish src/Fieldnotes/Fieldnotes.csproj -c Release --no-restore -o /app/publish

FROM nginxinc/nginx-unprivileged:stable-alpine AS final
COPY deploy/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 8080
