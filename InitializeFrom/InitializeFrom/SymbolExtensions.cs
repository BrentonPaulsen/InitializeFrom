using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace InitializeFrom
{
    public static class SymbolExtensions
    {
        public static IEnumerable<TypedSymbol> GetTypedMembers(this ITypeSymbol symbol)
        {
            var members = symbol.GetMembers()
                .Where(x => x.CanBeReferencedByName);
            var fields = members.OfType<IFieldSymbol>().Select(x => new TypedSymbol
            {
                Symbol = x,
                Type = x.Type
            });
            var properties = members.OfType<IPropertySymbol>().Select(x => new TypedSymbol
            {
                Symbol = x,
                Type = x.Type
            });
            return fields.Union(properties);
        }
    }
}
