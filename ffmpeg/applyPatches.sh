hash git &> /dev/null
if [ $? -eq 1 ]; then
    echo "ERROR : git required but not found"
	exit 1
fi

# Copy patches into ffmpeg.2.4 directory
# They'll be removed after the patch has been applied
echo "Preparing patches ..."
cp patches/*.patch ffmpeg.2.4

pushd ffmpeg.2.4

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
