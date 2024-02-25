using System;
using System.Collections.Generic;
using System.Linq;

string name = "Radek", surname = "Szurek";
int a=1, b=2, c=33;
List<int> lista1 = new List<int> { 1, 2, 3, 4, 5 };
List<int> lista2 = new List<int> { 2, 3, 4, 6, 7 };

Console.WriteLine(czesc(name, surname));

Console.WriteLine(iloczyn(a, b));

bool wynik = czyParzysta(c);
if(wynik == true)
{
    Console.WriteLine("Liczba prarzysta");
}
else
{
    Console.WriteLine("Liczba nieparzysta");
}

Console.WriteLine(trzyLiczby(a, b, c));

Console.WriteLine(lista(lista1, a));


foreach (int element in dwieListy(lista1, lista2))
{
    Console.Write($"{element}, ");
}

Console.WriteLine();
Console.ReadKey();

int iloczyn(int x, int y)
{
    return x * y;
}

bool czyParzysta(int x)
{
    return x%2==0;
}

bool trzyLiczby(int x, int y, int z)
{
    return x + y >= z;
}

bool lista(List<int> l, int x)
{
    return l.Contains(x);
}

List<int> dwieListy(List<int> lista1, List<int> lista2)
{
    return new HashSet<int>(lista1.Concat(lista2)).Select(x => (int)Math.Pow(x, 3)).ToList();
}
