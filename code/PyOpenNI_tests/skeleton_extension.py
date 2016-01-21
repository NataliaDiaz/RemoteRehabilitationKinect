import numpy as np
import math
import membership

real_angle = (360/(2*np.pi))

def unit_vector(vector):
    """ Returns the unit vector of the vector.  """
    return vector / np.linalg.norm(vector)


def angle_between_vectors(v1, v2):
    """ Returns the angle in radians between vectors 'v1' and 'v2'::

            >>> angle_between((1, 0, 0), (0, 1, 0))
            1.5707963267948966
            >>> angle_between((1, 0, 0), (1, 0, 0))
            0.0
            >>> angle_between((1, 0, 0), (-1, 0, 0))
            3.141592653589793
    """
    v1_u = unit_vector(v1)
    v2_u = unit_vector(v2)
    #print "Unit vectors:", v1[0],v1[1],v1[2],"---", v2[0],v2[1],v2[2], "---", v1_u[0],v1_u[1],v1_u[2],"---", v2_u[0],v2_u[1],v2_u[2]
    angle = np.arccos(np.dot(v1_u, v2_u))
    if math.isnan(angle):
        if (v1_u == v2_u).all():
            return 0.0
        else:
            return np.pi
    return angle

def angle_between_vectors2(v1, v2):
    """ Returns the angle in radians between vectors 'v1' and 'v2'::

            >>> angle_between((1, 0, 0), (0, 1, 0))
            1.5707963267948966
            >>> angle_between((1, 0, 0), (1, 0, 0))
            0.0
            >>> angle_between((1, 0, 0), (-1, 0, 0))
            3.141592653589793
    """
    angle = np.arctan(np.dot(v1, v2))
    return angle #(angle*180)/np.pi

def points_to_vector(list1, list2):
    #difference = list1-list2
    #print "Lists to difference vector: ", list1, list2#, difference
    return list2-list1

def limb_angle(limb1, limb2, limb3):
    v1 = points_to_vector(limb2,limb1)
    v2 = points_to_vector(limb2, limb3)
    #print "List1, 2 and Difference ", limb2, limb1, v1
    return np.degrees(angle_between_vectors(v1, v2))

def calculateAngle(p1Ini, p1End, p2Ini, p2End):  # OLMO
    v1 = points_to_vector(p1Ini,p1End)
    v2 = points_to_vector(p2Ini, p2End)
    return np.degrees(angle_between_vectors2(v1, v2))

def computeAngle(p1Ini, p1End, p2Ini, p2End):
    v1 = points_to_vector(p1End,p1Ini)
    v2 = points_to_vector(p2Ini,p2End)
    return computeAngleBtwVectors(v1, v2)

def computeAngleBtwVectors(v1, v2):
    # cos theta = dotProduct(a,b)/(|a||b|)
    cosAngle= np.dot(v1,v2)/(np.linalg.norm(v1)* np.linalg.norm(v2))
    return np.degrees(np.arccos(cosAngle))


def print_if_accurate(angle, limb1_conf, limb2_conf, limb3_conf,name=""):
    if limb1_conf+limb2_conf+limb3_conf == 3:
        print name,':',angle

# def is_accurate(limb1_conf, limb2_conf, limb3_conf):
#     if limb1_conf+limb2_conf+limb3_conf == 3:
#         return True
#     else:
#         return False

def is_accurate(limb1_conf, limb2_conf, limb3_conf, limb4_conf):
    if limb1_conf+limb2_conf+limb3_conf+limb4_conf == 4:
        return True
    else:
        return False

def getAccuracyOld(listOfActionsDetected):
    #listOfWeights = []
    #differentWeights = 0
    partialAccuracy = 0.0
    for action in listOfActionsDetected:
        if action['angle']>(action['proper_angle']-action['threshold']) and action['angle']<(action['proper_angle']+action['threshold']):
            #listOfWeights.append(action['weight'])
            #different_weights += 1
            #At the moment only using full confidence measurements, TODO: expand to fuzzy movements
            partialAccuracy += action['weight']
    return partialAccuracy/100.0

