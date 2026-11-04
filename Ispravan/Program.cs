var builder = WebApplication.CreateBuilder(args);

// Sigurna verzija je dostupna samo lokalno i koristi poseban port.
builder.WebHost.UseUrls("http://127.0.0.1:5056");
var app = builder.Build();

// Isti testni podaci kao u problematicnoj verziji.
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

// Potpuno ista pocetna stranica kao u problematicnoj verziji.
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

// ISPRAVLJENO (CWE-22): korisnicki unos vise ne postaje putanja.
app.MapGet("/dokument", (string naziv) =>
{
    // U ovom malom portalu korisnik moze zatraziti samo javni ugovor.
    // Odbijamo ../, URL-enkodirane putanje, druge nazive i apsolutne putanje.
    if (!string.Equals(naziv, "ugovor.txt", StringComparison.Ordinal))
        return Results.BadRequest("Nedozvoljen dokument.");

    // Putanju formira aplikacija iz fiksnog, odobrenog naziva,
    // bez korisnickog unosa koji bi mogao mijenjati direktorij.
    string putanja = Path.Combine(javno, "ugovor.txt");

    if (!File.Exists(putanja))
        return Results.NotFound("Dokument ne postoji.");

    return Results.Text(File.ReadAllText(putanja), "text/plain; charset=utf-8");
});

app.Run();
