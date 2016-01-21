from skeleton_extension import *
import numpy as np

# 3D POINTS
a= np.array([1,1,1])
b= np.array([1,-2,-3])
c= np.array([-4,-5,8])
d= np.array([5,-4,7])
e= np.array([-2,8,-6])
f= np.array([1,0,0])
g= np.array([-2,8,-6])
h= np.array([2,3,5])
i= np.array([1,6,-4])
print "ANGLES: ", a, b, c, d, e, f, g

print "Norm of h and norm of i:", np.linalg.norm(h), np.linalg.norm(i)
print "UNIT VECTOR: ", unit_vector(a), unit_vector(b),unit_vector(c),unit_vector(d),unit_vector(e)
print "Limb angle a,b,c :",limb_angle(a,b,c), " angle olmo: ", calculateAngle(a,b,b,c), " final right angle: ", computeAngle(a,b,b,c)
print "Limb angle b,c,d :",limb_angle(b,c,d), " angle olmo: ", calculateAngle(b,c,c,d), " final right angle: ", computeAngle(b,c,c,d)
print "Limb angle c,d,e :",limb_angle(c,d,e), " angle olmo: ", calculateAngle(c,d,d,e), " final right angle: ", computeAngle(c,d,d,e)
print "Limb angle d,e,a :",limb_angle(d,e,a), " angle olmo: ", calculateAngle(d,e,e,a), " final right angle: ", computeAngle(d,e,e,a)
print "Limb angle e,a,b :",limb_angle(e,a,b), " angle olmo: ", calculateAngle(e,a,a,b), " final right angle: ", computeAngle(e,a,a,b)
print "Limb angle btw vectors h and i :", computeAngleBtwVectors(h,i)