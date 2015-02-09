# How to create an FFMPEG patch

After extracting the ffmpeg.2.4 source, the pingerInstall.sh script iteratively applies and commits each patch to a local *pinger_ffmpeg* branch. This makes it easy to track the changes you have made since the last patch was installed i.e. using git status.

Once you are happy with your local changes and ready to make a patch, stage and commit your changes to the *pinger_ffmpeg* branch.

```
git add *
git commit -a -m "updated foo() method"
```
Then obtain the sha digest associated with your commit using the git log command.
```
$ git log --pretty=oneline -1
0d008bc29e94d789 updated foo() method
```
Next, create the patch file using the sha obtained from the git log command:
```
git format-patch -1 <sha>
```

Copy the resulting patch file into the *patches* directory and rename as appropriate. The file must have a .patch suffix.

Finally, commit and push your patch file to master.
