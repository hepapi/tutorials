using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Content(@"
<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
  <title>Shipping Dashboard</title>
  <style>
    :root {
      --bg: #0f172a;
      --bg-soft: #111827;
      --panel: rgba(15, 23, 42, 0.76);
      --border: rgba(148, 163, 184, 0.18);
      --text: #e5eefb;
      --muted: #94a3b8;
      --accent: #14b8a6;
      --accent-2: #38bdf8;
      --success: #22c55e;
      font-family: ""Segoe UI"", system-ui, sans-serif;
    }

    * { box-sizing: border-box; }

    body {
      margin: 0;
      min-height: 100vh;
      color: var(--text);
      background:
        radial-gradient(circle at top, rgba(20, 184, 166, 0.16), transparent 30%),
        linear-gradient(180deg, #0b1120 0%, #111827 100%);
    }

    .shell {
      width: min(1100px, calc(100% - 32px));
      margin: 0 auto;
      padding: 40px 0 56px;
    }

    .hero {
      padding: 32px;
      border: 1px solid var(--border);
      border-radius: 28px;
      background: linear-gradient(135deg, rgba(15, 23, 42, 0.92), rgba(17, 24, 39, 0.74));
      box-shadow: 0 24px 60px rgba(0, 0, 0, 0.28);
    }

    .eyebrow {
      display: inline-block;
      margin-bottom: 12px;
      padding: 8px 12px;
      border-radius: 999px;
      background: rgba(20, 184, 166, 0.14);
      color: #7dd3fc;
      font-size: 12px;
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    h1 {
      margin: 0 0 12px;
      font-size: clamp(2.4rem, 4vw, 4.2rem);
      line-height: 0.95;
      letter-spacing: -0.05em;
    }

    .lead {
      max-width: 54ch;
      margin: 0;
      color: var(--muted);
      font-size: 1.02rem;
      line-height: 1.75;
    }

    .grid {
      display: grid;
      grid-template-columns: repeat(3, minmax(0, 1fr));
      gap: 18px;
      margin-top: 24px;
    }

    .card {
      padding: 20px;
      border: 1px solid var(--border);
      border-radius: 22px;
      background: rgba(15, 23, 42, 0.6);
    }

    .card strong {
      display: block;
      margin-bottom: 8px;
      font-size: 1.5rem;
      letter-spacing: -0.04em;
    }

    .card span,
    .card p,
    .endpoints li {
      color: var(--muted);
    }

    .stack {
      display: grid;
      grid-template-columns: 1.2fr 0.8fr;
      gap: 18px;
      margin-top: 18px;
    }

    .panel {
      padding: 24px;
      border: 1px solid var(--border);
      border-radius: 22px;
      background: rgba(15, 23, 42, 0.54);
    }

    .panel h2 {
      margin: 0 0 12px;
      font-size: 1.2rem;
    }

    .panel p {
      margin: 0;
      line-height: 1.7;
      color: var(--muted);
    }

    .pill {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      margin-top: 14px;
      padding: 10px 14px;
      border-radius: 999px;
      background: rgba(34, 197, 94, 0.12);
      color: #bbf7d0;
      font-weight: 600;
    }

    .dot {
      width: 10px;
      height: 10px;
      border-radius: 50%;
      background: var(--success);
      box-shadow: 0 0 16px rgba(34, 197, 94, 0.7);
    }

    code {
      color: #bfdbfe;
      font-family: ""SFMono-Regular"", Consolas, monospace;
    }

    ul {
      margin: 0;
      padding-left: 18px;
    }

    @media (max-width: 860px) {
      .grid,
      .stack {
        grid-template-columns: 1fr;
      }

      .shell {
        width: min(1100px, calc(100% - 20px));
        padding-top: 20px;
      }
    }
  </style>
</head>
<body>
  <main class=""shell"">
    <section class=""hero"">
      <span class=""eyebrow"">.NET Example</span>
      <h1>Shipping Dashboard</h1>
      <p class=""lead"">
        A small ASP.NET Core app for Docker demos. It is intentionally lightweight,
        fast to build, and simple enough to use when explaining image layers and multi-stage builds.
      </p>

      <div class=""grid"">
        <div class=""card"">
          <strong>8080</strong>
          <span>application port</span>
        </div>
        <div class=""card"">
          <strong>.NET 6</strong>
          <span>runtime target</span>
        </div>
        <div class=""card"">
          <strong>2</strong>
          <span>useful demo endpoints</span>
        </div>
      </div>

      <div class=""stack"">
        <div class=""panel"">
          <h2>Why this sample exists</h2>
          <p>
            This app gives you a clean baseline for containerization: one process, one port,
            one straightforward startup command. It is ideal when the goal is teaching Docker,
            not debugging application complexity.
          </p>
          <div class=""pill"">
            <span class=""dot""></span>
            Status: healthy and ready to containerize
          </div>
        </div>
        <div class=""panel endpoints"">
          <h2>Endpoints</h2>
          <ul>
            <li><code>/</code> visual landing page</li>
            <li><code>/health</code> simple health response</li>
            <li><code>/api/info</code> app metadata as JSON</li>
          </ul>
        </div>
      </div>
    </section>
  </main>
</body>
</html>
", "text/html"));

app.MapGet("/health", () => Results.Json(new { status = "ok" }));
app.MapGet("/api/info", () => Results.Json(new
{
    name = "shipping-dashboard",
    framework = ".NET 6",
    port = 8080
}));

app.Run("http://0.0.0.0:8080");
