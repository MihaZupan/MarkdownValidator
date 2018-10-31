FROM microsoft/dotnet:2.1-runtime AS runtime
WORKDIR /app
ADD src/MarkdownValidator.CI/bin/Release/netcoreapp2.1/publish/ ./
ENTRYPOINT ["dotnet","MarkdownValidator.CI.dll"]