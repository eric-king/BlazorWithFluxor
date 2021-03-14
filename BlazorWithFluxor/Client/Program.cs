using Blazored.LocalStorage;
using Blazored.Toast;
using Fluxor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorWithFluxor.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddFluxor(o => o.ScanAssemblies(typeof(Program).Assembly).UseReduxDevTools());
            builder.Services.AddBlazoredToast();
            builder.Services.AddBlazoredLocalStorage(config => 
            {
                config.JsonSerializerOptions.WriteIndented = true;
            });
            await builder.Build().RunAsync();
        }
    }
}
