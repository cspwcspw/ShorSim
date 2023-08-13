
using System;
using System.Diagnostics;
using System.Numerics;

namespace ShorSim;

// Pete Wentworth, August 2023
// I'm trying to understand some quantum computing ideas using a classical simulation.

// This code of Shor's algorithm on a clasical computer is transcribed and freely
// adapted / improvised / inspired / hacked from code in Matthew Hayward's paper at
// https://quantum-algorithms.herokuapp.com/299/paper/paper.html   You can download his
// code from the appendices at this link, paper and build it with a gcc compiler, and it  
// should give you everything that my port gives. I've made changes, i.e. reduced his 
// (unsinged long long int) to just use int instead, and cut some corners, thrown away his
// Complex class in favour of the built-in one in the C# libraries, questioned some of 
// his algorithms, etc. But without Matthew's code I would not have got started at all. 

// For Pete, a pre-requisite to understanding the "quantum" implementation  
// is to understand some number theory about factoring - can I even wrap 
// my head around why this approach finds factors on a classical algorithm?
// It is not a "good" classical factoring algorithm - there are better - but Shor has
// had to find factors with one hand tied behind his back: he has been forced to used an
// approach where the expensive steps can be put onto a quantum machine. 

// Here is something that might be useful for test data and an initial understanding of what is going on:
// https://blendmaster.github.io/ShorJS/#:~:text=Shor's%20Algorithm%20Simulator,algorithm%20is%20simulated%20using%20probabilities.
// n=35, force x=13, a^x mod n [0:34] = {1,13,29,27, 1,13,29,27...}
// Init quantum reg sz = 2048, prob of each is .0005 
// That transforms into reg2[] with indexes {1,13,27,29} each having probability 0.25 and all
// other superimposed values having probability 0.  In this case it randomly measures 13.
// The entanglement forces reg1 to have 0 everywhere except in indexes 1,5,9,...  p=0.0020 at each of these.
// Fourier Transformed pdf gives 4 peaks at 0, 521 1024, 1536, he chooses 1536.
// 0 should be avoided in a simulated world.
// So k*Q/R: 1536   where Q us 2048,  k/r: 0.75, as a fraction 3/4,  period 4.
//   13^(4/2) gives 169,   gcd(170, 35):5  gcd(168,35):7

// The README file for the project on GitHub describes my (sometimes confused?) thinking.  Once I got this far
// (i.e. my console program working like Matthew Hayward's one does), I stopped working on this code and
// started again from scratch in my second pass to gain some insight into what magic x values work, etc.
// That work is in FactorLab.



public class Shor
{
    static void Main(string[] args)
    {
        Random rng = new Random();   

        string greeting = @"Welcome to Pete's C# hack of Matthew Hayward's simulation of Shor's algorithm. 
There are four restrictions for Shor's algorithm: 
   1) The number to be factored (n) must be >= 15.  
   2) The number to be factored must be odd.
   3) The number must not be prime.
   4) The number must not be a prime power.
