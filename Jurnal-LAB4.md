# Laboratorul Î4 · Padurar Andra · nr. matricol 121200068 · varianta A3 · B2

| Parametru | Valoarea mea |
| A (ultima cifră a nr. matricol: 8) | A3 |
| B (penultima cifră a nr. matricol: 6) | B2 |
| V1 · cărți pe pagină | 8 |
| V2 · sortarea implicită a catalogului | Număr de comenzi, descrescător |
| V3 · filtrul din S3 | Preț minim–maxim |
| V4 · indicatorul din S2 | Anul primei și al ultimei cărți |

## S1 · Comenzile

* **Protecția la overposting aleasă:** `Bind`
* **De ce am ales-o:** Am folosit atributul `[Bind]` pentru a limita proprietățile care pot fi transmise prin formularele de creare și editare a comenzilor. Astfel, utilizatorul poate modifica doar datele necesare, fără a suprascrie proprietățile de navigare `Customer` și `Book`. În metoda `Edit`, am actualizat explicit proprietățile permise ale entității existente.

Pentru crearea unei comenzi am utilizat:

```csharp
[Bind("CustomerID,BookID,OrderDate")] Order order
```

Pentru editare am utilizat:

```csharp
[Bind("OrderID,CustomerID,BookID,OrderDate")] Order order
```

Am implementat operațiile CRUD pentru comenzi, afișarea numelui cititorului și a titlului cărții, filtrarea după cititor și sortarea comenzilor descrescător după dată.

## S2 · Pagina unui autor și sortarea după autor

* **Indicatorul V4 (B2):** Anul primei și al ultimei cărți publicate de autor.

Indicatorul este calculat printr-o interogare LINQ tradusă de Entity Framework Core în SQL. Pentru fiecare autor, folosesc `MIN(Year)` și `MAX(Year)` pentru a obține anii extremi din cărțile înregistrate în catalog.

Interogarea SQL echivalentă este:

```sql
SELECT
    AuthorID,
    MIN(Year) AS FirstYear,
    MAX(Year) AS LastYear
FROM Book
WHERE AuthorID = @AuthorID
GROUP BY AuthorID;
```

Interogarea LINQ implementată în aplicație:

```csharp
var statistics = await _context.Book
    .Where(b => b.AuthorID == id)
    .GroupBy(b => b.AuthorID)
    .Select(g => new
    {
        TotalOrders = g.Sum(b => b.Orders.Count()),
        FirstYear = g.Min(b => b.Year),
        LastYear = g.Max(b => b.Year)
    })
    .FirstOrDefaultAsync();
```

* **De ce `OrderBy(b => b.Author.FullName)` aruncă o excepție?**

Proprietatea `FullName` este calculată în C# și nu este mapată direct la o coloană din baza de date. Entity Framework Core nu poate traduce automat această proprietate în SQL atunci când este folosită pentru sortare.

* **Cum am rezolvat:**

Am realizat sortarea folosind proprietățile `LastName` și `FirstName`, care sunt mapate în baza de date.

```csharp
"author" => books
    .OrderBy(b => b.Author!.LastName)
    .ThenBy(b => b.Author!.FirstName)
    .ThenBy(b => b.Title),

"author_desc" => books
    .OrderByDescending(b => b.Author!.LastName)
    .ThenByDescending(b => b.Author!.FirstName)
    .ThenBy(b => b.Title),
```

Astfel, sortarea este realizată la nivelul bazei de date, înainte de paginare.

## S3 · Filtrul variantei mele (V3)

* **Filtrul implementat:** Preț minim–maxim.

Am adăugat două câmpuri în pagina catalogului, care permit utilizatorului să introducă un preț minim și unul maxim.

Filtrarea este realizată prin LINQ, înainte de sortare și paginare.

```csharp
if (minPrice.HasValue)
{
    books = books.Where(b =>
        b.Price >= minPrice.Value);
}

if (maxPrice.HasValue)
{
    books = books.Where(b =>
        b.Price <= maxPrice.Value);
}
```

Interogarea SQL echivalentă, cu ambele limite introduse:

```sql
SELECT
    b.ID,
    b.Title,
    b.Price,
    a.FirstName,
    a.LastName,
    g.Name AS Genre,
    (
        SELECT COUNT(*)
        FROM "Order" o
        WHERE o.BookID = b.ID
    ) AS NumberOfOrders
FROM Book b
LEFT JOIN Author a ON b.AuthorID = a.ID
LEFT JOIN Genre g ON b.GenreID = g.ID
WHERE b.Price >= 20
  AND b.Price <= 50
ORDER BY NumberOfOrders DESC, b.ID
LIMIT 8 OFFSET 0;
```

Aceasta este o reprezentare SQL echivalentă pentru exemplul cu preț minim 20 și maxim 50, fără un filtru suplimentar după titlu.

Am păstrat filtrele active atunci când utilizatorul schimbă sortarea sau navighează între pagini. Paginarea este realizată în baza de date prin `Skip()` și `Take()`, cu 8 cărți pe pagină, conform variantei A3.

## S4 · Folosirea AI

* **Instrumente folosite:** ChatGPT (OpenAI).

* **La ce pași:** Am utilizat ChatGPT pentru explicarea cerințelor laboratorului, corectarea erorilor din controllere și verificarea funcționalităților.

* **Un exemplu de cerere trimisă:**

> „De ce primesc eroare la Customer?”

* **O situație în care am corectat sau respins un răspuns AI:**

  * **Ce a propus AI-ul / care era eroarea:** În procesul de adaptare a controllerului pentru cititori, au apărut erori după înlocuirea unor identificatori. În unele locuri era folosit `id`, deși modelul `Customer` avea proprietatea `CustomerID`.

  * **Cum mi-am dat seama că era greșit:** Visual Studio afișa erori de compilare `CS1061`, deoarece clasa `Customer` nu conținea o proprietate numită `id`. Am verificat mesajele din Error List și definiția modelului.

  * **Codul inițial, cu eroare:**

```csharp
var customer = await _context.Customer
    .FirstOrDefaultAsync(m => m.id == id);
```

  * **Codul final, corectat:**

```csharp
var customer = await _context.Customer
    .FirstOrDefaultAsync(m => m.CustomerID == id);
```

---