using ShorSim;
using Xunit;
using Xunit.Abstractions;

namespace xTestSuite;


public class MatthewHayward
{
    private readonly ITestOutputHelper output;
    public MatthewHayward(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Prime()
    {
        Assert.True(SimUtils.IsPrime(2));
        Assert.True(SimUtils.IsPrime(3));
        Assert.True(SimUtils.IsPrime(11));

        Assert.False(SimUtils.IsPrime(16));
        Assert.True(SimUtils.IsPrime(17));
        Assert.True(SimUtils.IsPrime(101));
        Assert.True(SimUtils.IsPrime(1009));
        Assert.True(SimUtils.IsPrime(104729));
        Assert.False(SimUtils.IsPrime(1009 * 1009));
        Assert.False(SimUtils.IsPrime(1009 * 104729));
    }

    [Fact]
    public void PrimePower()
    {
        var result = SimUtils.FindPrimePower((int)(Math.Pow(17, 6)));
        Assert.Equal((17, 6), result);
        Assert.Equal((17, 7), SimUtils.FindPrimePower((int)(Math.Pow(17, 7))));
        Assert.NotEqual((15, 7), SimUtils.FindPrimePower((int)(Math.Pow(15, 7))));
        Assert.Equal(0, SimUtils.FindPrimePower((int)(Math.Pow(15, 7))).Item2);
        Assert.Equal(0, SimUtils.FindPrimePower(42).Item2);
    }


    [Fact]
    public void GCD()
    {
        Assert.Equal(1, SimUtils.GCD(99, 2));
        Assert.Equal(1, SimUtils.GCD(2, 99));
        Assert.Equal(101, SimUtils.GCD(1009 * 101, 101 * 60));
        Assert.Equal(104729, SimUtils.GCD(104729 * 101, 104729 * 42));
    }

    [Fact]
    public void BitSz()
    {
        Assert.Equal(6, SimUtils.BitsNeeded(63));
        Assert.Equal(7, SimUtils.BitsNeeded(64));

        Assert.Equal(2, SimUtils.BitsNeeded(3));
        Assert.Equal(1, SimUtils.BitsNeeded(1));
        Assert.Equal(20, SimUtils.BitsNeeded(1000000));
    }

    [Fact]
    public void GetQ()
    {
        Assert.Equal(128, SimUtils.GetQ(11));
        Assert.Equal(1024 * 1024, SimUtils.GetQ(1001));
        //    Assert.Equal(256ul * 512UL, Utils.GetQ(256));   // Check what the semantics ought to be on the edge case
        Assert.Equal(256 * 256, SimUtils.GetQ(256));
    }


    [Fact]
    public void ModExp()
    {
        Assert.Equal(4, SimUtils.modexp(2, 8, 7));
        Assert.Equal(445, SimUtils.modexp(4, 13, 497)); // wikipedia examples

    }


    [Theory]
    [InlineData(4, 0.75, 100)]
    [InlineData(50, 0.66, 100)]
    [InlineData(7, 3.14159, 100)]
    [InlineData(3044, 3.14159, 10000)]
    [InlineData(24239, 3.14159, 100000)]
    [InlineData(5, 0.8, 100)]
    [InlineData(2, 0.5, 100)]      // I saw this misbehave, telling me in Shor.c that the rational approx
    [InlineData(2, 0.49999, 100)]  // of 0.5 was 1/1.   But I can't reproduce it and stupidly don't know
    [InlineData(2, 0.50001, 100)]  // what x or n was
    public void denominator(int expected, double frac, int qmax)
    {
        int denom = SimUtils.denominator(frac, qmax);
        double numerator = frac * denom;
        output.WriteLine($"{frac} can be rationalized as approx {numerator} / {denom}");
        Assert.Equal(expected, denom);
    }

}

