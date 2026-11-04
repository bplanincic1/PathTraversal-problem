# CWE-22 — poređenje problematicne i ispravljene aplikacije

Dva nezavisna ASP.NET Core (net8.0) projekta, sa identicnom pocetnom stranicom, istom rutom `/dokument` i istim lokalnim testnim dokumentima. Jedina bitna razlika u ponasanju je validacija korisnickog parametra `naziv`.

## Struktura

- `Problematican/`: izvorni, namjerno ranjivi `Program.cs`, port **5055**.
- `Ispravan/`: ispravljeni `Program.cs`, port **5056**.
- `PathTraversal-p.slnx`: oba projekta otvorena zajedno u Visual Studiju.

## Pokretanje

Instaliran .NET 8 SDK (ili noviji SDK koji moze buildati `net8.0` projekat).

U prvom terminalu, iz korijena repozitorija:

```powershell
dotnet run --project .\Problematican\PathTraversal-p.csproj
```

U drugom terminalu:

```powershell
dotnet run --project .\Ispravan\PathTraversal-Ispravan.csproj
```

Mozes pokrenuti oba istovremeno: jedan slusa na `127.0.0.1:5055`, drugi na `127.0.0.1:5056`.

## Tri poredbena testa (iskljucivo lokalno)

| Test | Problematican | Ispravan |
|---|---|---|
| Pocetna | http://127.0.0.1:5055/ | http://127.0.0.1:5056/ |
| Javni ugovor | http://127.0.0.1:5055/dokument?naziv=ugovor.txt | http://127.0.0.1:5056/dokument?naziv=ugovor.txt |
| Nedozvoljeni interni dokument | http://127.0.0.1:5055/dokument?naziv=../Interno/procjena-rizika.txt | http://127.0.0.1:5056/dokument?naziv=../Interno/procjena-rizika.txt |

**Ocekivano:** obe pocetne stranice rade, oba javna ugovora se vracaju (`200 OK`). Ranjivi endpoint u problematicnoj verziji moze vratiti interni tekst (`200 OK`), dok ispravan endpoint odbija naziv (`400 Bad Request`).

Slicno testiraj `%2e%2e%2fInterno%2fprocjena-rizika.txt` u **query parametru**. Dvostruko enkodiranje nije primijenjeno u ovom kodu i ne treba tvrditi da automatski funkcionise.

## Sta je ispravljeno

U problematicnoj verziji korisnik odlucuje sta ce se dodati u putanju, a provjera ogranicava samo na roditeljski folder `Podaci`, pa se moze izaci iz `Javno` i uci u `Interno`.

U ispravnoj verziji dozvoljen je jedino `ugovor.txt` (`StringComparison.Ordinal`). Aplikacija sastavlja putanju iz **fiksnog** naziva, tako da vrijednost query parametra ne moze promijeniti adresu datoteke. Ovo je namjerno jednostavno rjesenje za demonstraciju; prava aplikacija bi imala mapiranje ID-ja dokumenta, provjeru ovlastenja i odgovarajuce kontrole pristupa datotekama.

## SAST poredjenje

Pokreni CodeQL i SonarQube Cloud na istom GitHub repozitoriju. Oba alata trebaju pregledati oba projekta (i njihov `.csproj`). **Nemoj unaprijed upisivati da je nalaz otkriven:** CodeQL i SonarQube mogu razlicito tretirati djelimicnu provjeru putanje u problematicnoj verziji. Prvo napravi screenshot stvarnih nalaza, pa tek onda zakljuci sta je alat otkrio i sta je propustio.

## Kako prebaciti u postojeci lokalni Git repozitorij

Kod tebe je korijen repozitorija vec `PathTraversal-problem/` i u njemu su na najvisem nivou `Program.cs`, `PathTraversal-p.csproj`, `PathTraversal-p.slnx`, `Podaci/`, `Properties/` i `.gitignore`.

**Opcija A (sigurna i jednostavna):** raspakuj arhivu na drugo mjesto i kopiraj dva nova foldera `Problematican/` i `Ispravan/` u svoj postojeći repo. Zatim u korijenu repo-a obrisi stare kopije `Program.cs`, `PathTraversal-p.csproj`, `Podaci/`, `Properties/` (ostale su duplikati). Iz arhive kopiraj novi `PathTraversal-p.slnx` u korijen. Osvjezi `.gitignore` ako je potrebno, a lokalni `.git/` folder NIKADA ne diraj. Ako zelis, kopiraj i novi `README.md` ili nadopuni svoj.

**Opcija B:** koristi datoteke iz ZIP-a kao osnovu sasvim novog repozitorija; ne prekopiravaj skrivenu `.git` datoteku ili mapu.

Zatim iz korijena repozitorija:

```powershell
git status
git add -A
git commit -m "Add vulnerable and fixed CWE-22 examples"
git push -u origin main
```

Zadnji `git push` izvrsavaj kada GitHub Git operacije ponovo rade normalno. Ne koristi `--force`.

## Oprez

Problematican projekat je namjerno ranjiv i veze se iskljucivo na `127.0.0.1`. Ne objavljuj ga na javnom serveru i ne dodaj stvarne povjerljive podatke.
