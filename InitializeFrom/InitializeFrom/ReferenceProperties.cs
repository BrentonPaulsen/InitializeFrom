using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace InitializeFrom
{
    public class ReferenceProperties
    {
        public ISymbol Reference { get; set; }
        public ITypeSymbol Type { get; set; }
        public IEnumerable<TypedSymbol> Properties { get; set; }
    }
}
