using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace InitializeFrom
{
    public class TypedSymbol
    {
        public ISymbol Symbol { get; set; }
        public ITypeSymbol Type { get; set; }
    }
}
