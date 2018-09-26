import sys, os, os.path

for i in range(1, len(sys.argv)):
	for f in os.listdir(sys.argv[i]):
		os.unlink(os.path.join(sys.argv[i], f))