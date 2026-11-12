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

Am implementat validarea cererilor în Recommend și StreamRecommendations.

Pentru cititorul -3, serverul a returnat InvalidArgument, cu mesajul „reader_id trebuie să fie un număr pozitiv”.

Pentru Top = 50, rezultatul așteptat este InvalidArgument, cu mesajul „top poate fi cel mult 20”.

La oprirea serverului Biblioteca, Portalul a afișat Unavailable: Error connecting to subchannel.

### S1 – Evaluările cititorului
Am creat serviciul ReaderRatings, cu operațiile ListRatings, GetRating, AddRating, UpdateRating și DeleteRating.

Am implementat paginile pentru afișarea, adăugarea, editarea și ștergerea evaluărilor în Biblioteca.Portal.

Am testat operațiile pentru cititorul 1068. Adăugarea, afișarea, modificarea și ștergerea funcționează.

Pentru cartea 999, serviciul a returnat eroarea NotFound.

Pentru nota 7, serviciul a returnat eroarea InvalidArgument.
Am putea implementa autentificare pentru a permite  cititorului  să-și gestioneze doar propriile evaluările.

### S2 – Tool MCP: most_ordered_books

- Am implementat instrumentul most_ordered_books în LibraryTools.cs.
- Instrumentul returnează cele mai comandate cărți, ordonate descrescător după numărul comenzilor.
- Parametrul top acceptă valori între 1 și 10, implicit 5.
- Am verificat schema JSON folosind Inspectorul MCP.
- Testul cu top = 5 a funcționat (IsError: False).
Rezultatul testului:
Carte	Număr comenzi
Craii de Curtea-Veche	15
Culoarea magiei	11
Cuvinte potrivite	11
Fundația	11
Istoria critică a românilor	10- Pentru top = 50, serverul a generat eroarea ArgumentException, cu mesajul „Parametrul top trebuie sa fie intre 1 si 10”.
 Am instalat modelul Ollama llama3.2, dar nu am reușit să finalizez testarea Agentului deoarece laptopul s-a blocat în timpul rulării.

### Folosirea AI

- Instrument utilizat: ChatGPT.
- Scop: explicarea cerințelor, configurarea proiectului și corectarea erorilor.
- Exemplu de cerere: „Cum configurez clientul gRPC în Biblioteca.Portal?”
La implementarea instrumentului most_ordered_books, AI-ul a sugerat folosirea Math.Clamp(top, 1, 10) pentru validarea parametrului. Această soluție nu era potrivită cerinței, deoarece modifica automat valorile invalide în loc să returneze o eroare.
Am corectat implementarea folosind ArgumentException, astfel încât pentru top = 50 să fie afișat un mesaj de eroare clar.