# ShorSim
Yet Another Implementation of Shor's Quantum Factoring Algorithm, this time in C#.

Pete Wentworth


This is my playground for trying to wrap my head around 
my first quantum computing algorithm.

Initially, most of this was just a C# port of Matthew Hayward's code in his paper at https://quantum-algorithms.herokuapp.com/299/paper/paper.html


Here is something that might be useful for test data: https://blendmaster.github.io/ShorJS/


Pete's disclaimer: I might have things wrong, so do be careful
about trusting what I say here.

Shor and Matthew represented the "physicist's view" of the world in 
their pseudocode / code.
Because models of quantum mechanics use (normalized)
complex numbers to represent and derive equations 
and entanglement, there is a code bias towards programming the simulation
with complex math, random quantum register 
measurement, measurement that collapses the quantum state, etc. 
All this is essential to demonstrate the "quantum viability" of
Shor's algorithm.  But if I want to explore how the factoring 
works in Shor's algorithm, perhaps I can (and do) take shortcuts.

Suppose we're trying to factor n=35, we must randomly pick a co-prime x, 
say we pick x to be 13.  

Observation: Matthew's algorithm tries different random x picks until 
GCF(n,x) == 1, but if you ever did pick x randomly and GCD(n,x) was bigger than one,
you have already found a divisor/factor for N, so you could call off the search 
there and then.

Now we generate a sequence 
[x^0^ mod 35, x^1^ mod 35, x^2^ mod 35, x^3^ mod 35, x^4^ mod 35, ...].
The factoring algorithm requires doing this up to the next power of
two that is bigger or equal to n^2^. So on a classic computer, this step is O(n^2^),
and is not too hectic for small n. But on a quantum computer, the step can be
O(1) if all values (and outcomes of the computation) are superimposed states
of an input q-register (a big one, with enough quBits to hold all superimposed
values up to n^2^) and a smaller output q-register (enough quBits to hold superimposed
values of all results. Because of the mod n, this means enough quBits to hold 
superimposed values up to n-1.  

The computed sequence when n=35 and x is chosen as 13 turns out to be [1,13,29,27,1,13,29,27 ...]   
Now we need to look for a period (trivially 4 in this case, easy to spot). 

Shor's quantum part of the algorithm superimposes [0..n-1) (all possible periods)
into a single q-register (reg1 in Matthew's code), all initially with equal 
probability.  These numbers seed a second q-register (reg2 in Matthew's code)
which is essentially a 
frequency bin count of repetitions. So in this trivial example, we'd have lots of 
hits into the bins for 1, 13, 29, 27 and zeros in all other frequency bins.

In the quantum formulation all the bins counting hits from 0 to n-1 are superimposed
in a single q-register, and the frequency counts are represented by probabilities 
on each state becoming "the chosen one" when the q-register is finally measured.  
Non-determinstically measuring the q-register collapses the multi-state register 
into a single value (the quantum reality, so we are told), 
and entanglement means that this measurement back-propagates to the 
reg1.  Reg1 collapses (but not entirely) so that what remains has to be consistent 
with what we measured in reg2. Concretely in this example, if our measurement in
reg2 happened to measure 13 as the popularity winner in the frequency bins, 
reg1 must colllapse so that 
only the entries containing 13 are all equally probable measurements, and all other
possible values take on a zero probability of being measured.  So the positions
holding the values 13 become "spikes" in an otherwise flattened distribution. 

Then a (quantum) Discrete Fourier Transform (DFT) algorithm applied to reg1 will 
expose the frequency of the spikes.  We now measure the quantum register reg1
non-deterministically, get some candidate frequency, and from there we try to establish
the period, and if we're lucky (for the specific magic-x that we choose), that often leads us to factors. 


To be continued ...


My current distraction is to play with different magic-x values and see which
ones lead to success, which don't, and whether there is any obvious relationship
between the winners and the losers.

<!---

In our simulator this must slow exponentially.

It turns out in Shor's algorithm (from a programmer's point of view 
when writing a classical simulation), the complex math is an overkill. 
There are no instances in which the Imaginary components every become
non-zero.  So we could just use doubles. Or maybe even ints. 

And representing frequency counts as "probabilities" from which we 
non-deterministically can choose only one might be a valid quantum world
constraint, but it is not a programming constraint.   For example,
in our trivial case, when we choose magic x as 13, measuring reg2 could 
non-deterministically return any one of the values 1,13,29,27.  
The quantum world only allows one to be chosen and explored, so its
hard to ask the question "Would they all lead me to the factors?".
In my classical world, of course, its easy. Try them all, and answer
the question.  

When I began I scribbed down some magic x values that worked, and some that
failed.  So I was surprised later that previously working values now failed,
and vice-versa. Because of the non-determinism in the measurement.  


--->