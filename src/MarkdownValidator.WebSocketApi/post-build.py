import sys, os, os.path, shutil

shutil.copyfile(sys.argv[1], os.path.join(sys.argv[2], 'index.html'))