from openni import *
#import time
from skeleton_extension import limb_angle, print_if_accurate, is_accurate, calc_accuracy, getAccuracy


standing_accuracy = []   # TODO: FRANK : CONSIDERED WAIST NODE FOR BETTER ACCURACY THAN TORSO? -WAIST JOINT DOESNT WORK?
            
standing_accuracy.append({
        'weight': 25,
        'angle': 190,#limb_angle(left_hip_point, left_knee_point, left_foot_point),
        'proper_angle': 180,
        'threshold': 25
})

for function in 
accuracy = getAccuracy(standing_accuracy)
    # if  acc > 55:
    #     position = 0 # Standing
    #print "Accuracy standing: ", accuracy0
    if  accuracy0 >= 0.3:  #SHOULD BE AT LEAST, SEEN FROM ONE SIDE, 0.5!!
        #position = 0 # Standing
        posture = "standing"
        #print "Accuracy standing >=0.5", accuracy0
        