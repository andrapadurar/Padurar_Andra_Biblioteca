# Laboratorul Î5 – Padurar Andra

**Număr matricol:** 121200068  
**Varianta:** A3, B2  
**Branch GitHub:** `laborator-5`

## A1 – Importul evaluărilor din CSV

 Am importat cu succes **6.599 de evaluări**.

## A2 – Reantrenarea modelului ML.NET

Am implementat un buton de reantrenare în pagina /Admin.

Rezultatele obținute:

Număr de evaluări utilizate: 6.599

RMSE: 0,71

Reantrenarea a fost executată cu succes din interfața aplicației.

## A3 – Recomandări și cold start

Cititorul 5 primește recomandări personalizate, calculate de modelul ML.NET. În răspuns apare "fallback": false, ceea ce confirmă că se folosește modelul.

Cititorul 999 primește recomandări generale, bazate pe cărțile cele mai bine evaluate, care au cel puțin 12 evaluări, conform variantei A3. În răspuns apare "fallback": true, ceea ce confirmă activarea mecanismului Cold Start.

## A4 – Clasificarea genului cărților

De completat după implementare.

## S1 – Jurnalul recomandărilor

De completat după implementare.

## S2 – Dashboard Chart.js

De completat după implementare.