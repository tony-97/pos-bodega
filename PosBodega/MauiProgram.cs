using Microsoft.Extensions.Logging;
using PosBodega.Data;
using PosBodega.Services;

namespace PosBodega;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		// Database
		builder.Services.AddSingleton<AppDbContext>();

		// Services - Singleton for desktop app (single user session)
		builder.Services.AddSingleton<AuthService>();
		builder.Services.AddSingleton<CategoriaService>();
		builder.Services.AddSingleton<ProductoService>();
		builder.Services.AddSingleton<ProveedorService>();
		builder.Services.AddSingleton<CompraService>();
		builder.Services.AddSingleton<VentaService>();
		builder.Services.AddSingleton<ClienteService>();
		builder.Services.AddSingleton<PagoFiadoService>();
		builder.Services.AddSingleton<ReporteService>();
		builder.Services.AddSingleton<EmpresaService>();
		builder.Services.AddSingleton<UsuarioService>();

		var app = builder.Build();

		// Ensure database is created and initialized with seed data
		using (var scope = app.Services.CreateScope())
		{
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
			DbInitializer.Initialize(db);
		}

		return app;
	}
}
