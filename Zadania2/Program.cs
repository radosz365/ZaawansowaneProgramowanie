using System;
using System.Collections.Generic;
using System.Linq;

using Zadania2;

string name = "Radek", surname = "Szurek";
int a=1, b=2, c=33;
List<int> lista1 = new List<int> { 1, 2, 3, 4, 5 };
List<int> lista2 = new List<int> { 2, 3, 4, 6, 7 };

Console.WriteLine(Zadanie21.czesc(name, surname));

Console.WriteLine(Zadanie22.iloczyn(a, b));

bool wynik = Zadanie23.czyParzysta(c);
if(wynik == true)
{
    Console.WriteLine("Liczba prarzysta");
}
else
{
    Console.WriteLine("Liczba nieparzysta");
}

Console.WriteLine(Zadanie24.trzyLiczby(a, b, c));

Console.WriteLine(Zadanie25.lista(lista1, a));


foreach (int element in Zadanie26.dwieListy(lista1, lista2))
{
    Console.Write($"{element}, ");
}

Console.WriteLine();
Console.ReadKey();