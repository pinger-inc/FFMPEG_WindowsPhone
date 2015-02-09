hash git &> /dev/null
if [ $? -eq 1 ]; then
    echo "ERROR : git required but not found"
	exit 1
fi

if [ -d  "ffmpeg.2.4" ]; then
	echo "ERROR : ffmpeg.2.4 has already been installed."
	echo "To re-install, you should save any local changes first and then rm -rf ffmpeg.2.4"
	exit 1
fi

# Extract ffmpeg winrt project source into ffmpeg.2.4
echo "Extracting ffmpeg.2.4 ..."
tar -xzf ffmpeg.2.4.tar

# Copy patches into ffmpeg.2.4 directory
# They'll be removed after the patch has been applied
echo "Preparing patches ..."
cp patches/*.patch ffmpeg.2.4

pushd ffmpeg.2.4

git init

# Create a local branch to apply and track our changes
git checkout -b "pinger_ffmpeg"

echo "Applying patches ..."

for patch_file in *.patch
do
	# Apply patch
	echo "Applying patch file $patch_file ..."
	git apply --whitespace nowarn $patch_file
	rm $patch_file

	# Commit the patched files to the local branch
	# This will make creating new patches easier
	git add *
	git commit -a -m "$patch_file"
done 

popd

echo "Done"
