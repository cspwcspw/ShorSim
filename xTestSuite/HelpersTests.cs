using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using Helpers;

namespace xTestSuite;

public class HelpersTests
{

    private readonly ITestOutputHelper output;
    public HelpersTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Prime()
    {
        Assert.True(Utils.IsPrime(2));
        Assert.True(Utils.IsPrime(3));
        Assert.True(Utils.IsPrime(11));

        Assert.False(Utils.IsPrime(16));
        Assert.True(Utils.IsPrime(17));
        Assert.True(Utils.IsPrime(101));
        Assert.True(Utils.IsPrime(1009));
        Assert.True(Utils.IsPrime(104729));
        Assert.False(Utils.IsPrime(1009 * 1009));
        Assert.False(Utils.IsPrime(1009 * 104729));
    }

    [Fact]
    public void PrimePower()
    {
        var result = Utils.FindPrimePower((int)(Math.Pow(17, 6)));
        Assert.Equal((17, 6), result);
        Assert.Equal((17, 7), Utils.FindPrimePower((int)(Math.Pow(17, 7))));
        Assert.NotEqual((15, 7), Utils.FindPrimePower((int)(Math.Pow(15, 7))));
        Assert.Equal(0, Utils.FindPrimePower((int)(Math.Pow(15, 7))).Item2);
        Assert.Equal(0, Utils.FindPrimePower(42).Item2);
    }


    [Fact]
    public void GCD()
    {
        Assert.Equal(1, Utils.GCD(99, 2));
        Assert.Equal(1, Utils.GCD(2, 99));
        Assert.Equal(101, Utils.GCD(1009 * 101, 101 * 60));
        Assert.Equal(104729, Utils.GCD(104729 * 101, 104729 * 42));
    }

    [Fact]
    public void BitsNeededz()
    {
        Assert.Equal(6, Utils.BitsNeeded(63));
        Assert.Equal(7, Utils.BitsNeeded(64));

        Assert.Equal(2, Utils.BitsNeeded(3));
        Assert.Equal(1, Utils.BitsNeeded(1));
        Assert.Equal(20, Utils.BitsNeeded(1000000));
    }

    [Fact]
    public void GetQ()
    {
        Assert.Equal(128, Utils.GetQ(11));
        Assert.Equal(1024 * 1024, Utils.GetQ(1001));
        //    Assert.Equal(256ul * 512UL, Utils.GetQ(256));   // Check what the semantics ought to be on the edge case
        Assert.Equal(256 * 256, Utils.GetQ(256));
    }


    [Fact]
    public void ModExp()
    {
        Assert.Equal(4, Utils.ModExp(2, 8, 7));
        Assert.Equal(445, Utils.ModExp(4, 13, 497)); // wikipedia examples

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
        int denom = Utils.denominator(frac, qmax);
        double numerator = frac * denom;
        output.WriteLine($"{frac} can be rationalized as approx {numerator} / {denom}");
        Assert.Equal(expected, denom);
    }

    [Fact]
    public void SmallPrimes()
    {
        Assert.True(Utils.SmallPrimes[19]);
        Assert.False(Utils.SmallPrimes[21]);

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < Utils.SmallPrimes.Length; i++)
        {
            if (Utils.SmallPrimes[i]) { sb.Append($"{i}, "); }
        }
        output.WriteLine(sb.ToString());
    }


    //[Theory]
    //[InlineData(2117, ]
    //[InlineData(50, 0.66, 100)]
    //[InlineData(7, 3.14159, 100)]
    //[InlineData(3044, 3.14159, 10000)]
    //[InlineData(24239, 3.14159, 100000)]
    //[InlineData(5, 0.8, 100)]
    //public void powersModN(int expected, double frac, int qmax)
    //{
    //}
}
