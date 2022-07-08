using System.Collections.Generic;

namespace MsgBox.Controllers
{
    internal struct EqualityComparerByTag : IEqualityComparer<object>
    {
        public static EqualityComparerByTag Default = new EqualityComparerByTag();

        bool IEqualityComparer<object>.Equals(dynamic x, dynamic y) => object.Equals(x.Tag, y.Tag);

        int IEqualityComparer<object>.GetHashCode(dynamic obj) => (int)obj.Tag.GetHashCode();
    }
}
