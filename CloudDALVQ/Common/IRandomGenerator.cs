using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AsynchronousQuantization
{
    public interface IRandomGenerator
    {
        double[][] Generate(int n, int d);
    }
}