def getAccuracy(listOfActionsDetected):
    #listOfWeights = []
    #differentWeights = 0
    partialAccuracy = 0.0

    for action in listOfActionsDetected:
        if action['angle']>(action['proper_angle']-action['threshold']) and action['angle']<(action['proper_angle']+action['threshold']):
            #listOfWeights.append(action['weight'])
            #different_weights += 1
            #At the moment only using full confidence measurements, TODO: expand to fuzzy movements
            # a = action['proper_angle'] - action['threshold']
            # b = action['proper_angle'] - (action['threshold']*3/4)
            # c = action['proper_angle'] + (action['threshold']*3/4)
            # d = action['proper_angle'] + action['threshold']  
            a = action['proper_angle'] - (action['threshold']*5/4)
            b = action['proper_angle'] - action['threshold']
            c = action['proper_angle'] + action['threshold']
            d = action['proper_angle'] + (action['threshold']*5/4)           
            #membershipFunction = membership.RectangularMF([a,d]) 
            membershipFunction = membership.TrapezoidalMF([a,b,c,d])
            #print "trapezoidal_mf evaluated to: {}" .format(trapezoidal_mf.Evaluate(action['angle']))
            result = membershipFunction.Evaluate(action['angle'])
            partialAccuracy += (action['weight']* result)
            #print "Evaluating Trapez. MF (with params {}-{}-{}-{}) for angle {} returned {}".format(a,b,c,d, action['angle'], result)
            #print "Evaluating Rectangular MF (with params {}-{}) for angle {} returned {}".format(a,d, action['angle'], result)
    return partialAccuracy/100.0

def calc_accuracy(value_list):
    weights = {}
    different_weights = 0
    total_weight = 0
    accuracy = 0

    for entry in value_list:
        if entry['angle']<entry['proper_angle']-entry['threshold'] or entry['angle']>entry['proper_angle']+entry['threshold']:
            if weights.get(entry['weight'], False):
                weights[entry['weight']].append(50)
            else:
                weights[entry['weight']] = [50]
                total_weight += entry['weight']
                different_weights += 1
        else:

            if weights.get(entry['weight'], False):
                weights[entry['weight']].append((100-(50/entry['threshold'])*(np.absolute(entry['proper_angle']-entry['angle']))))
            else:
                weights[entry['weight']] = [100-(50/entry['threshold'])*(np.absolute(entry['proper_angle']-entry['angle']))]
                total_weight += entry['weight']
                different_weights += 1


    for k,v in weights.iteritems():
        if not different_weights == 1:
            accuracy += (sum(v)/len(v))*(k/100.)
        else:
            accuracy += (sum(v)/len(v))

    if total_weight > 100:
        raise Exception

    return accuracy

"""
sitting_accuracy = [
    {'weight': 70,
     'angle': 78,
     'proper_angle': 90,
     'threshold': 10},
    {'weight': 70,
     'angle': 93,
     'proper_angle': 90,
     'threshold': 10},
    {'weight': 30,
     'angle': 184,
     'proper_angle': 90,
     'threshold': 15},
    {'weight': 30,
     'angle': 97,
     'proper_angle': 90,
     'threshold': 15},
    ]

print calc_accuracy(sitting_accuracy)
"""


#hip = np.array([52.9139251709, -240.605789185, 2008.12695312])
#knee = np.array([49.5384559631, -602.490844727, 2049.76586914])
#foot = np.array([177.358627319, -875.845947266, 2139.63818359])

#hip_knee = hip-knee
#hip_knee_l = np.linalg.norm(hip-knee)

#knee_foot = knee-foot
#knee_foot_l = np.linalg.norm(knee-foot)

#angle = np.dot(u,v)/np.norm(u)/np.norm(v)
#angle = angle_between(hip_knee, knee_foot)

#print hip_knee
#print hip_knee_l

#print knee_foot
#print knee_foot_l

#print angle





