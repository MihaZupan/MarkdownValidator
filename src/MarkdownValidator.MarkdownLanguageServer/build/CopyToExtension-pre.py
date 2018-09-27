import sys, os, os.path

if os.getenv('CI') is not None:
	exit()

if sys.argv[1] != 'Release':
	exit()

for i in range(2, len(sys.argv)):
	for f in os.listdir(sys.argv[i]):
		os.unlink(os.path.join(sys.argv[i], f))