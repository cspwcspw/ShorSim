
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Helpers;

namespace FactorLab;


public class Factorizer
{
    /// <summary>
    /// Number we are trying to factor
    /// </summary>
    public int N { get; private set; }

    /// <summary>
    /// Number of QuBits required in Reg1 - it must be big enough to hold value N*N
    /// </summary>
    public int QuBits { get; private set; }

    public int NumPossibleSuperimposedValues { get { return Reg1.Length; } }

    /// <summary>
    /// Each Reg1[i] holds the probability that i will be measured.
    /// </summary>
    public double [] Reg1 { get; private set; }

    // The choice for X.
    public int X { get; private set; }

    /// <summary>
    /// Once we've chosen a magic-X, this holds the sequence (X^i) % N) for each i.
    /// </summary>
    public List<int> ModExps { get; private set; }

    public Factorizer(int n)
    {
        this.N = n;

        if (n % 2 == 0) throw new ApplicationException("N must be odd");
        if (Utils.IsPrime(n)) throw new ApplicationException("N is prime");
        (int prime, int power) = Utils.FindPrimePower(n);
        if (power > 1) throw new ApplicationException($"{n} is a prime {prime} to the power of {power}");

        // Set up registers we'll use

        int q = Utils.GetQ(n); // This always returns a power of 2 such that  n^2 <= q < 2n^2.
  
        QuBits = Utils.BitsNeeded(q) - 1;
        Reg1 = new double[1 << QuBits];
      
    }

    ///// <summary>
    ///// This tries to find factors, and if it fails for any reason it throws an exception.
    ///// The smaller factor is always returned first. 
    ///// The caller supplies two random numbers in range [0..1) used for quantum measurements.
    ///// Explicit provision of these randoms let's us build deterministic code for testing.
    ///// </summary>
    ///// <param name="x"></param>
    ///// <param name="rand1">Used for quantum measurement of frequency counts of powers of x mod n</param>
    ///// <param name="rand2">Used for quantum measurement of result of DFT</param>
    ///// <returns></returns>
    //public (int, int) HuntForFactors(int x, double rand1, double rand2)
    //{
    //    List<(int freqCount, int val)> popularVals = FindPopularModExpValues(x);

    //    return (5, 7);

    //    //FoldCountsAndSort(popularVals);

    //    //// ToDo - try all?  or try just the first?
    //    //var chosenOne = popularVals[0];
    //    //double probabilitiesOfHitItems = ((double)popularCount) / ((double)reg1.NumPossibleSuperimposedValues);
    //    //int m = MeasureSomePeakFreqIndex(chosenOne.val, rand2);
    //    //int q = reg1.NumPossibleSuperimposedValues;

    //    //// If nothing goes wrong from here on out, we are done.

    //    //// Try to determine the period of our function, and get try to find factors of n.
    //    //// Now c =~ lambda / r for some integer lambda.  Borrowed with
    //    //// modifications from Berhnard Ohpner.
    //    //double c = (double)m / (double)q;


    //    ////Calculate the denominator of the best rational approximation
    //    ////to c with den < q.  Since c is lambda / r for some integer
    //    ////lambda, this will provide us with our guess for r, our period.
    //    //int den = Utils.denominator(c, q);

    //    ////Calculate the numerator from the denominator.
    //    //int p = (int)Math.Floor(den * c + 0.5);

    //    //Console.WriteLine($"Measured m == {m}, a rational approximation for m/q = {c} is: {p}/{den}");

    //    //// The denominator is our period, and an odd period is not useful as a result of Shor's algorithm.
    //    //// If the denominator times two is still less than q we can use that.
    //    //if (den % 2 == 1 && 2 * den < q)
    //    //{
    //    //    Console.WriteLine($"\tOdd denominators are no good. Double to {2 * p}/{2 * den} (the denominator is still < {q})");
    //    //    p = 2 * p;
    //    //    den = 2 * den;
    //    //}

    //    //if (den % 2 == 1)
    //    //{
    //    //    throw new ApplicationException($"\tOdd period found. This trial (when x={x}) fails.");
    //    //}

    //    ////Calculate candidates for possible common factors with n.
    //    //int a = (int)Utils.modexp(x + 1, (den / 2), n);        // ToDo fix types
    //    //int b = (int)Utils.modexp(x - 1, (den / 2), n);        // ToDo fix types
    //    //int factor = (int)Math.Max(Utils.GCD(n, a), Utils.GCD(n, b));


