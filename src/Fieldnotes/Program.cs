using Fieldnotes.Components;
using Fieldnotes.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(ConceptCatalog.LoadBundled());
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IStudyStore, BrowserStudyStore>();
builder.Services.AddScoped<StudyService>();

await builder.Build().RunAsync();

public partial class Program;