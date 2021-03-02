
#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

  FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
  WORKDIR /app
  EXPOSE 80
  EXPOSE 443

  FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
  WORKDIR /src
  COPY ["yy.blog.fileServices.csproj", "fileser/yy.blog.fileServices/"]
  RUN dotnet restore "fileser/yy.blog.fileServices/yy.blog.fileServices.csproj"
#RUN dotnet dev-certs https --clean
#RUN dotnet dev-certs https -ep /app/publish/fileservice.pfx  -p dahai123
  RUN ls -la
  RUN ls -la ./fileser/yy.blog.fileServices/
  COPY . ./fileser/yy.blog.fileServices/
  WORKDIR "/src/fileser/yy.blog.fileServices"
  RUN dotnet build "yy.blog.fileServices.csproj" -c Release -o /app/build
  RUN dotnet dev-certs https --trust

  FROM build AS publish
  RUN dotnet publish "yy.blog.fileServices.csproj" -c Release -o /app/publish

  FROM base AS final
  WORKDIR /app
  COPY --from=publish /app/publish .
  RUN ls -la
  ENTRYPOINT ["dotnet", "yy.blog.fileServices.dll"]

