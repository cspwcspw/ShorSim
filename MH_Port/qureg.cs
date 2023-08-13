
using System.Diagnostics;
using System.Security.Cryptography;
using System.Numerics;

namespace ShorSim;

public class QuReg
{
    public int numQuBits { get; }      // number of qubits in the register 
    Complex[] State = null;

    /// <summary>
    /// Called Q in other places
    /// </summary>
    public int NumPossibleSuperimposedValues  
    {
        get
        {
            return State.Length;
        }
    }

    //Default constructor.  Size is the size in bits of our register.
    //In our implementation of Shor's algorithm we will need size bits
    //to represent our value for "q" which is a number we have chosen
    //with small prime factors which is between 2n^2 and 3n^2 inclusive
    //where n is the number we are trying to factor.  We envision our
    //the description of our register of size "S" as 2^S complex
    //numbers, representing the probability of finding the register on
    //one of or 2^S base states.  Thus we use an array of size 2^S, of
    //Complex numbers.  Thus if the size of our register is 3 bits
    //array[7] is the probability amplitude of the state |1,1,1>, and
    //array[7] * Complex Conjugate(array[7]) = probability of choosing
    //that state.  We use normalized state vectors thought the
    //simulation, thus the sum of all possible states times their
    //complex conjugates is = 1.


    //Constructor.
    public QuReg(int numBits)
    {
        if (numBits > 30)
        {
            throw new ApplicationException("Pete's simulator can only handle 30 qubits");
        }
        this.numQuBits = numBits;
        int regNumVals = 1 << numBits;
        State = new Complex[regNumVals];
        for (int i = 0; i < NumPossibleSuperimposedValues; i++)
        {
            State[i] = new Complex();
        }
    }

    public QuReg(QuReg old) // Copy constructor
    {
        numQuBits = old.numQuBits;
        State = new Complex[numQuBits];
        for (int i = 0; i < numQuBits; i++)
        {
            State[i] = old.State[i];
        }
    }

    //Measures our quantum register, and returns the decimal
    //interpretation of the bit string measured. Collapse
    //the state so that the probability of measuring the measured value in
    //the future is 1, and the probability of measuring any other state is 0.

    public int DecMeasure(double tripThresh)
    {
        double a=0, b=0;
 
        for (int i = 0; i < NumPossibleSuperimposedValues; i++)
        {
            double magSq = State[i].Real * State[i].Real + State[i].Imaginary * State[i].Imaginary;
            b += magSq;
            if (b > tripThresh && tripThresh > a)
            {
                // We have just picked the i'th universe for our measurement.
                for (int j = 0; j < State.Length; j++)
                {
                    State[j] = new Complex(0, 0); // collapse all states 
                }
                State[i] = new Complex(1, 0);     // and set this one certain certain in future,
                return i;
            }
            a = b;
        }
        throw new ApplicationException("Unable to DecMeasure");
    }

    //Normalize the probability / state amplitude, this ensures that the sum of
    //the sum of the squares of all the real and imaginary components is
    //equal to one.   Pete observes that in Matthew's C++ code, the imaginary
    // parts of the complex numbers are always zero. 
    public void Normalize()
    {
        double sumSqMagnitudes = 0;
        double f, g;
        for (int i = 0; i < NumPossibleSuperimposedValues; i++)
        {
            sumSqMagnitudes += State[i].Real * State[i].Real + State[i].Imaginary * State[i].Imaginary;
        }

        double normalizingScaleFactor = 1.0 / Math.Sqrt(sumSqMagnitudes);
 
        for (int i = 0; i < State.Length; i++)
        {
            f = State[i].Real * normalizingScaleFactor;
            g = State[i].Imaginary * normalizingScaleFactor;
            State[i] = new Complex(f, g);
        }
    }

    //Sets state of the qubits using the arrays of complex amplitudes.
    public void SetState(Complex[] new_state)
    {
        Debug.Assert(new_state.Length == NumPossibleSuperimposedValues);

        for (int i = 0; i < NumPossibleSuperimposedValues; i++)
        {
            State[i] = new Complex(new_state[i].Real, new_state[i].Imaginary);
        }
    }

    //Sets state of the qubits using the arrays of real parts
    public void SetStateWithImpliedImaginaryZero(double[] new_state)
    {
        Debug.Assert(new_state.Length == NumPossibleSuperimposedValues);

        for (int i = 0; i < NumPossibleSuperimposedValues; i++)
        {
            State[i] = new Complex(new_state[i], 0);
        }
    }

    // Sets the state to a superposition of all possible states having the same probability.
    public void SetAllValuesEquallyLikely()
    {
        double prob = 1.0 / Math.Sqrt(NumPossibleSuperimposedValues);
        double sumOfProbsSq = 0;
        for (int i = 0; i < State.Length; i++)
        {
            State[i] = new Complex(prob, 0);
            sumOfProbsSq += prob * prob;
        }
        Debug.Assert(Math.Abs(sumOfProbsSq - 1.0) < 10E-10);     // ToDo - remove eventually 
    }


    //Get the probability of a given state.  This is used in the
    //discrete Fourier transformation.  In a real quantum computer such
    //an operation would not be possible, on the flip side, it would
    //also not be necessary as you could simply build a DFT gate, and
    //run your superposition through it to get the right answer.
    public Complex GetProb(int whichVal)

    {
        return (State[whichVal]);
    }
}

