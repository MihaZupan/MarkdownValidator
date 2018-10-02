import sys, os, os.path, shutil

if os.getenv('CI') is not None:
	exit()

shutil.copyfile(sys.argv[1], os.path.join(sys.argv[2], 'index.html'))