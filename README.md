Modifies Gcode to create a wood grain pattery by changing the hotend temperature.

Usage:

- Get an stl from somewhere, slice it with your desired settings in cura.
- Start Woodgrain.exe
- Open the .gcode file you got from cura.
- Enter the minimum and max. temperature that should be used, 200/210 to 240 should be fine for most filaments.
- Click "generate preview" until there is a pattern you like, or enter a seed to generate a specific pattern. 
- The bottom left shows the seed used, you can remember this to print different parts with the same pattern later.
- Click on "Process File". For large files ( >40 Mb ) this may take a while.
- Click "Write to file" to store the resulting gcode.
- The textbox on the left allows you to check if "M104" messages were inserted after each ;LAYER comment. This gcode  message modifies the temperature
- Note that this textbox only shows 100.000 chars, but you can always open the file in a text editor if you want to see the whole thing.
- Drag this new file into Cura to view it, upload it to octoprint, or place it on your printer SD cart as usual.

Tested with Cura 3.8 and an Ender 3 Pro. 
Made this because I was not happy with other options. This works well for me, but be carefull when using it, there are almost no sanity checks in place, so it might crash on some inputs.
Requires Cura generated gcode, because cura adds lots of comments that make it easy to find layers. Not sure how other slicers do this but feel free to modify the code to suit your needs.

![Interface](https://github.com/19Lukas/WoodGrain/blob/main/Pictures/UI.PNG?raw=true "Interface")
