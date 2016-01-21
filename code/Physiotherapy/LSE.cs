using System;
using System.Object;

namespace Kinect.Toolbox
{
    public class LSE
    {
        public LSE()
        {
        }

     //In this post I'll show you code on how to take the resulting LU jagged array and P array we got and solve Ax = b
        //Given P, L, U, and b LUPSolve solves for x by combining forward and back substitution. 
        //Here is the code in C#:

    /*
    * Given L,U,P and b solve for x.
    * Input the L and U matrices as a single matrix LU.
    * Return the solution as a double[].
    * LU will be a n+1xm+1 matrix where the first row and columns are zero.
    * This is for ease of computation and consistency with Cormen et al.
    * pseudocode.
    * The pi array represents the permutation matrix.
    * */
    public static double[] LUPSolve(double[][] LU, int[] pi, double[] b)
    {
        int n = LU.Length-1;
        double[] x = new double[n+1];
        double[] y = new double[n+1];
        double suml = 0;
        double sumu = 0;
        double lij = 0;

        /*
        * Solve for y using formward substitution
        * */
        for (int i = 0; i <= n; i++)
        {
            suml = 0;
            for (int j = 0; j <= i - 1; j++)
            {
                /*
                * Since we've taken L and U as a singular matrix as an input
                * the value for L at index i and j will be 1 when i equals j, not LU[i][j], since
                * the diagonal values are all 1 for L.
                * */
                if (i == j)
                {
                    lij = 1;
                }
                else
                {
                    lij = LU[i][j];
                }
                suml = suml + (lij * y[j]);
            }
            y[i] = b[pi[i]] - suml;
        }
        //Solve for x by using back substitution
        for (int i = n; i >= 0; i--)
        {
            sumu = 0;
            for (int j = i + 1; j <= n; j++)
            {
                sumu = sumu + (LU[i][j] * x[j]);
            }
            x[i] = (y[i] - sumu) / LU[i][i];
        }
        return x;
    }
    }

    //In this blog post I'll begin the first in a two part post about solving a system of linear equations. It is also one in a series I hope to do on matrix operations. In this post I'll show and explain some code for LUP decomposition.
    //In LUP decomposition we want to find three n x n matrices L, U, and P such that 
    //PA = LU 
    //where 
    //L is a unit lower-triangular matrix
    //U is an upper-trangular matrix
    //P is a permutation matrix
    //We use a process known as Gaussian elimination to create an LU decomposition.

    //Here's the code for LUP decomposition of a matrix. In the next post I'll show you how use the results of LUPDecomposition to solve a system of linear equations.

    /*
    * Perform LUP decomposition on a matrix A.
    * Return L and U as a single matrix(double[][]) and P as an array of ints.
    * We implement the code to compute LU "in place" in the matrix A.
    * In order to make some of the calculations more straight forward and to 
    * match Cormen's et al. pseudocode the matrix A should have its first row and first columns
    * to be all 0.
    * */
    public static Tuple<double[][], int[]> LUPDecomposition(double[][] A)
    {
        int n = A.Length-1;
        /*
        * pi represents the permutation matrix.  We implement it as an array
        * whose value indicates which column the 1 would appear.  We use it to avoid 
        * dividing by zero or small numbers.
        * */
        int[] pi = new int[n+1];
        double p = 0;
        int kp = 0;
        int pik = 0;
        int pikp = 0;
        double aki = 0;
        double akpi = 0;
            
        //Initialize the permutation matrix, will be the identity matrix
        for (int j = 0; j <= n; j++)
        {
            pi[j] = j;
        }

        for (int k = 0; k <= n; k++)
        {
            /*
            * In finding the permutation matrix p that avoids dividing by zero
            * we take a slightly different approach.  For numerical stability
            * We find the element with the largest 
            * absolute value of those in the current first column (column k).  If all elements in
            * the current first column are zero then the matrix is singluar and throw an
            * error.
            * */
            p = 0;
            for (int i = k; i <= n; i++)
            {
                if (Math.Abs(A[i][k]) > p)
                {
                    p = Math.Abs(A[i][k]);
                    kp = i;
                }
            }
            if (p == 0)
            {
                throw new Exception("singular matrix");
            }
            /*
            * These lines update the pivot array (which represents the pivot matrix)
            * by exchanging pi[k] and pi[kp].
            * */
            pik = pi[k];
            pikp = pi[kp];
            pi[k] = pikp;
            pi[kp] = pik;
                
            /*
            * Exchange rows k and kpi as determined by the pivot
            * */
            for (int i = 0; i <= n; i++)
            {
                aki = A[k][i];
                akpi = A[kp][i];
                A[k][i] = akpi;
                A[kp][i] = aki;
            }

            /*
                * Compute the Schur complement
                * */
            for (int i = k+1; i <= n; i++)
            {
                A[i][k] = A[i][k] / A[k][k];
                for (int j = k+1; j <= n; j++)
                {
                    A[i][j] = A[i][j] - (A[i][k] * A[k][j]); 
                }
            }
        }
        return Tuple.Create(A,pi);
    }
}
}       
