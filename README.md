# Salon fryzjerski

Aplikacja do zarządzania salonem fryzjerskim: usługami, pracownikami, wizytami, grafikiem pracy oraz uprawnieniami użytkowników.

## Technologie

- C# i ASP.NET Core MVC (.NET 8)
- Entity Framework Core i SQLite
- ASP.NET Core Identity
- Razor, Bootstrap i jQuery

## Uruchomienie lokalne

Wymagany jest **.NET 8 SDK** (sam runtime nie wystarczy). Projekt można otworzyć w Visual Studio 2022 za pomocą pliku `Salon fryzjerski.sln`.

W terminalu, w katalogu projektu:

```powershell
dotnet restore
dotnet tool install --global dotnet-ef --version 8.0.23
dotnet ef database update
dotnet run --launch-profile https
```

Jeśli narzędzie `dotnet-ef` jest już zainstalowane w wersji 8, pomiń polecenie instalacji. W razie problemów z lokalnym certyfikatem HTTPS wykonaj `dotnet dev-certs https --trust`.

Baza jest tworzona przez migracje w `Data/app.db`; repozytorium zawiera schemat i migracje, a dane pozostają lokalne.

## Struktura

- `Controllers/` — obsługa modułów aplikacji
- `Models/` — modele i modele widoków
- `Views/` — widoki Razor
- `Data/` i `Migrations/` — kontekst bazy i historia schematu
- `Security/` i `Services/` — kontrola dostępu
- `wwwroot/` — style, skrypty i biblioteki przeglądarkowe

## Zapisywanie kolejnych zmian

```powershell
git add .
git commit -m "Opis zmian"
git push
```

Plik `.gitignore` pomija bazę SQLite, wyniki kompilacji i lokalne ustawienia Visual Studio. Pełną kopię bazy przechowuj osobno, po zamknięciu aplikacji, aby nie stracić danych zapisanych w pliku WAL. Oryginalne archiwum `Salon fryzjerski.7z` zawiera wcześniejszy stan projektu wraz z lokalną bazą.

Kod zaimportowano z archiwum bez zmian w logice aplikacji. W środowisku importu nie był dostępny .NET SDK, więc kompilacja i uruchomienie nie zostały sprawdzone.

Repozytorium pomija generowane mapy debugowania bibliotek (`wwwroot/**/*.map`). Pliki CSS i JavaScript oraz licencje bibliotek pozostają w repozytorium; mapy są zachowane w oryginalnym archiwum i lokalnej paczce ZIP.