    //    //if ((factor == n || factor == 1))
    //    //{
    //    //    throw new ApplicationException($"\tFound trivial factors 1 and {n}.");

    //    //}

    //    //int coFactor = n / factor;
    //    //if (factor > coFactor)
    //    //{
    //    //    factor = coFactor;
    //    //    coFactor = n / factor;
    //    //}

    //    //return (factor, coFactor);
    //}

    public List<int> GenModExps(int x)
    {
        X = x;
        int q = Utils.GetQ(N);
        List<int> result = new();
        for (int i=0; i < q; i++)
        {
            result.Add(Utils.ModExp(x, i, N));
        }
        return result;
    }

    //public List<(int freqCount, int val)> FindPopularModExpValues(int x)
    //{
    //    int[] freqBin = new int[n];

    //    for (int i = 0; i < modexps.Length; i++)
    //    {
    //        int val = Utils.modexp(x, i, n);
    //        modexps[i] = val;
    //        freqBin[val]++;
    //    }

    //    // Instead of doing this in a pseudo-quantum register,
    //    // let's just do this bit classically.

    //    // First collect only the non-empty bins (and their values)
    //    List<(int freqCount, int val)> interestingVals = new();
    //    for (int i = 0; i < freqBin.Count(); i++)
    //    {
    //        if (freqBin[i] != 0)
    //        {
    //            interestingVals.Add(new(freqBin[i], modexps[i]));
    //        }
    //    }
    //    return interestingVals;
    //}

    //public void FoldCountsAndSort(List<(int freqCount, int val)> interestingVals)
    //{
    //    // Order the values if interest in increasing popularity / density
    //    interestingVals.Sort();
    //    // Overwrite each freqCount with its cumulative popularity up to this point
    //    int cumulativeHits = 0;
    //    for (int i = 0; i < interestingVals.Count(); i++)
    //    {
    //        (int c, int indx) = interestingVals[i];
    //        cumulativeHits += c;
    //        interestingVals[i] = (cumulativeHits, indx);
    //    }
    //    Debug.Assert(cumulativeHits == reg1.NumPossibleSuperimposedValues); // sanity cross-check, note not normalized, just integers.
    //    interestingVals.Reverse();
    //}

    //public int MeasurePopularValue(List<(int freqCount, int val)> interestingVals, double probThresh)
    //{
    //    // Now randomly measure an index according to the probability density function
    //    int randPick = (int)(probThresh * reg1.NumPossibleSuperimposedValues);
    //    // Search in the bins, bypassing those whose cumulative probability is less than randPick
    //    var chosenElem = interestingVals.Find(elem => { return elem.freqCount >= randPick; });

    //    // My code may have a bug.  I think if it has 10 choices all equally probable, it will
    //    // always choose the first in the list. 

    //    int measuredValue = chosenElem.val;
    //    return measuredValue;
    //}

    ///// <summary>
    ///// This tries to find factors, and if it fails for any reason it throws an exception.
    ///// The smaller factor is always returned first. 
    ///// </summary>
    ///// <param name="x"></param>
    ///// <param name="rand1">Supply a random double in range [0..1).  Used for quantum measurement of powers of x mod n</param>
    ///// <param name="rand2">Supply a random double in range [0..1).  Used for quantum measurement of result of DFT</param>
    ///// <returns></returns>
    //public int MeasureSomePeakFreqIndex(int popularValue, double probabilitiesOfHitItems, double rand2)
    //{
    //    double[] collapsedRealParts = new double[reg1.NumPossibleSuperimposedValues];
    //    for (int i = 0; i < modexps.Length; i++)
    //    {
    //        collapsedRealParts[i] = (modexps[i] == popularValue) ? probabilitiesOfHitItems : 0;
    //    }

    //    reg1.SetStateWithImpliedImaginaryZero(collapsedRealParts);

    //    // Here we do our Fourier transformation.  
    //    Utils.DFT(reg1);

    //    //Next we measure register one, due to the Fourier transform the
    //    //number we measure, m will be some multiple of lambda/r, where
    //    //lambda is an integer and r is the desired period.
    //    int m = reg1.DecMeasure(rand2);
    //    Console.WriteLine($"Step 8 complete, value of m measured as {m}.");

    //    return m;
    //}
}
