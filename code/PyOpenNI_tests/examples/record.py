#!/usr/bin/python
## The equivalent of:
##  "Recording and Playing Data"
## in the OpenNI user guide.

"""
The following code generates a depth generator, and then records it
to a file in the current working directory. This file can then be used 
for playback.
"""


from openni import *

ctx = Context()
ctx.init()

# Create a depth generator
depth = DepthGenerator()
depth.create(ctx)

# Start generating
ctx.start_generating_all()

# Create Recorder
recorder = Recorder()
recorder.create(ctx)

# Init it
recorder.destination = "tempRec.oni"

# Add depth node to recording
recorder.add_node_to_rec(depth, CODEC_16Z_EMB_TABLES)


try:
	while True:
	    # Update to next frame
	    nRetVal = ctx.wait_one_update_all(depth)

	    depthMap = depth.map

	    # Get the coordinates of the middle pixel
	    x = depthMap.width / 2
	    y = depthMap.height / 2
	    
	    # Get the pixel at these coordinates
	    pixel = depthMap[x,y]

	    print "The middle pixel is %d millimeters away." % pixel

except KeyboardInterrupt:
	# not required. just for an example of removing
	# a node from being recorded
	recorder.rem_node_from_rec(depth)

ctx.shutdown()

