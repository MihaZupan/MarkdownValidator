import os, os.path

outputPath = "../extensions/vscode/bin";

for f in os.listdir(outputPath):
	os.unlink(os.path.join(outputPath, f))