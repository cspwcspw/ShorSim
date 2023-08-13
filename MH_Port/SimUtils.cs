using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ShorSim;

// Pete did some hacks here in addition to a direct translation. 

public class SimUtils
{
    //This function takes an integer input and returns true if it is a prime

    public static bool IsPrime(int n)
    {
        // We only need to look as far as the sqrt of N for a factor, and treat 2 as a special case.
        int upperBound = (int)(Math.Sqrt(n));
        if (n == 2) return true;
        if (n % 2 == 0) return false;

        for (int i = 3; i <= upperBound; i += 2)
        {
            if (n % i == 0)
            {
                return false;
            }
        }
        return true;
    }

    // This function takes an integer input and returns (p,q) if p^q == n for
    // some prime number raised to an integer power.  If it returns (_, 0) it
    // is not a prime raised to a power.
    public static (int, int) FindPrimePower(int n)
    {
        int i = 2;

        // Look for the first thing that divides into n
        int upperBound = (int)(Math.Sqrt(n)) + 1;
        while ((i <= upperBound))
        {
            if ((n % i) == 0) break;
            i++;
        }
        int j = i;

        // Are we sure j is prime now? 
        // Now solve j^p = n;  If it solves exactly, we're good

        int ub2 = (int)(Math.Log(n) / Math.Log(j)) + 1;
        for (i = 2; i <= ub2; i++)
        {
            if (Math.Pow(j, i) == n)
            {
                return (j,i);
            }
        }
        return (1,0);
    }

    //This function computes the greatest common denominator of two integers.
    //Since the modulus of a number mod 0 is not defined, we return a -1 as
    //an error code if we ever would try to take the modulus of something and
    //zero.

    // Pete thinks, let the caller beware. I'm happy to have it crash when b == 0 
 
    public static int GCD(int aInitial, int bInitial)
    {
        int a = aInitial;
        int b = bInitial;
        while (a % b != 0)
        {
            int d = a % b;
            a = b;
            b = d;
        }
        return b;
    }

    // This function takes and integer argument, and returns the size in bits
    // needed to represent that integer.
    public static int BitsNeeded(int a)
    {
        int size = 0;
        while (a != 0)
        {
            a = a >> 1;
            size++;
        }
        return size;
    }


   // q is the power of two such that n^2 <= q < 2n^2.
   // I don't understand the comment. What this does is find the next bigger power of 2
   // that is strictly bigger than n^2.    
    public static int GetQ(int n)
    {
        int nSq = n * n;
        int q = 1;
        while (q < nSq) q <<= 1;
        return q;
    }

    //This function takes three integers, x, a, and n, and returns x^a mod
    //n.  This algorithm is known as the "Russian peasant method," I
    //believe, and avoids overflow by never calculating x^a directly.
    public static int modexp(int x, int a, int n)
    {
        int value = 1;
        int tmp;
        tmp = x % n;
        while (a > 0)
        {
            if ((a & 1) == 1)
            {
                value = (value * tmp) % n;
            }
            tmp = (tmp * tmp) % n;
            a = a >> 1;
        }
        return value;
    }

    // This function finds the denominator q of the best rational
    // denominator q for approximating p / q for c with q < qmax.
    public static int denominator(double c, int qmax)
    {
        double y = c;
        double z;
        int q0 = 0;
        int q1 = 1;
        int q2 = 0;

        double eps = 0.5 / (qmax * qmax);
        while (true)
        {
            z = y - Math.Floor(y);
            if (z < eps)
            {
                return (q1);
            }
            if (z != 0)
            {
                //Can't divide by 0.
                y = 1 / z;
            }
            else
            {
                //Warning this is broken if q1 == 0, but that should never happen.
                return (q1);
            }
        
            q2 = (int)Math.Floor(y) * q1 + q0;
            if (q2 >= qmax)
            {
                return (q1);
            }
            q0 = q1;
            q1 = q2;
        }
    }


    // This function computes the discrete Fourier transformation on a register's
    // 0 -> q - 1 entries. 
    public static void DFT(QuReg reg)
    {
        // The Fourier transform maps functions in the time domain to
        // functions in the frequency domain.  Frequency is 1/period, thus
        // this Fourier transform will take our periodic register, and peak it
        // at multiples of the inverse period.  Our Fourier transformation on
        // the state a takes it to the state: q^(-.5) * Sum[c = 0 -> c = q - 1,
        // c * e^(2*Pi*i*a*c / q)].  Remember, e^ix = cos x + i*sin x.

        int q = reg.NumPossibleSuperimposedValues;

        DateTime t0 = DateTime.Now;
        Complex[] init = new Complex[q];
   
        int countProgress = 0;  // For visual feedback
        int sideSteps = 0;      // For trying to understand whether this helps

        int progressReport = (int)(q / 20);
        Console.Write($"DFT: ");

        for (int a = 0; a < q; a++)    
        { 
            Complex prob = reg.GetProb(a);

            // Matthew: This if statement helps prevent previous round off errors from propagating further.
            // Pete: thinks it is useful because it sidesteps most of the work. 
            double noiseThresh = 10E-12;
            if (prob.Magnitude > noiseThresh)
            {
                // This is the heart of the q^2 performance bottleneck. 
                for (int c = 0; c < q; c++)
                {
                    Complex tmpcomp  = new Complex(Math.Pow(q, -.5) * Math.Cos(2 * Math.PI * a * c / q),
                            Math.Pow(q, -.5) * Math.Sin(2 * Math.PI * a * c / q));
                    init[c] = init[c] + (prob * tmpcomp);
                }
            }
            else
            {
                sideSteps++;
            }
            countProgress++;
            if (countProgress == progressReport)
            {
                // Console.WriteLine($"Fourier transform, {(100 * ((double)a / (double)(q - 1))).ToString("F0")}% done!");
                Console.Write('.');
                countProgress = 0;
            }
        }
        double secs = (DateTime.Now - t0).TotalSeconds;
        double workDonePct = 100.0 - (sideSteps*100.0) / (double)q;
        Console.WriteLine($" ET={secs.ToString("F1")} secs. Only {(workDonePct).ToString("F1")}% of the positions were considered.");
        reg.SetState(init);
        reg.Normalize();
    }


    // Pete says, I want a global rng that can be explicitly seeded, so that we can write repeatable
    // unit tests.  In real life when not testing, we'll avoid the explicit seed.
    public static int PickCoPrime(Random rng, int n)
    {
        int xMax = (int)Math.Sqrt(n) + 1;
        int x = 0;
        int ub = 100; // initially try only small two-digit random numbers for x
 
        int xtries = 0;
        while (true)
        {
            x = rng.Next(3,ub-1);  // Don't allow x=0, 1 or 2, or x == ub-1
            int g = (int)SimUtils.GCD(n, x);
            Console.Write($"gcd({n},{x})={g}; ");
            if (g == 1)
            {
                Console.WriteLine($"Yay, found co-prime to try, x={x}");
                break;
            }

            // I'd like small x, but if we can't find small ones, try a bigger range of random numbers
            if (++xtries % 10 == 0)
            {
                ub *= 10;
                ub = Math.Min(ub, (int)n);
            }
        }
        return x;
    }
}