There are efficient classical methods of factoring 
any of the above numbers, or determining that they are prime.
";

        Console.WriteLine(greeting);

        while (true)
        {

            Console.WriteLine("\n========== Try factor a new number n ==============");
            int n = 0;
            Console.Write("Input odd, non-prime n (Enter, or n <= 2" +
                " quits the program)\n  n = (e.g. 15, 35, 55, 99, 143 ...) ? ");

            string? response = Console.ReadLine();
            if (string.IsNullOrEmpty(response)) break;

            if (!int.TryParse(response, out n)) break;
            if (n <= 2) break;

            if (n % 2 == 0)
            {
                Console.WriteLine($"Oops, {n} must be odd.  Try again.");
                continue;
            }

            if (SimUtils.IsPrime((int)n))
            {
                Console.WriteLine($"Oops, {n} is prime.  Try again.");
                continue;
            }

            {
                (int prime, int pow) = SimUtils.FindPrimePower(n);
                if (pow != 0)
                {
                    Console.WriteLine($"Oops, {n} is a prime power = {prime}^{pow}.  Try again.");
                    continue;
                }
            }

            // Now we must figure out how big a quantum register we need for our
            // input, n.  We must establish a quantum register big enough to hold
            // an equal superposition of all integers 0 through q - 1 where q is
            // the power of two such that n^2 <= q < 2n^2.
            int q = SimUtils.GetQ(n);
            Console.WriteLine($"\nTo factorize {n} Shor needs a quantum register to simultaneously hold all values up to {q - 1}.");
            Console.WriteLine($"This is the smallest power of 2 that is bigger or equal to n*n = {n * n}.");


            int regSz = SimUtils.BitsNeeded(q) - 1;
            QuReg reg1 = new QuReg(regSz);
            Console.WriteLine($"\nIn a quantum system we'd only need a register of {regSz} qubits. But here we've created");
            Console.WriteLine($"a list of q={q} possible values that could all be superimposed into that register.");
            Console.WriteLine($"(If you're into the multiverse thing, perhaps imagine {q} different universes...)");


            //This modex array will remember what values of q produced for x^q mod n.
            //It is necessary to retain these values for use when we collapse
            //register one after measuring register two.  In a real quantum
            //computer these registers would be entangled, and thus this extra
            //bookkeeping would not be needed at all.  The laws of quantum
            //mechanics dictate that register one would collapse as well, and
            //into a state consistent with the measured value in register two.
            int[] modexp = new int[q];

            //Console.WriteLine($"\nNow we need a random x that is co-prime to {n} ...");


            //This array holds the probability amplitudes of the collapsed state
            //of register one, after register two has been measured it is used
            //to put register one in a state consistent with that measured in
            //register two.



            // This is the second register.  It needs to be big enough to hold
            // the superposition of numbers ranging from 0 -> n - 1.

            QuReg reg2 = new QuReg(SimUtils.BitsNeeded(n - 1));

            Console.WriteLine($"\nCreated a second quantum register reg2 of {reg2.numQuBits} quBits.");
            Console.WriteLine($"It will hold a superposition of all the powers of some (x^i) % {n}");

            //This is a temporary value.
            int value = 0;

            //c is some multiple lambda of q/r, where q is q in this program,
            //and r is the period we are trying to find to factor n.  m is the
            //value we measure from register one after the Fourier
            //transformation.
            double c, m;

            //This is used to store the denominator of the fraction p / den where
            //p / den is the best approximation to c with den <= q.
            int den;

            //This is used to store the numerator of the fraction p / den where
            //p / den is the best approximation to c with den <= q.
            int p;

            //The integers e, a, and b are used in the end of the program when
            //we attempts to calculate the factors of n given the period it
            //measured.
            //Factor is the factor that we find.
            int a, b;
            int factor = -1;

            //Shor's algorithm can sometimes fail, in which case you do it
            //again. The done variable is set to 0 when the algorithm has
            //failed.  Only try a maximum number of tries.
            bool done = false;
            int tries = 0;
            while (!done)
            {
                if (++tries > 50)
                {
                    Console.WriteLine("There have been too many failures, giving up.");
                    break;
                }

                Console.WriteLine($"\n---------------------------------\nStep 5: Starting attempt number {tries}.");

                //    //Now we must pick a random integer x, coprime to n.  Numbers are
                //    //coprime when their greatest common denominator is one.  One is not
                //    //a useful number for the algorithm.

                int x = SimUtils.PickCoPrime(rng, n);

                // Populate reg1 so that all possible values have an equal probability of being measured
                reg1.SetAllValuesEquallyLikely();


                //Now we perform a modular exponentiation on the superposed
                //elements of reg 1.  That is, perform x^a mod n, but exploiting
                //quantum parallelism a quantum computer could do this in one
                //step, whereas we must calculate it once for each possible
                //measurable value in register one.  We store the result in a new
                //register, reg2, which is entangled with the first register.
                //This means that when one is measured, and collapses into a base
                //state, the other register must collapse into a superposition of
                //states consistent with the measured value in the other..  The
                //size of the result modular exponentiation will be at most n, so
                //the number of bits we will need is therefore less than or equal
                //to log2 of n.  At this point we also maintain a array of what
                //each state produced when modularly exponised, this is because
                //these registers would actually be entangled in a real quantum
                //computer, this information is needed when collapsing the first
                //register later.

                //This is a new array of probability amplitudes for our second
                //quantum register, that populated by the results of x^a mod n.

                Complex[] mdx = new Complex[reg2.NumPossibleSuperimposedValues];


                //This for loop ranges over q, and puts the value of x^a mod n in
                //modex[a].  It also increases the probability amplitude of the value
                //of mdx[x^a mod n] in our array of complex probabilities.
                Console.Write($"The first few powers of ({x}^i) % {n} are ");
                for (int i = 0; i < q; i++)
                {
                    //We must use this version of modexp instead of c++ builtins as
                    //they overflow when x^i is large.
                    int tmpval = (int)SimUtils.modexp(x, i, n);
                    modexp[i] = tmpval;
                    if (i < 100)
                    {
                        if (i % 26 == 0) Console.WriteLine();
                        Console.Write($"{tmpval}, ");
                    }
                    mdx[tmpval] = new Complex(mdx[tmpval].Real + 1, 0);
                }
                Console.WriteLine($" ...\n   This is what we want to find the period of.");
                //Set the state of register two to what we calculated it should be.
                reg2.SetState(mdx);

                //Normalize register two, so that the probability of measuring a
                //state is given by summing the squares of its probability amplitude.
                reg2.Normalize();
                Console.WriteLine("Step 6 complete");


                //Now we measure reg2. 
                try
                {
                    value = reg2.DecMeasure(rng.NextDouble());
                    Console.WriteLine($"Step 7 measured {value} (and collapsed reg2 so that this is the only candidate now).");
                }
                catch
                {
                    Console.WriteLine("Oops, unable to measure in step 7, Go to Jail, Do not pass GO, abandoning this try.");
                    continue;  // we need a fresh x
                }

                // Now we must use the information in the array modex to collapse
                // the state of register one into a state consistent with the value
                // we measured in register two.

                Complex[] collapse = new Complex[q];
                int numHits = 0;
                for (int i = 0; i < q; i++)
                {
                    if (modexp[i] == value)
                    {
                        collapse[i] = new Complex(1, 0);
                        numHits++;
                    }
                }

                //Now we set the state of register one to be consistent with what
                //we measured in state two, and normalize the probability amplitudes.
                reg1.SetState(collapse);
                reg1.Normalize();
                Console.WriteLine("Step 7 complete, Step 8 (DFT) would be quick on a quantum computer.");

                // Here we do our Fourier transformation.  
                SimUtils.DFT(reg1);


                //Next we measure register one, due to the Fourier transform the
                //number we measure, m will be some multiple of lambda/r, where
                //lambda is an integer and r is the desired period.
                try
                {
                    m = reg1.DecMeasure(rng.NextDouble());
                    Console.WriteLine($"Step 8 complete, value of m measured as {m}.");
                }
                catch
                {
                    Console.WriteLine($"Unable to measure reg1, giving up this try when x={x}.");
                    continue;  // find another x
                }

                // If we measured zero, we have gained no new information about the period, we must try again.
                if (m == 0)
                {
                    Console.WriteLine($"Reg1 measured 0, but that can't help us find a period. This trial (when x={x}) fails !");
                    continue;
                }

                // If nothing goes wrong from here on out, we are done.

                // Try to determine the period of our function, and get try to find factors of n.
                // Now c =~ lambda / r for some integer lambda.  Borrowed with
                // modifications from Berhnard Ohpner.
                c = (double)m / (double)q;

                Console.WriteLine("Steps 10 and 11 starting:");
                //Calculate the denominator of the best rational approximation
                //to c with den < q.  Since c is lambda / r for some integer
                //lambda, this will provide us with our guess for r, our period.
                den = SimUtils.denominator(c, q);

                //Calculate the numerator from the denominator.
                p = (int)Math.Floor(den * c + 0.5);  

                Console.WriteLine($"Measured m (peak index after DFT) = {m}, of q={q} possible positions.");
                Console.WriteLine($"The rational approximation for m/q = {c} is: {p}/{den}. ");

                // The denominator is our period, and an odd period is not useful as a result of Shor's algorithm.
                // If the denominator times two is still less than q we can use that.
                if (den % 2 == 1 && 2 * den < q)
                {
                    Console.WriteLine($"   Odd denominators are no good. Double the fraction to {2 * p}/{2 * den} (the denominator is still < {q})");
                    p = 2 * p;
                    den = 2 * den;
                }

                //Initialize helper variables.
                a = b = 0;
                factor = 0;

                // Failed if odd denominator.
                if (den % 2 == 1)
                {
                    Console.WriteLine($"   Odd period found. This trial (when x={x}) fails.");
                    continue;
                }
                if (den == 2)  // it is too small, and will pass a zero to GCD - double it.
                {
                    Console.WriteLine($"   Denominator 2 is too small. Double the fraction to {2 * p}/{2 * den} (the denominator is still < {q})");
                    p = 2 * p;
                    den = 2 * den;
                }


                //Calculate candidates for possible common factors with n.
                Console.WriteLine($"   Candidate period is {den}");
                a = SimUtils.modexp(x + 1, (den / 2), n);      
                b = SimUtils.modexp(x - 1, (den / 2), n);


                if (a == 0 || b == 0)
                {
                    Console.WriteLine($"   a={a} and b={b}: Our pick for x={x} is leads to a zero which cannot work.  Trying again.");
                    continue;
                }

                factor = (int)Math.Max(SimUtils.GCD(n, a), SimUtils.GCD(n, b));


                if ((factor == n || factor == 1))
                {
                    Console.WriteLine($"   Found trivial factors 1 and {n}.  Trying again.");
                    continue;
                }

                //If nothing else has gone wrong, and we got a factor we are
                //finished.  Otherwise start over.
                if (factor != 0)
                {
                    done = true;
                    string msg = $"****  Yebo! It worked! {n} has factors {factor} and {(int)n / factor}.  On try {tries}.  The magic x was {x}.  ****";
                    string box = new string('*', msg.Length);
                    Console.WriteLine(box);
                    Console.WriteLine(msg);
                    Console.WriteLine(box);
                }
                else if (!done)
                {
                    Console.WriteLine("   Found factor to be 0, error. We'll have to try again.");
                    done = false;
                }
            }
        }
    }
}
