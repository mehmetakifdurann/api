# 1. Aşama: Build (İnşa etme)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Proje dosyalarını kopyala
COPY *.csproj ./
RUN dotnet restore

# Geri kalan her şeyi kopyala ve build al
COPY . ./
RUN dotnet publish -c Release -o out

# 2. Aşama: Run (Çalıştırma)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Render genelde PORT çevre değişkenini atar, onu dinleyelim
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Burayı kendi proje dll isminle değiştir! (örn: MyApi.dll)
ENTRYPOINT dotnet FtpWebApiProject.dll