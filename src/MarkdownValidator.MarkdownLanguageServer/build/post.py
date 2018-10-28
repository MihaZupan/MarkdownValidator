import sys, os, os.path

if sys.argv[1] != 'Release':
	exit()

os.system("dotnet publish " + sys.argv[2] + " --no-build -c Release -o " + sys.argv[3])

for f in os.listdir(sys.argv[3]):
	if f.endswith(".dev.json"):
		os.unlink(os.path.join(sys.argv[3], f))