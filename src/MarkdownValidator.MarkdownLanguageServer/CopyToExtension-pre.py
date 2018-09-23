import sys, os, os.path

for f in os.listdir(sys.argv[1]):
	os.unlink(os.path.join(sys.argv[1], f))