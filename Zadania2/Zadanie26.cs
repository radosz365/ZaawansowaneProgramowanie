using System.Collections.Generic;
using System.Linq;

namespace Zadania2
{
    public class Zadanie26
    {
        public static List<int> dwieListy(List<int> lista1, List<int> lista2)
        {
            return new HashSet<int>(lista1.Concat(lista2)).Select(x => (int)Math.Pow(x, 3)).ToList();
        }
    }
}