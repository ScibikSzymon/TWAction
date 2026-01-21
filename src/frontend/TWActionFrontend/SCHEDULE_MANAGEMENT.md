# Dokumentacja Systemu Zarządzania Rozpiskami

## Przegląd

System umożliwia zalogowanym użytkownikom:

- Przeglądanie swoich rozpisek
- Tworzenie nowych rozpisek
- Edycję istniejących rozpisek
- Usuwanie rozpisek
- Wylogowanie się z systemu

## Struktura Projektu

### Typy TypeScript

**`types/schedule.ts`** - Definicje typów dla rozpisek:

- `WorldType` - enum światów (pl218-pl223)
- `ScheduleType` - enum typów rozpisek (Fake, Reconaissance, Main)
- `Schedule` - interfejs rozpiski
- `CreateScheduleRequest` - request do tworzenia
- `UpdateScheduleRequest` - request do aktualizacji

**`types/user.ts`** - Definicje typów dla użytkownika

### Serwisy API

**`services/scheduleService.ts`** - Obsługa wszystkich operacji CRUD na rozpiskach:

- `getSchedulesByUser(userId)` - pobieranie rozpisek użytkownika
- `getScheduleById(userId, scheduleId)` - pobieranie pojedynczej rozpiski
- `createSchedule(request)` - tworzenie nowej rozpiski
- `updateSchedule(scheduleId, request)` - aktualizacja rozpiski
- `deleteSchedule(scheduleId)` - usuwanie rozpiski

**`services/authService.ts`** - Obsługa autentykacji:

- `getMe()` - pobieranie danych zalogowanego użytkownika
- `logout()` - wylogowanie
- `redirectToGoogleLogin()` - przekierowanie do logowania Google

### Custom Hooks

**`hooks/useAuth.ts`** - Hook do zarządzania sesją użytkownika:

- Automatycznie sprawdza stan sesji przy montowaniu
- Zapewnia metody login/logout
- Zarządza stanem ładowania i błędów
- Zwraca status autentykacji

### Komponenty

**`components/ScheduleForm.tsx`** - Formularz tworzenia/edycji rozpiski:

- Walidacja nazwy (wymagana)
- Wybór świata z selecta
- Wybór typu rozpiski
- Obsługa stanu ładowania
- Wyświetlanie błędów

**`components/ScheduleList.tsx`** - Lista rozpisek:

- Wyświetlanie wszystkich rozpisek użytkownika
- Formatowanie daty utworzenia
- Przyciski edycji i usuwania
- Potwierdzenie przed usunięciem
- Stan pustej listy

### Strony

**`pages/HomePage.tsx`** - Główna strona aplikacji:

- Ekran logowania dla niezalogowanych użytkowników
- Panel rozpisek dla zalogowanych użytkowników
- Obsługa wszystkich operacji CRUD
- Zarządzanie stanem formularza (tworzenie/edycja)

## Przepływ Aplikacji

### 1. Logowanie

```
Użytkownik niezalogowany → Przycisk "Zaloguj się przez Google"
→ Przekierowanie do auth/google → Po zalogowaniu powrót do aplikacji
→ Hook useAuth automatycznie pobiera dane użytkownika
```

### 2. Wyświetlanie Rozpisek

```
HomePage montowanie → useAuth pobiera użytkownika
→ useEffect wykrywa user.id → loadSchedules()
→ scheduleService.getSchedulesByUser() → Aktualizacja stanu schedules
→ ScheduleList renderuje listę
```

### 3. Tworzenie Rozpiski

```
Przycisk "Nowa rozpiska" → setShowForm(true)
→ Renderowanie ScheduleForm → Wypełnienie formularza
→ Submit → handleCreateSchedule() → scheduleService.createSchedule()
→ Dodanie do stanu → Zamknięcie formularza
```

### 4. Edycja Rozpiski

```
Przycisk "Edytuj" → setEditingSchedule() + setShowForm(true)
→ ScheduleForm z danymi rozpiski → Modyfikacja
→ Submit → handleUpdateSchedule() → scheduleService.updateSchedule()
→ Aktualizacja w stanie → Zamknięcie formularza
```

### 5. Usuwanie Rozpiski

```
Przycisk "Usuń" → Potwierdzenie (confirm)
→ handleDeleteSchedule() → scheduleService.deleteSchedule()
→ Usunięcie ze stanu
```

### 6. Wylogowanie

```
Przycisk "Wyloguj się" → logout() z useAuth
→ authService.logout() → Czyszczenie stanu użytkownika
→ Przekierowanie do ekranu logowania
```

## Dobre Praktyki Zastosowane

### React Best Practices

1. **Functional Components & Hooks** - Wszystkie komponenty jako funkcje z hookami
2. **Custom Hooks** - `useAuth` enkapsuluje logikę autentykacji
3. **Controlled Components** - Formularze używają controlled inputs
4. **Proper State Management** - Stan lokalny dla UI, serwisy dla API
5. **Error Handling** - Try-catch we wszystkich operacjach async
6. **Loading States** - Wskaźniki ładowania dla lepszego UX
7. **TypeScript** - Pełne typowanie dla type safety
8. **CSS Modules** - Izolowane style dla każdego komponentu

### Zarządzanie Sesją

1. **Automatic Session Check** - useAuth sprawdza sesję przy montowaniu
2. **Credentials Handling** - `withCredentials: true` w axios dla cookies
3. **Error Recovery** - Graceful handling błędów autentykacji
4. **Conditional Rendering** - Różne widoki dla zalogowanych/niezalogowanych

### Struktura Kodu

1. **Separation of Concerns** - Serwisy, hooki, komponenty oddzielnie
2. **Reusable Components** - ScheduleForm dla create i edit
3. **Type Safety** - Type-only imports gdzie wymagane
4. **Clean Architecture** - Logika biznesowa oddzielona od UI

## Endpointy API (Backend)

```
GET    /schedules/{userId}              - Pobierz wszystkie rozpiski użytkownika
GET    /schedules/{userId}/{scheduleId} - Pobierz pojedynczą rozpiskę
POST   /schedules                       - Utwórz nową rozpiskę
PUT    /schedules/{scheduleId}          - Zaktualizuj rozpiskę
DELETE /schedules/{scheduleId}          - Usuń rozpiskę
```

## Testowanie

1. **Sprawdź logowanie** - Przekierowanie do Google auth
2. **Sprawdź pustą listę** - Komunikat "Brak rozpisek"
3. **Utwórz rozpiskę** - Formularz, walidacja, zapisanie
4. **Edytuj rozpiskę** - Załadowanie danych, modyfikacja
5. **Usuń rozpiskę** - Potwierdzenie, usunięcie
6. **Wyloguj się** - Powrót do ekranu logowania

## Potencjalne Rozszerzenia

- Paginacja dla dużej liczby rozpisek
- Filtrowanie i sortowanie
- Wyszukiwanie rozpisek
- Szczegóły rozpiski (osobna strona)
- Udostępnianie rozpisek
- Import/Export rozpisek
