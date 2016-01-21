#!/usr/bin/python
## The equivalent of:
##  "Working with the Skeleton"
## in the OpenNI user guide.

"""
This shows how to identify when a new user is detected, look for a pose for
that user, calibrate the users when they are in the pose, and track them.

Specifically, it prints out the location of the users' head,
as they are tracked.
"""
# TODO: 


# Pose to use to calibrate the user
pose_to_use = 'Psi'  # 2 hands up in |_o_| form
from openni import *
#import time
from skeleton_extension import limb_angle, print_if_accurate, is_accurate, calc_accuracy, getAccuracy
import numpy as np
#import pprint

ctx = Context()
ctx.init()

# Create the user generator
user = UserGenerator()
user.create(ctx)

# Obtain the skeleton & pose detection capabilities
skel_cap = user.skeleton_cap
pose_cap = user.pose_detection_cap

# Declare the callbacks
def new_user(src, id):
    print "1/4 User {} detected. Looking for pose..." .format(id)
    pose_cap.start_detection(pose_to_use, id)

def pose_detected(src, pose, id):
    print "2/4 Detected pose {} on user {}. Requesting calibration..." .format(pose,id)
    pose_cap.stop_detection(id)
    skel_cap.request_calibration(id, True)

def calibration_start(src, id):
    print "3/4 Calibration started for user {}." .format(id)

def calibration_complete(src, id, status):
    if status == CALIBRATION_STATUS_OK:
        print "4/4 User {} calibrated successfully! Starting to track." .format(id)
        skel_cap.start_tracking(id)
    else:
        print "ERR User {} failed to calibrate. Restarting process." .format(id)
        new_user(user, id)

def lost_user(src, id):
    print "--- User {} lost." .format(id)

# Register them
user.register_user_cb(new_user, lost_user)
pose_cap.register_pose_detected_cb(pose_detected)
skel_cap.register_c_start_cb(calibration_start)
skel_cap.register_c_complete_cb(calibration_complete)

# Set the profile
skel_cap.set_profile(SKEL_PROFILE_ALL)

# Start generating
ctx.start_generating_all()
print "0/4 Starting to detect users. Press Ctrl-C to exit."

current_position = -1 # standing
accuracy0 = -1
accuracy1 = -1
accuracy2_1 = -1
accuracy2_2 = -1
accuracy3 = -1
accuracy4 = -1
accuracy5 = -1
accuracy6 = -1
posture = "initial"#"standing"#
current_posture = "initial pose"

