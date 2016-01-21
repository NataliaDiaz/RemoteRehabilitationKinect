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
from skeleton_extension import computeAngle, is_accurate, calc_accuracy,getAccuracy #print_if_accurate, 
import numpy as np
import math
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


#sa = SceneAnalyzer()  NOT SUPPORTED IN PYOPENNI
#sa.GetMetaData(scene)
#sa.GetFloor(plane)
#sa.get_floor(plane)
#d= DepthGenerator
#d.ConvertProjectiveToRealWorld()



while True:
    # Update to next frame
    ctx.wait_and_update_all()
    usersID = []
    nUsers = 15

    # Extract head position of each tracked user
    for id in user.users:
        if skel_cap.is_tracking(id):

            #position = 0  # standing  # right here?
            #posture = "standing"

            left_hip = skel_cap.get_joint_position(id, SKEL_LEFT_HIP)
            left_knee = skel_cap.get_joint_position(id, SKEL_LEFT_KNEE)
            left_foot = skel_cap.get_joint_position(id, SKEL_LEFT_FOOT)
            right_hip = skel_cap.get_joint_position(id, SKEL_RIGHT_HIP)
            right_knee = skel_cap.get_joint_position(id, SKEL_RIGHT_KNEE)
            right_foot = skel_cap.get_joint_position(id, SKEL_RIGHT_FOOT)
            right_shoulder = skel_cap.get_joint_position(id, SKEL_RIGHT_SHOULDER)
            left_shoulder = skel_cap.get_joint_position(id, SKEL_LEFT_SHOULDER)

            torso = skel_cap.get_joint_position(id, SKEL_TORSO)
            #waist = skel_cap.get_joint_position(id, SKEL_WAIST)   #THESE DO NOT WORK
            #center_hip = skel_cap.get_joint_position(id, SKEL_HIP_CENTER)#SKEL_CENTER_HIP)

            left_hip_point = np.array([left_hip.point[0], left_hip.point[1], left_hip.point[2]])
            left_knee_point = np.array([left_knee.point[0], left_knee.point[1], left_knee.point[2]])
            left_foot_point = np.array([left_foot.point[0], left_foot.point[1], left_foot.point[2]])

            right_hip_point = np.array([right_hip.point[0], right_hip.point[1], right_hip.point[2]])
            right_knee_point = np.array([right_knee.point[0], right_knee.point[1], right_knee.point[2]])
            right_foot_point = np.array([right_foot.point[0], right_foot.point[1], right_foot.point[2]])
            #print "Right Knee point: ", right_knee_point

            torso_point = np.array([torso.point[0], torso.point[1], torso.point[2]])

            right_shoulder_point = np.array([right_shoulder.point[0], right_shoulder.point[1], right_shoulder.point[2]])
            left_shoulder_point = np.array([left_shoulder.point[0], left_shoulder.point[1], left_shoulder.point[2]])

            #waist_point = np.array([waist.point[0], waist.point[1], waist.point[2]])

            standing_accuracy = []  
            
            if is_accurate(left_hip.confidence, left_knee.confidence, left_knee.confidence, left_foot.confidence):
                angle = computeAngle(left_hip_point, left_knee_point,left_knee_point, left_foot_point)
                standing_accuracy.append({
                    'weight': 25,
                    'angle': angle,
                    'proper_angle': 153, #Theoretical 180, in practice, average is 165 degrees
                    'threshold': 25,
                    'name': "left knee angle"
                })
                #print "L Knee Standing angle ",computeAngle(left_hip_point, left_knee_point, left_foot_point) 
            if is_accurate(right_hip.confidence, right_knee.confidence, right_knee.confidence,right_foot.confidence):
                angle = computeAngle(right_hip_point, right_knee_point, right_knee_point, right_foot_point)
                standing_accuracy.append({
                    'weight': 25,
                    'angle': angle,
                    'proper_angle': 153, #Theoretical 180, in practice, average is 165 degrees
                    'threshold': 25,
                    'name': "right knee angle"
                })
                #print "R Knee Standing angle ",computeAngle(right_hip_point, right_knee_point, right_foot_point) 
            # if is_accurate(torso.confidence, right_hip.confidence, right_hip.confidence,right_knee.confidence):
            #     angle = computeAngle(torso_point, right_hip_point,right_hip_point, right_knee_point)
            #     standing_accuracy.append({
            #         'weight': 25,
            #         'angle': angle,
            #         'proper_angle': 180,
            #         'threshold': 20
            #     })
                
            # if is_accurate(torso.confidence, left_hip.confidence, left_hip.confidence,left_knee.confidence):
            #     angle = computeAngle(torso_point, left_hip_point,left_hip_point, left_knee_point)
            #     standing_accuracy.append({
            #         'weight': 25,
            #         'angle': angle,
            #         'proper_angle': 180,
            #         'threshold': 20
            #     })
                
            if is_accurate(right_shoulder.confidence, right_hip.confidence, right_hip.confidence,right_knee.confidence):
                angle = computeAngle(right_shoulder_point, right_hip_point,right_hip_point, right_knee_point)
                standing_accuracy.append({
                    'weight': 25,
                    'angle': angle,
                    'proper_angle': 173,
                    'threshold': 20,
                    'name': "right lateral hip angle"
                })

            if is_accurate(left_shoulder.confidence, left_hip.confidence,left_hip.confidence, left_knee.confidence):
                angle = computeAngle(left_shoulder_point, left_hip_point, left_hip_point, left_knee_point)
                standing_accuracy.append({
                    'weight': 25,
                    'angle': angle,
                    'proper_angle': 173,
                    'threshold': 20,
                    'name': "left lateral hip angle"
                })

            if len(standing_accuracy) >= 3:
                #acc = calc_accuracy(standing_accuracy)
                accuracy0 = getAccuracy(standing_accuracy)
                # if  acc > 55:
                #     position = 0 # Standing
                #print "Accuracy standing: ", accuracy0, "with key angles \n", #standing_key_angles[0], "-",standing_key_angles[1], "-",standing_key_angles[2], "-",standing_key_angles[3]
                #for angle in standing_key_angles: print "  ",angle, " - "
                if  accuracy0 > 0.5 and right_foot.confidence == 1 and left_foot.confidence ==1 and math.fabs(right_foot_point[1]-left_foot_point[1])<35:  #SHOULD BE AT LEAST, SEEN FROM ONE SIDE, 0.5!!
                    #position = 0 # Standing
                    posture = "standing"
                    #print "Accuracy standing ", accuracy0
                    

            #######################################################
            # kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated; // Use Seated Mode in Kinect for windows
            # To access the current setting of the pipeline, you can read the TrackingMode again.
                
            #   private void kinect_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
            #   {
            #       using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame()) // Open the Skeleton frame
            #       {
            #           if (skeletonFrame != null && this.skeletonData != null) // check that a frame is available
            #           {
            #               skeletonFrame.CopySkeletonDataTo(this.skeletonData); // get the skeletal information in this frame
            #               if (kinect.SkeletonStream.TrackingMode == SkeletonTrackingMode.Seated)


            sitting_accuracy = []

            if is_accurate(left_hip.confidence, left_knee.confidence,left_knee.confidence, left_foot.confidence):
                angle = computeAngle(left_hip_point, left_knee_point,left_knee_point, left_foot_point)
                sitting_accuracy.append({
                    'weight': 10,
                    'angle': angle,
                    'proper_angle': 90,
                    'threshold': 30,
                    'name': "left knee angle"
                })
                #print "Sitting knee angle left ",angle

            if is_accurate(right_hip.confidence, right_knee.confidence,right_knee.confidence, right_foot.confidence):
                angle = computeAngle(right_hip_point, right_knee_point,right_knee_point, right_foot_point)
                sitting_accuracy.append({
                    'weight': 10,
                    'angle': angle,
                    'proper_angle': 90,
                    'threshold': 30,
                    'name': "right knee angle"
                })
                #print "Sitting knee angle right ",angle

            if is_accurate(torso.confidence, right_hip.confidence,right_hip.confidence, right_knee.confidence):
                angle = computeAngle(torso_point, right_hip_point,right_hip_point, right_knee_point)
                sitting_accuracy.append({
                    'weight': 40,
                    'angle': angle,
                    'proper_angle': 90,
                    'threshold': 25,
                    'name': "right hip angle"
                })
                #print "Sitting torso angle right ",angle

            if is_accurate(torso.confidence, left_hip.confidence, left_hip.confidence, left_knee.confidence):
                angle = computeAngle(torso_point, left_hip_point,left_hip_point, left_knee_point)
                sitting_accuracy.append({
                    'weight': 40,
                    'angle': angle,
                    'proper_angle': 90,
                    'threshold': 25,
                    'name': "left hip angle"
                })
                #print "Sitting torso angle left  ",angle

            # if is_accurate(right_shoulder.confidence, right_hip.confidence, right_knee.confidence):   # GIVES 8 DEGREES?????
            #     sitting_accuracy.append({
            #         'weight': 30,
            #         'angle': computeAngle(right_shoulder_point, right_hip_point,right_hip_point, right_knee_point),
            #         'proper_angle': 90,
            #         'threshold': 25
            #     })
            #     print "Sitting torso angle right ",computeAngle(right_shoulder_point,right_hip_point, right_hip_point, right_knee_point)
            #     sitting_key_angles.append(computeAngle(right_shoulder_point, right_hip_point,right_hip_point, right_knee_point))

            # if is_accurate(left_shoulder.confidence, left_hip.confidence, left_knee.confidence):
            #     sitting_accuracy.append({
            #         'weight': 30,
            #         'angle': computeAngle(left_shoulder_point, left_hip_point,left_hip_point, left_knee_point),
            #         'proper_angle': 90,
            #         'threshold': 25
            #     })
            #     print "Sitting torso angle left ",computeAngle(left_shoulder_point, left_hip_point,left_hip_point, left_knee_point)
            #     sitting_key_angles.append(computeAngle(left_shoulder_point, left_hip_point, left_hip_point, left_knee_point))


            #pp = pprint.PrettyPrinter(indent=4)
            #pp.pprint(sitting_accuracy)
            #print calc_accuracy(sitting_accuracy)
            if len(sitting_accuracy) >= 3: # WHEN SITTING, USUALLY LESS ACCURATE NODES ARE TRACKED (USUALLY LESS THAN 3 NODES)
                #acc = calc_accuracy(sitting_accuracy)
                accuracy1 = getAccuracy(sitting_accuracy)
                # if  acc > 55:
                #     position = 1 # Sitting
                #print "Accuracy sitting ", accuracy1, "with key angles \n",
                #for measurement in sitting_accuracy: print "  ",measurement['name'], " - " , measurement['angle']
                if  accuracy1 > 0.7:  #0.5 is best but prints accuracy is 0.9???
                    #position = 1 # Sitting
                    posture = "sitting"
                    
                    #print "Sitting.  Accuracy:  ",acc
                #print "\n" 

            #############################################################

            hip_abduction_right_accuracy = []   # FIRST PART SAME AS STANDING   # TODO-TRY WITH AND WITHOUT SKIRT!:P
        
            if is_accurate(left_hip.confidence, left_knee.confidence, left_knee.confidence,left_foot.confidence):
                #print "Left foot: ", left_foot_point
                angle = computeAngle(left_hip_point, left_knee_point,left_knee_point, left_foot_point)
                hip_abduction_right_accuracy.append({
                    'weight': 24,
                    'angle': angle,
                    'proper_angle': 161, #Theoretical 180, in practice, average is 165 degrees
                    'threshold': 10,
                    'name': "left knee angle"
                })
                #print "L Knee Standing angle ",computeAngle(left_hip_point, left_knee_point, left_foot_point) 
            if is_accurate(right_hip.confidence, right_knee.confidence,right_knee.confidence, right_foot.confidence):
                #print "Right foot: ", right_foot_point
                angle = computeAngle(right_hip_point, right_knee_point,right_knee_point, right_foot_point)
                hip_abduction_right_accuracy.append({
                    'weight': 24,
                    'angle': angle,
                    'proper_angle': 161, #Theoretical 180, in practice, average is 165 degrees
                    'threshold': 10,
                    'name': "right knee angle"
                })
                #print "R Knee Standing angle ",computeAngle(right_hip_point, right_knee_point, right_foot_point)     

            if is_accurate(right_knee.confidence, right_hip.confidence,right_hip.confidence, left_knee.confidence): #is_accurate(right_knee.confidence, torso.confidence, left_knee.confidence):
                angle = computeAngle(right_knee_point, right_hip_point,right_hip_point, left_knee_point)
                hip_abduction_right_accuracy.append({
                    'weight': 28,
                    'angle': angle, #torso_point, left_knee_point),
                    'proper_angle': 55,   #DOES ORIENTATION OF THE 180 DEGREES MATTER??NO
                    'threshold': 10,
                    'name': "right groin angle"  
                })
                

            if is_accurate(torso.confidence, right_hip.confidence, right_hip.confidence, right_knee.confidence):
                angle = computeAngle(torso_point, right_hip_point,right_hip_point, right_knee_point)
                hip_abduction_right_accuracy.append({
                    'weight': 24,
                    'angle': angle,
                    'proper_angle': 157,
                    'threshold': 10,
                    'name': "right lateral hip angle"
                })


            # if len(hip_abduction_right_accuracy) >= 3:
            #     #acc = calc_accuracy(hip_abduction_right_accuracy)
            #     accuracy3 = getAccuracy(hip_abduction_right_accuracy)
             
            #     print "Accuracy hip abduction right: ", accuracy3, "with key angles \n",
            #     for measurement in hip_abduction_right_accuracy: print "  ",measurement['name'], " - " , measurement['angle']
            #     print "Left foot:  ", left_foot_point
            #     print "Right foot: ", right_foot_point, "and difference", math.fabs(right_foot_point[1]-left_foot_point[1])
            #     if  accuracy3 > 0.7 and right_foot.confidence == 1 and left_foot.confidence ==1 and math.fabs(right_foot_point[1]-left_foot_point[1])>200:
            #         print "-------------------------------------------------------------------------------------------------------------------------------------------------BINGO RIGHT!!!"
            #         #position = 3 # Hip Abduction
            #         posture = "hip abduction right"
            #         #print "R hip_abduction_accuracy angle ",computeAngle(right_knee_point, torso_point, left_knee_point) 
                    
 

	        #######################################################

            # v.12 acc 1.0
            # Accuracy hip_abduction_left  1.0 with key angles 
            #    left knee angle  -  163.639246456
            #    right knee angle  -  159.309889957
            #    left groin angle  -  55.0174343812
            #    left lateral hip angle  -  157.960316595
            # Left foot:   [  -80.04658508 -1161.28417969  2493.7746582 ]
            # Right foot:  [  401.18539429  -938.86987305  2305.64208984] and difference -938.869873047 -1161.28417969
            #************** DETECTED: Left Hip Abduction.  Accuracy:  1.0
            # v13
            # Accuracy hip_abduction_left  0.0 with key angles 
            # right knee angle  -  173.073754387
            # left groin angle  -  69.6999716408
            # left lateral hip angle  -  171.309600828
            # Left foot:   [ -236.46589661 -1246.18688965  2297.43457031]
            # Right foot:  [  483.17462158  -811.05029297  2336.04443359] and difference 435.13659668

               # Accuracy hip_abduction_left  0.76 with key angles 
               # left knee angle  -  168.195249763
               # right knee angle  -  158.836319699
               # left groin angle  -  63.731199801
               # left lateral hip angle  -  171.945575069
               #  Left foot:   [ -249.12213135 -1159.71899414  2287.38574219]
               #  Right foot:  [  370.08258057  -873.88439941  2306.44775391] and difference 285.834594727

            hip_abduction_left_accuracy = []   # FIRST PART SAME AS STANDING   # TODO-ADD LEFT AND RIGHT DISTINCTION
        
            if is_accurate(left_hip.confidence, left_knee.confidence, left_knee.confidence, left_foot.confidence):
                #print "Left foot:  ", left_foot_point
                angle= computeAngle(left_hip_point, left_knee_point,  left_knee_point, left_foot_point)
                hip_abduction_left_accuracy.append({
                    'weight': 24,
                    'angle': angle,
                    'proper_angle': 168,
                    'threshold': 10,
                    'name': "left knee angle"
                })
                #print "L Knee Standing angle ",computeAngle(left_hip_point, left_knee_point, left_foot_point) 
            if is_accurate(right_hip.confidence, right_knee.confidence,right_knee.confidence, right_foot.confidence):
                #print "Right foot: ", right_foot_point
                angle = computeAngle(right_hip_point, right_knee_point, right_knee_point, right_foot_point)
                hip_abduction_left_accuracy.append({
                    'weight': 24,
                    'angle': angle,
                    'proper_angle': 165, #Theoretical 180, in practice, average is 165 degrees
                    'threshold': 10,
                    'name': "right knee angle"
                })
                #print "R Knee Standing angle ",computeAngle(right_hip_point, right_knee_point, right_foot_point)     

            if is_accurate(right_knee.confidence, right_hip.confidence, right_hip.confidence, left_knee.confidence):#is_accurate(right_knee.confidence, torso.confidence, left_knee.confidence):
                hip_abduction_left_accuracy.append({
                    'weight': 28,
                    'angle': computeAngle(right_knee_point, right_hip_point, right_hip_point, left_knee_point),
                    'proper_angle': 67,   #DOES ORIENTATION OF THE 180 DEGREES MATTER??NO
                    'threshold': 10,
                    'name': "left groin angle"
                })                

            if is_accurate(left_shoulder.confidence, left_hip.confidence, left_hip.confidence, left_knee.confidence):
                hip_abduction_left_accuracy.append({
                    'weight': 24,
                    'angle': computeAngle(left_shoulder_point, left_hip_point, left_hip_point, left_knee_point),
                    'proper_angle': 171,
                    'threshold': 10,
                    'name': "left lateral hip angle"
                })


            # if is_accurate(torso.confidence, left_hip.confidence, left_hip.confidence, left_knee.confidence):
            #     hip_abduction_left_accuracy.append({
            #         'weight': 24,
            #         'angle': computeAngle(torso_point, left_hip_point, left_hip_point, left_knee_point),
            #         'proper_angle': 157,
            #         'threshold': 10,
            #         'name': "left lateral hip angle"
            #     })


            if len(hip_abduction_left_accuracy) >= 3:
                #acc = calc_accuracy(hip_abduction_left_accuracy)
                accuracy4 = getAccuracy(hip_abduction_left_accuracy)
             
                #print "Accuracy hip_abduction_left ", accuracy4, "with key angles \n",
                #for measurement in hip_abduction_left_accuracy: print "  ",measurement['name'], " - " , measurement['angle']
                #print "Left foot:  ", left_foot_point
                #print "Right foot: ", right_foot_point , "and difference", math.fabs(right_foot_point[1]-left_foot_point[1])
                if  accuracy4 > 0.7 and right_foot.confidence == 1 and left_foot.confidence ==1 and math.fabs(right_foot_point[1]-left_foot_point[1])>165:
                    #position = 4 # Hip Abduction
                    posture = "hip abduction left"
                    #print "L hip_abduction_accuracy angle ",computeAngle(right_knee_point, torso_point, left_knee_point) 

            ############################################
            #############################################
            knee_extension_right_accuracy = []
        
            if is_accurate(torso.confidence, right_hip.confidence, right_hip.confidence, right_knee.confidence):
                knee_extension_right_accuracy.append({
                    'weight': 20,
                    'angle': computeAngle(torso_point, right_hip_point,right_hip_point, right_knee_point),
                    'proper_angle': 90,
                    'threshold': 20,
                    'name': "right lateral hip angle"
                })
            if is_accurate(torso.confidence, left_hip.confidence, left_hip.confidence, left_knee.confidence):
                knee_extension_right_accuracy.append({
                    'weight': 20,
                    'angle': computeAngle(torso_point, left_hip_point,left_hip_point, left_knee_point),
                    'proper_angle': 90,
                    'threshold': 20,
                    'name': "left lateral hip angle"
                })    

            if is_accurate(right_hip.confidence, right_knee.confidence, right_knee.confidence, right_foot.confidence):
                knee_extension_right_accuracy.append({
                    'weight': 20,
                    'angle': computeAngle(right_hip_point, right_knee_point, right_knee_point,right_foot_point),
                    'proper_angle': 180,   #DOES ORIENTATION OF THE 180 DEGREES MATTER????????NO
                    'threshold': 20,
                    'name': "right knee angle"
                })
                #print "R Knee extension angle ",computeAngle(right_hip_point, right_knee_point, right_foot_point) 
            if is_accurate(left_hip.confidence, left_knee.confidence, left_knee.confidence, left_foot.confidence):
                knee_extension_right_accuracy.append({
                    'weight': 20,
                    'angle': computeAngle(left_hip_point, left_knee_point,left_knee_point, left_foot_point),
                    'proper_angle': 90,
                    'threshold': 25,
                    'name': "left knee angle"
                })

            if right_foot.confidence == 1 and left_foot.confidence ==1 and right_foot_point[1]-left_foot_point[1]>165:
                knee_extension_right_accuracy.append({
                    'weight': 20,
                    'angle': right_foot_point[1]-left_foot_point[1],
                    'proper_angle': 165,
                    'threshold': 10,
                    'name': "right foot higher"
                })

            
            if len(knee_extension_right_accuracy) >= 3:
                #acc = calc_accuracy(knee_extension_right_accuracy)
                accuracy2_1 = getAccuracy(knee_extension_right_accuracy)
                print " Left Knee Extension.  Accuracy: {}. Angles:" .format(accuracy2_1)
                for measurement in knee_extension_right_accuracy: print "  ",measurement['name'], " - " , measurement['angle']
                if  accuracy2_1 > 0.8:
                    posture = "knee extension right"
                    
            # ###Extending left foot (ocluded one):
            #---Left foot:   [   10.81082535 -1341.05822754  2412.46240234]
            # ---Right foot:  [  627.49755859  -943.13238525  2156.42944336] and difference 397.925842285
            #  Left Knee Extension.  Accuracy: 0.0. Angles:
            #    right lateral hip angle  -  130.848538784
            #    left lateral hip angle  -  155.25412345
            #    right knee angle  -  150.920932581
            #  Right Knee Extension.  Accuracy: 0.0. Angles:
            #    right lateral hip angle  -  130.848538784
            #    left lateral hip angle  -  155.25412345
            #    right knee angle  -  150.920932581
            # ---Left foot:   [    9.55354309 -1343.65991211  2410.62744141]
            # ---Right foot:  [  628.95593262  -951.04138184  2156.08911133] and difference 392.618530273
            #  Left Knee Extension.  Accuracy: 0.0. Angles:
            #    right lateral hip angle  -  134.500042196
            #    left lateral hip angle  -  149.169354864
            #    right knee angle  -  147.311832827
            #  Right Knee Extension.  Accuracy: 0.0. Angles:
            #    right lateral hip angle  -  134.500042196
            #    left lateral hip angle  -  149.169354864
            #    right knee angle  -  147.311832827
            # ###Extending right foot:

         #    #############################################
            
            knee_extension_accuracy_left = []
        
            if is_accurate(torso.confidence, right_hip.confidence, right_hip.confidence,right_knee.confidence):
                knee_extension_accuracy_left.append({
                    'weight': 20,
                    'angle': computeAngle(torso_point, right_hip_point,right_hip_point, right_knee_point),
                    'proper_angle': 90,
                    'threshold': 20,
                    'name': "right lateral hip angle"
                })
            if is_accurate(torso.confidence, left_hip.confidence, left_hip.confidence, left_knee.confidence):
                knee_extension_accuracy_left.append({
                    'weight': 20,
                    'angle': computeAngle(torso_point, left_hip_point,  left_hip_point,left_knee_point),
                    'proper_angle': 90,
                    'threshold': 20,
                    'name': "left lateral hip angle"
                })
            if is_accurate(right_hip.confidence, right_knee.confidence, right_knee.confidence, right_foot.confidence):
                knee_extension_accuracy_left.append({
                    'weight': 20,
                    'angle': computeAngle(right_hip_point, right_knee_point, right_knee_point,right_foot_point),
                    'proper_angle': 90,
                    'threshold': 25,
                    'name': "right knee angle"
                }) 
            if is_accurate(left_hip.confidence, left_knee.confidence,left_knee.confidence, left_foot.confidence):
                knee_extension_accuracy_left.append({
                    'weight': 20,
                    'angle': computeAngle(left_hip_point, left_knee_point,left_knee_point, left_foot_point),
                    'proper_angle': 180,
                    'threshold': 20,
                    'name': "left knee angle"
                })
                #print "L Knee extension angle ",computeAngle(left_hip_point, left_knee_point, left_foot_point) 

            if right_foot.confidence == 1 and left_foot.confidence ==1 and left_foot_point[1]-right_foot_point[1]>165:
                knee_extension_right_accuracy.append({
                    'weight': 20,
                    'angle': left_foot_point[1]-right_foot_point[1],
                    'proper_angle': 165,
                    'threshold': 10,
                    'name': "left foot higher"
                })
                #print "L Knee extension angle ",computeAngle(left_hip_point, left_knee_point, left_foot_point) 


            
            if len(knee_extension_accuracy_left) >= 3:
                #acc = calc_accuracy(knee_extension_accuracy_left)
                accuracy2_2 = getAccuracy(knee_extension_accuracy_left)
                print " Right Knee Extension.  Accuracy: {}. Angles:" .format(accuracy2_2)
                for measurement in knee_extension_accuracy_left: print "  ",measurement['name'], " - " , measurement['angle']                
                if  accuracy2_2 > 0.8:
                    #position = 22 # Knee Extension
                    posture = "knee extension left"
                    #
         #    #############################################
                    

            # Select the max accuracy value move (good for real time and fast movement detection) or override current position 
            # (checkings need to be done in sequential order from more general to more specific, but specificity may be vague sometimes)
	    
            print "---Left foot:  ", left_foot_point
            print "---Right foot: ", right_foot_point, "and difference", math.fabs(right_foot_point[1]-left_foot_point[1])

            if posture != current_posture:
                current_posture = posture
                if current_posture == "standing":# and accuracy0 >=0.3: @TODO: doesnt work, and without, threads make print detected Standing with 0 accuracy.   
                    print "************** DETECTED: Standing.  Accuracy: {}. Angles:" .format(accuracy0)
                    #print "With key angles \n",
                    for measurement in standing_accuracy: print "  ",measurement['name'], " - " , measurement['angle']
                    #print "************** DETECTED: Standing.  Accuracy: {}" .format(accuracy0)
                elif current_posture == "sitting":# and accuracy1 >0.3: 
                    print "************** DETECTED: Sitting.  Accuracy: {}. Angles:" .format(accuracy1)
                    #print "With key angles \n",
                    for measurement in sitting_accuracy: print "  ",measurement['name'], " - " , measurement['angle']
                elif current_posture == "knee extension right":
                    print "************** DETECTED: Right Knee Extension.  Accuracy: {}. Angles:" .format(accuracy2_1)
                    #print "With key angles \n",
                    for measurement in knee_extension_right_accuracy: print "  ",measurement['name'], " - " , measurement['angle']
                elif current_posture == "knee extension left":

                    print "************** DETECTED: Left Knee Extension.  Accuracy: {}. Angles:" .format(accuracy2_2)
                    #print "With key angles \n",
                    for measurement in knee_extension_left_accuracy: print "  ",measurement['name'], " - " , measurement['angle']
                elif current_posture == "hip abduction right":
                    print "************** DETECTED: Right Hip Abduction.  Accuracy: {}. Angles:" .format(accuracy3)
                    #print "With key angles \n",
                    for measurement in hip_abduction_right_accuracy: print "  ",measurement['name'], " - " , measurement['angle']
                elif current_posture == "hip abduction left":
                    print "************** DETECTED: Left Hip Abduction.  Accuracy: {}. Angles:" .format(accuracy4)
                    #print "With key angles \n",
                    for measurement in hip_abduction_left_accuracy: print "  ",measurement['name'], " - " , measurement['angle']
                elif current_posture == "hip extension right":
                    print "************** DETECTED: Right Hip Extension.  Accuracy: ", accuracy5
                elif current_posture == "hip extension left":
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
