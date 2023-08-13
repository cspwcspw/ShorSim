
using FactorLab;
using Xunit;
using Xunit.Abstractions;

namespace xTestSuite;
// NB: I had to set the properties of the xTestSuite to target Windows to
// get rid of the triangle error on the project reference.  JohnH entry at
// https://csharpforums.net/threads/cannot-add-reference-to-xunit-test-project.8515/   


// Incomplete

public class FactorLabTests
{

    [Theory]
    [InlineData(35, 13, 5, 7)]
    [InlineData(55, 83, 5, 11)]
    [InlineData(143, 51, 11, 13)]
    [InlineData(77, 71, 7, 11)]
    [InlineData(99, 37, 9, 11)]

    public void factorizer(int n, int magicX, int factor1, int factor2)
    {
        Factorizer sf = new Factorizer(n);
        //var factors = sf.HuntForFactors(magicX, .31, .41);
        //Assert.Equal((factor1, factor2), factors);
    }

}