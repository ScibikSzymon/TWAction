# Zarządzanie Stanem Wojsk

## Przegląd

Funkcjonalność zarządzania stanem wojsk pozwala użytkownikom wgrywać i zarządzać danymi o wojskach dla aktywnej rozpiski. Każda rozpiska może mieć maksymalnie jeden stan wojsk, który może być aktualizowany w dowolnym momencie.

## Struktura Plików

### Typy TypeScript

- **src/types/troopsState.ts** - Definicje typów dla stanu wojsk
  - `TroopsState` - Interfejs reprezentujący stan wojsk
  - `UploadTroopsStateRequest` - Interfejs dla żądania wgrania stanu wojsk

### Serwisy

- **src/services/troopsStateService.ts** - Serwis API dla operacji na stanie wojsk
  - `getTroopsState(scheduleId)` - Pobiera stan wojsk dla rozpiski
  - `uploadTroopsState(scheduleId, request)` - Wgrywa/aktualizuje stan wojsk

### Komponenty

- **src/components/TroopsStateManager.tsx** - Główny komponent zarządzania stanem wojsk
- **src/components/TroopsStateManager.module.css** - Style dla komponentu

## Funkcjonalność

### Wyświetlanie Informacji o Stanie Wojsk

Gdy stan wojsk jest już wgrany dla aktywnej rozpiski, użytkownik widzi:

- **Ostatnia aktualizacja** - Data i czas ostatniej aktualizacji
- **Liczba graczy** - Ilość unikalnych graczy w stanie wojsk
- **Liczba wiosek** - Całkowita liczba wiosek
- **Data utworzenia** - Kiedy stan wojsk został po raz pierwszy wgrany

### Wgrywanie Stanu Wojsk

Użytkownik może wgrać stan wojsk poprzez:

1. Wybranie aktywnej rozpiski z listy
2. Skopiowanie danych ze statystyk gry (format CSV)
3. Wklejenie danych w pole tekstowe
4. Kliknięcie przycisku "Wgraj" lub "Aktualizuj"

### Format Danych

Akceptowany format danych (CSV):

```
Nazwa gracza,Wioska,Piki,Miecze,Zwiad,CK,Katasy,Topory,LK,Tarany,Grube
6ikanar6 x Ciawka24,492|577,140,140,345,0,45,5505,2194,298,4
6ikanar6 x Ciawka24,505|571,0,54,50,0,0,0,0,0,0
AASgirl,495|566,177,160,0,0,0,0,0,0,0
```

Backend obsługuje również angielskie nagłówki:

```
PlayerName,Village,Spear,Sword,Archer,Marcher,Catapult,Axe,Polearm,Ram,Trebuchet
```

## Obsługa Błędów

Komponent obsługuje różne scenariusze błędów:

### Błędy Walidacji (Backend)

- Brak nagłówka w danych
- Nieprawidłowe koordynaty wiosek
- Nieprawidłowy format danych
- Błędy parsowania

Komunikaty błędów są wyświetlane użytkownikowi z dokładnym opisem problemu zwróconym przez backend.

### Błędy Sieci

- Problemy z połączeniem
- Timeout
- Błędy serwera (500)

### Brak Aktywnej Rozpiski

Gdy użytkownik nie ma wybranej aktywnej rozpiski, wyświetlany jest komunikat:
"Wybierz aktywną rozpiskę, aby zarządzać stanem wojsk"

## Integracja z HomePage

Komponent `TroopsStateManager` jest zintegrowany z `HomePage` i:

- Automatycznie ładuje stan wojsk dla aktywnej rozpiski
- Reaguje na zmiany aktywnej rozpiski
- Wyświetla się poniżej listy rozpisek

## Komunikaty Użytkownika

### Sukces

Po pomyślnym wgraniu stanu wojsk:

```
Stan wojsk został pomyślnie wgrany! Znaleziono X wiosek i Y graczy.
```

Komunikat automatycznie znika po 5 sekundach.

### Błędy

Błędy są wyświetlane w czerwonym panelu z dokładnym opisem problemu.

## Stany Komponentu

1. **Brak aktywnej rozpiski** - Wyświetlanie komunikatu informacyjnego
2. **Ładowanie** - Podczas pobierania stanu wojsk z API
3. **Brak danych** - Gdy rozpiska nie ma jeszcze stanu wojsk
4. **Dane załadowane** - Wyświetlanie informacji + możliwość aktualizacji
5. **Wgrywanie** - Podczas wysyłania danych do API

## Dobre Praktyki

### Performance

- Komponent korzysta z `useEffect` do automatycznego ładowania danych
- Nie wykonuje zbędnych zapytań API
- Obsługuje cleanup w przypadku odmontowania komponentu

### UX

- Wyłączanie przycisku podczas wgrywania
- Natychmiastowa informacja zwrotna (błędy/sukces)
- Automatyczne czyszczenie pola tekstowego po sukcesie
- Podpowiedzi dla użytkownika (placeholder, hints)

### Bezpieczeństwo

- Walidacja danych po stronie backendu
- Obsługa różnych typów błędów
- Sanityzacja danych wejściowych

### Dostępność

- Semantyczny HTML (label + input)
- Opisowe przyciski
- Komunikaty dla czytników ekranu

## Przyszłe Rozszerzenia

Potencjalne funkcjonalności do rozważenia:

- Podgląd sparsowanych danych przed wgraniem
- Historia zmian stanu wojsk
- Eksport stanu wojsk
- Porównywanie stanów wojsk w czasie
- Walidacja po stronie klienta (przed wysłaniem do API)
- Drag & drop dla plików CSV
