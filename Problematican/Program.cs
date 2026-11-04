var builder = WebApplication.CreateBuilder(args);

// Namjerno dostupno samo na racunaru na kojem je aplikacija pokrenuta.
builder.WebHost.UseUrls("http://127.0.0.1:5055");
var app = builder.Build();

// Testni podaci: jedan javni i jedan interni dokument.
string podaci = Path.Combine(app.Environment.ContentRootPath, "Podaci");
string javno = Path.Combine(podaci, "Javno");
string interno = Path.Combine(podaci, "Interno");
Directory.CreateDirectory(javno);
Directory.CreateDirectory(interno);

string javniDokument = Path.Combine(javno, "ugovor.txt");
string interniDokument = Path.Combine(interno, "procjena-rizika.txt");
if (!File.Exists(javniDokument))
    File.WriteAllText(javniDokument, "Ugovor broj 105 - javni testni dokument.");
if (!File.Exists(interniDokument))
    File.WriteAllText(interniDokument, "INTERNO - povjerljiva procjena kreditnog rizika (testni podaci).");

// Jedina vidljiva stranica aplikacije.
app.MapGet("/", () => Results.Content("""
    <!doctype html>
    <html lang="bs">
    <head><meta charset="utf-8"><title>Kreditni portal</title></head>
    <body style="font-family:Arial;max-width:640px;margin:80px auto">
      <h1>Kreditni portal</h1>
      <p>Dobrodošli u portal za upravljanje kreditima.</p>
    </body>
    </html>
    """, "text/html; charset=utf-8"));

// (CWE-22): korisnik upravlja nazivom i putanjom.
app.MapGet("/dokument", (string naziv) =>
{
    string putanja = Path.GetFullPath(Path.Combine(javno, naziv));

    // Ogranicenje: zabranjujemo izlaz IZ CIJELOG TESTNOG FOLDERA.
    // Propust ostaje: moguc je izlaz iz Javno u Interno.
    string granica = Path.GetFullPath(podaci) + Path.DirectorySeparatorChar;
    if (!putanja.StartsWith(granica, StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest("Putanja je izvan testnog foldera.");

    if (!File.Exists(putanja))
        return Results.NotFound("Dokument ne postoji.");

    return Results.Text(File.ReadAllText(putanja), "text/plain; charset=utf-8");
});

app.Run();
                