print "starting.."
while True:
    # Update to next frame
    ctx.wait_and_update_all()
    usersID = []
    nUsers = 15

    # Extract head position of each tracked user
    for id in user.users:
        if skel_cap.is_tracking(id):

            position = 0  # standing  # right here?
            posture = "standing"

            left_hip = skel_cap.get_joint_position(id, SKEL_LEFT_HIP)
            left_knee = skel_cap.get_joint_position(id, SKEL_LEFT_KNEE)
            left_foot = skel_cap.get_joint_position(id, SKEL_LEFT_FOOT)
            right_hip = skel_cap.get_joint_position(id, SKEL_RIGHT_HIP)
            right_knee = skel_cap.get_joint_position(id, SKEL_RIGHT_KNEE)
            right_foot = skel_cap.get_joint_position(id, SKEL_RIGHT_FOOT)

            torso = skel_cap.get_joint_position(id, SKEL_TORSO)
            #waist = skel_cap.get_joint_position(id, SKEL_WAIST)

            left_hip_point = np.array([left_hip.point[0], left_hip.point[1], left_hip.point[2]])
            left_knee_point = np.array([left_knee.point[0], left_knee.point[1], left_knee.point[2]])
            left_foot_point = np.array([left_foot.point[0], left_foot.point[1], left_foot.point[2]])

            right_hip_point = np.array([right_hip.point[0], right_hip.point[1], right_hip.point[2]])
            right_knee_point = np.array([right_knee.point[0], right_knee.point[1], right_knee.point[2]])
            right_foot_point = np.array([right_foot.point[0], right_foot.point[1], right_foot.point[2]])

            torso_point = np.array([torso.point[0], torso.point[1], torso.point[2]])
            #waist_point = np.array([waist.point[0], waist.point[1], waist.point[2]])

            # print_if_accurate(limb_angle(left_hip_point, left_knee_point, left_foot_point), left_hip.confidence, left_knee.confidence, left_foot.confidence, name="Left knee")
            # print_if_accurate(limb_angle(right_hip_point, right_knee_point, right_foot_point), right_hip.confidence, right_knee.confidence, right_foot.confidence, name="Right knee")
            # print_if_accurate(limb_angle(torso_point, right_hip_point, right_knee_point), torso.confidence, right_hip.confidence, right_knee.confidence, name="Right hip")
            # print_if_accurate(limb_angle(torso_point, left_hip_point, left_knee_point), torso.confidence, left_hip.confidence, left_knee.confidence, name="Left hip")


            standing_accuracy = []   # TODO: FRANK : CONSIDERED WAIST NODE FOR BETTER ACCURACY THAN TORSO? -WAIST JOINT DOESNT WORK?
            
            if is_accurate(left_hip.confidence, left_knee.confidence, left_foot.confidence):
                standing_accuracy.append({
                    'weight': 25,
                    'angle': limb_angle(left_hip_point, left_knee_point, left_foot_point),
                    'proper_angle': 180,
                    'threshold': 25
                })
                #print "L Knee Standing angle ",limb_angle(left_hip_point, left_knee_point, left_foot_point) 
            if is_accurate(right_hip.confidence, right_knee.confidence, right_foot.confidence):
                standing_accuracy.append({
                    'weight': 25,
                    'angle': limb_angle(right_hip_point, right_knee_point, right_foot_point),
                    'proper_angle': 180,
                    'threshold': 25
                })
                #print "R Knee Standing angle ",limb_angle(right_hip_point, right_knee_point, right_foot_point) 
            if is_accurate(torso.confidence, right_hip.confidence, right_knee.confidence):
                standing_accuracy.append({
                    'weight': 25,
                    'angle': limb_angle(torso_point, right_hip_point, right_knee_point),
                    'proper_angle': 180,
                    'threshold': 20
                })
                
            if is_accurate(torso.confidence, left_hip.confidence, left_knee.confidence):
                standing_accuracy.append({
                    'weight': 25,
                    'angle': limb_angle(torso_point, left_hip_point, left_knee_point),
                    'proper_angle': 180,
                    'threshold': 20
                })
                
            #pp = pprint.PrettyPrinter(indent=4)
            #pp.pprint(sitting_accuracy)
            #print calc_accuracy(sitting_accuracy)
            if len(standing_accuracy) >= 3:
                #acc = calc_accuracy(standing_accuracy)
                accuracy0 = getAccuracy(standing_accuracy)
                # if  acc > 55:
                #     position = 0 # Standing
                #print "Accuracy standing: ", accuracy0
                if  accuracy0 >= 0.3:  #SHOULD BE AT LEAST, SEEN FROM ONE SIDE, 0.5!!
                    #position = 0 # Standing
                    posture = "standing"
                    #print "Accuracy standing >=0.5", accuracy0
                    

            #######################################################

            sitting_accuracy = []
            
            if is_accurate(left_hip.confidence, left_knee.confidence, left_foot.confidence):
                sitting_accuracy.append({
                    'weight': 20,
                    'angle': limb_angle(left_hip_point, left_knee_point, left_foot_point),
                    'proper_angle': 90,
                    'threshold': 25
                })

            if is_accurate(right_hip.confidence, right_knee.confidence, right_foot.confidence):
                sitting_accuracy.append({
                    'weight': 20,
                    'angle': limb_angle(right_hip_point, right_knee_point, right_foot_point),
                    'proper_angle': 90,
                    'threshold': 25
                })
            
            if is_accurate(torso.confidence, right_hip.confidence, right_knee.confidence):
                sitting_accuracy.append({
                    'weight': 30,
                    'angle': limb_angle(torso_point, right_hip_point, right_knee_point),
                    'proper_angle': 90,
                    'threshold': 25
                })
            if is_accurate(torso.confidence, left_hip.confidence, left_knee.confidence):
                sitting_accuracy.append({
                    'weight': 30,
                    'angle': limb_angle(torso_point, left_hip_point, left_knee_point),
                    'proper_angle': 90,
                    'threshold': 25
                })

            #pp = pprint.PrettyPrinter(indent=4)
            #pp.pprint(sitting_accuracy)
            #print calc_accuracy(sitting_accuracy)
            if len(sitting_accuracy) >= 2:
                #acc = calc_accuracy(sitting_accuracy)
                accuracy1 = getAccuracy(sitting_accuracy)
                # if  acc > 55:
                #     position = 1 # Sitting
                #print "Accuracy sitting ", accuracy1
                if  accuracy1 > 0.3:
                    position = 1 # Sitting
                    posture = "sitting"
                    
                    #print "Sitting.  Accuracy:  ",acc
                #print "\n" 

            #############################################
            knee_extension_right_accuracy = []
        
            if is_accurate(torso.confidence, right_hip.confidence, right_knee.confidence):
                knee_extension_right_accuracy.append({
                    'weight': 20,
                    'angle': limb_angle(torso_point, right_hip_point, right_knee_point),
                    'proper_angle': 90,
                    'threshold': 20
                })
            if is_accurate(torso.confidence, left_hip.confidence, left_knee.confidence):
                knee_extension_right_accuracy.append({
                    'weight': 20,
                    'angle': limb_angle(torso_point, left_hip_point, left_knee_point),
                    'proper_angle': 90,
                    'threshold': 20
                })    

            if is_accurate(right_hip.confidence, right_knee.confidence, right_foot.confidence):
                knee_extension_right_accuracy.append({
                    'weight': 30,
                    'angle': limb_angle(right_hip_point, right_knee_point, right_foot_point),
                    'proper_angle': 180,   #DOES ORIENTATION OF THE 180 DEGREES MATTER????????NO
                    'threshold': 20
                })
                #print "R Knee extension angle ",limb_angle(right_hip_point, right_knee_point, right_foot_point) 
            if is_accurate(left_hip.confidence, left_knee.confidence, left_foot.confidence):
                knee_extension_right_accuracy.append({
                    'weight': 30,
                    'angle': limb_angle(left_hip_point, left_knee_point, left_foot_point),
                    'proper_angle': 90,
                    'threshold': 25
                })
                #print "L Knee extension angle ",limb_angle(left_hip_point, left_knee_point, left_foot_point) 


            
            if len(knee_extension_right_accuracy) >= 3:
                #acc = calc_accuracy(knee_extension_right_accuracy)
                accuracy2_1 = getAccuracy(knee_extension_right_accuracy)
                # if  acc > 55:
                #     position = 2 # Knee Extension
                
                #print "Accuracy knee extension ", accuracy2_1
                if  accuracy2_1 > 0.8:
                    position = 21 # Knee Extension
                    posture = "knee extension right"
                    #
                    

            #############################################
            
            knee_extension_accuracy_left = []
        
            if is_accurate(torso.confidence, right_hip.confidence, right_knee.confidence):
                knee_extension_accuracy_left.append({
                    'weight': 20,
                    'angle': limb_angle(torso_point, right_hip_point, right_knee_point),
                    'proper_angle': 90,
                    'threshold': 20
                })
            if is_accurate(torso.confidence, left_hip.confidence, left_knee.confidence):
                knee_extension_accuracy_left.append({
                    'weight': 20,
                    'angle': limb_angle(torso_point, left_hip_point, left_knee_point),
                    'proper_angle': 90,
                    'threshold': 20
                })
            if is_accurate(right_hip.confidence, right_knee.confidence, right_foot.confidence):
                knee_extension_accuracy_left.append({
                    'weight': 30,
                    'angle': limb_angle(right_hip_point, right_knee_point, right_foot_point),
                    'proper_angle': 90,
                    'threshold': 25
                }) 
            if is_accurate(left_hip.confidence, left_knee.confidence,left_foot.confidence):
                knee_extension_accuracy_left.append({
                    'weight': 30,
                    'angle': limb_angle(left_hip_point, left_knee_point, left_foot_point),
                    'proper_angle': 180,
                    'threshold': 20
                })
                #print "L Knee extension angle ",limb_angle(left_hip_point, left_knee_point, left_foot_point) 


            
            if len(knee_extension_accuracy_left) >= 3:
                #acc = calc_accuracy(knee_extension_accuracy_left)
                accuracy2_2 = getAccuracy(knee_extension_accuracy_left)
                # if  acc > 55:
                #     position = 22 # Knee Extension
                
                #print "Accuracy knee extension ", accuracy2
                if  accuracy2_2 > 0.8:
                    position = 22 # Knee Extension
                    posture = "knee extension left"
                    #
            #############################################

            hip_abduction_right_accuracy = []   # FIRST PART SAME AS STANDING   # TODO-ADD LEFT AND RIGHT DISTINCTION
        
            if is_accurate(left_hip.confidence, left_knee.confidence, left_foot.confidence):
                hip_abduction_right_accuracy.append({
                    'weight': 24,
                    'angle': limb_angle(left_hip_point, left_knee_point, left_foot_point),
                    'proper_angle': 180,
                    'threshold': 10
                })
                #print "L Knee Standing angle ",limb_angle(left_hip_point, left_knee_point, left_foot_point) 
            if is_accurate(right_hip.confidence, right_knee.confidence, right_foot.confidence):
                hip_abduction_right_accuracy.append({
                    'weight': 24,
                    'angle': limb_angle(right_hip_point, right_knee_point, right_foot_point),
                    'proper_angle': 180,
                    'threshold': 10
                })
                #print "R Knee Standing angle ",limb_angle(right_hip_point, right_knee_point, right_foot_point)     

            if is_accurate(right_knee.confidence, torso.confidence, left_knee.confidence):
                hip_abduction_right_accuracy.append({
                    'weight': 28,
                    'angle': limb_angle(right_knee_point, torso_point, left_knee_point),
                    'proper_angle': 45,   #DOES ORIENTATION OF THE 180 DEGREES MATTER??NO
                    'threshold': 10
                })
                

            if is_accurate(torso.confidence, right_hip.confidence, right_knee.confidence):
                hip_abduction_right_accuracy.append({
                    'weight': 24,
                    'angle': limb_angle(torso_point, right_hip_point, right_knee_point),
                    'proper_angle': 140,
                    'threshold': 10
                })


            if len(hip_abduction_right_accuracy) >= 3:
                #acc = calc_accuracy(hip_abduction_right_accuracy)
                accuracy3 = getAccuracy(hip_abduction_right_accuracy)
             
                #print "Accuracy hip abduction ", accuracy3
                if  accuracy3 > 0.76:
                    position = 3 # Hip Abduction
                    posture = "hip abduction (right)"
                    print "R hip_abduction_accuracy angle ",limb_angle(right_knee_point, torso_point, left_knee_point) 
                    
 

	        #######################################################

            hip_abduction_left_accuracy = []   # FIRST PART SAME AS STANDING   # TODO-ADD LEFT AND RIGHT DISTINCTION
        
            if is_accurate(left_hip.confidence, left_knee.confidence, left_foot.confidence):
                hip_abduction_left_accuracy.append({
                    'weight': 24,
                    'angle': limb_angle(left_hip_point, left_knee_point, left_foot_point),
                    'proper_angle': 180,
                    'threshold': 10
                })
                #print "L Knee Standing angle ",limb_angle(left_hip_point, left_knee_point, left_foot_point) 
            if is_accurate(right_hip.confidence, right_knee.confidence, right_foot.confidence):
                hip_abduction_left_accuracy.append({
                    'weight': 24,
                    'angle': limb_angle(right_hip_point, right_knee_point, right_foot_point),
                    'proper_angle': 180,
                    'threshold': 10
                })
                #print "R Knee Standing angle ",limb_angle(right_hip_point, right_knee_point, right_foot_point)     

            if is_accurate(right_knee.confidence, torso.confidence, left_knee.confidence):
                hip_abduction_left_accuracy.append({
                    'weight': 28,
                    'angle': limb_angle(right_knee_point, torso_point, left_knee_point),
                    'proper_angle': 45,   #DOES ORIENTATION OF THE 180 DEGREES MATTER??NO
                    'threshold': 10
                })
                

            if is_accurate(torso.confidence, left_hip.confidence, left_knee.confidence):
                hip_abduction_left_accuracy.append({
                    'weight': 24,
                    'angle': limb_angle(torso_point, left_hip_point, left_knee_point),
                    'proper_angle': 140,
                    'threshold': 10
                })


            if len(hip_abduction_left_accuracy) >= 3:
                #acc = calc_accuracy(hip_abduction_left_accuracy)
                accuracy4 = getAccuracy(hip_abduction_left_accuracy)
             
                #print "Accuracy hip abduction left ", accuracy4
                if  accuracy4 > 0.76:
                    position = 4 # Hip Abduction
                    posture = "hip abduction (left)"
                    print "L hip_abduction_accuracy angle ",limb_angle(right_knee_point, torso_point, left_knee_point) 

            ############################################
            ############################################
            ############################################
                    

            # Select the max accuracy value move (good for real time and fast movement detection) or override current position 
            # (checkings need to be done in sequential order from more general to more specific, but specificity may be vague sometimes)
	    
            # if position != current_position:
            #     current_position = position
            #     if current_position == 0:   # IS =0?
            #         #print "************** DETECTED: Standing.  Accuracy: ", accuracy0
            #         print "************** DETECTED: Standing.  Accuracy: {}" .format(accuracy0)
            #     elif current_position == 1:
            #         print "************** DETECTED: Sitting.  Accuracy: ", accuracy1
            #         #print "************** DETECTED: Sitting2.  Accuracy: {}" .format(accuracy1)
            #     elif current_position is 2:
            #         print "************** DETECTED: Knee Extension.  Accuracy: ", accuracy2
            #     elif current_position == 3:
            #         print "************** DETECTED: Hip Abduction.  Accuracy: ", accuracy3
            #     elif current_position == 4:
            #         print "************** DETECTED: Hip Extension.  Accuracy:  ", accuracy4
            #     print "\n"

            if posture != current_posture:
                current_posture = posture
                if current_posture == "standing"  and accuracy0 >-1:   
                    print "************** DETECTED: Standing.  Accuracy: ", accuracy0
                    #print "************** DETECTED: Standing.  Accuracy: {}" .format(accuracy0)
                elif current_posture == "sitting":
                    print "************** DETECTED: Sitting.  Accuracy: ", accuracy1
                    #print "************** DETECTED: Sitting2.  Accuracy: {}" .format(accuracy1)
                elif current_posture == "knee extension right":
                    print "************** DETECTED: Right Knee Extension.  Accuracy: ", accuracy2_1
                elif current_posture == "knee extension left":
                    print "************** DETECTED: Left Knee Extension.  Accuracy: ", accuracy2_2
                elif current_posture == "hip abduction (right)":
                    print "************** DETECTED: Right Hip Abduction.  Accuracy: ", accuracy3
                elif current_posture == "hip abduction (left)":
                    print "************** DETECTED: Left Hip Abduction.  Accuracy: ", accuracy4
                elif current_posture == "hip extension (right)":
                    print "************** DETECTED: Right Hip Extension.  Accuracy: ", accuracy5
                elif current_posture == "hip extension (left)":
                    print "************** DETECTED: Left Hip Extension.  Accuracy: ", accuracy6
                print "\n"

#ctx.shutdown() 

#ctx.stop_generating_all()
#sys.exit(0)

            # print "  {}: l_hip at ({loc[0]}, {loc[1]}, {loc[2]}) [{conf}]" .format(id, loc=left_hip.point, conf=left_hip.confidence)
            # print "  {}: l_knee at ({loc[0]}, {loc[1]}, {loc[2]}) [{conf}]" .format(id, loc=left_knee.point, conf=left_knee.confidence)
            # print "  {}: l_foot at ({loc[0]}, {loc[1]}, {loc[2]}) [{conf}]" .format(id, loc=left_foot.point, conf=left_foot.confidence)
            # time.sleep(1)

            # head_ori = skel_cap.get_joint_orientation(id, SKEL_HEAD)
            # print "  {}: head at ({loc[0]}, {loc[1]}, {loc[2]}) [{conf}]" .format(id, loc=head.point, conf=head.confidence)
            # print head_ori.matrix
