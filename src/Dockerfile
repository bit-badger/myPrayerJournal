FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /mpj
COPY ./MyPrayerJournal/MyPrayerJournal.fsproj ./
RUN dotnet restore

COPY ./MyPrayerJournal ./
RUN dotnet publish -c Release -r linux-x64
RUN rm bin/Release/net7.0/linux-x64/publish/appsettings.*.json

FROM  mcr.microsoft.com/dotnet/aspnet:7.0-alpine as final
WORKDIR /app
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
COPY --from=build /mpj/bin/Release/net7.0/linux-x64/publish/ ./

EXPOSE 80
CMD [ "dotnet", "/app/MyPrayerJournal.dll" ]