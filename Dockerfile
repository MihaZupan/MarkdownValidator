FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

COPY src/ReferenceValidator/*.csproj ./
RUN dotnet restore

COPY src/ReferenceValidator/. ./
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.1-runtime AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "ReferenceValidator.dll"]