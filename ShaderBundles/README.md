Temporary fake "web service" that allows the library to pull in new .shaderbundle files

Eventually this should be converted to a real web service, but I wanted to make sure I had something to push .shaderbundle updates
Manifest is path-based instead of describing individual shaders - not ideal, but it'll work as long as nobody changes their shaderbundle paths

If you have a .shaderbundle you'd like to be included, feel free to send a PR or DM me and i'll check it