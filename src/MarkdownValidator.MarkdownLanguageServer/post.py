import os, os.path

outputPath = "../extensions/vscode/bin";

os.system("dotnet publish --no-build -c Release -o " + outputPath)

for f in os.listdir(outputPath):
	if f.endswith(".dev.json"):
		os.unlink(os.path.join(outputPath, f))