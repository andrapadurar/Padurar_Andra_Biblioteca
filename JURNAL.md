## Laboratorul Î9 · Padurar Andra · nr. matricol 121200068 · varianta A3 · B2

| Parametru | Valoarea mea |
|---|---|
| A (ultima cifră: 8) | A3 |
| B (penultima cifră: 6) | B2 |
| ID cititor pentru teste | 1068 |
| Prag „Foarte recomandată” | 4,5 |
| Prag „Recomandată” | 3,5 |
| Tool MCP pentru S2 | most_ordered_books |

### A1 – Portalul cititorului

- Am creat proiectul Biblioteca.Portal folosind ASP.NET Core MVC.
- Am configurat comunicarea gRPC cu aplicația Biblioteca.
- Am implementat RecommendationsViewModel, RecommendationsController și pagina Recommendations/Index.
- Cititorul 5: au fost afișate 5 recomandări cu titluri și scoruri.
- Cititorul 1068: nu au fost afișate recomandări.

**Eticheta „cititor nou”:** Nu apare pentru cititorul 1068 deoarece serviciul nu a returnat recomandări. Eticheta este afișată doar pentru rezultatele cu `Fallback = true`.

### A2 – Contractul gRPC

Am adăugat câmpul label = 5 în recommender.proto.

Am implementat etichetele pentru varianta A3, cu pragurile 4,5 și 3,5.

Portalul a funcționat în continuare cu vechiul contract gRPC.

Biblioteca.Client: opțiunea 1 (Unary) a returnat 3 recomandări, iar opțiunea 2 (Server streaming) a returnat 5 recomandări.

După actualizarea contractului în Portal, etichetele au fost afișate corect.

Experiment title = 6: Titlurile cărților au dispărut din tabel, dar scorurile și etichetele au rămas vizibile. Acest lucru se întâmplă deoarece Protocol Buffers identifică fiecare câmp după numărul său, nu după nume.

Am revenit la title = 2 pentru a restabili compatibilitatea.
### A3 – Erori gRPC

[De completat după testare]

### S1 – Evaluările cititorului

[De completat după implementare]

### S2 – Tool MCP: most_ordered_books

[De completat după implementare]

### Folosirea AI

- Instrument utilizat: ChatGPT.
- Scop: explicarea cerințelor, configurarea proiectului și corectarea erorilor.
- Exemplu de cerere: „Cum configurez clientul gRPC în Biblioteca.Portal?”
- Răspuns greșit sau incomplet: [De completat după identificarea unui exemplu concret și modul în care a fost corectat.]

### Ce nu am reușit

[De completat la final, dacă este cazul.]