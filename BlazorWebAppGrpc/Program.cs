using System.IO.Pipes;
using System.Net;
using System.Security.Principal;
using BlazorWebAppGrpc.Components;
using BlazorWebAppGrpc.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddGrpc();

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Loopback, 5000, options =>
    {
        options.Protocols = HttpProtocols.Http1AndHttp2;
    });
    serverOptions.ListenNamedPipe("MyPipeName", options =>
    {
        options.Protocols = HttpProtocols.Http2;
    });
});

//builder.WebHost.UseNamedPipes();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapGrpcService<WeatherService>();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public class NamedPipesConnectionFactory
{
    private readonly string pipeName;

    public NamedPipesConnectionFactory(string pipeName)
    {
        this.pipeName = pipeName;
    }

    public async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext _,
        CancellationToken cancellationToken = default)
    {
        var clientStream = new NamedPipeClientStream(
            serverName: ".",
            pipeName: this.pipeName,
            direction: PipeDirection.InOut,
            options: PipeOptions.WriteThrough | PipeOptions.Asynchronous,
            impersonationLevel: TokenImpersonationLevel.Anonymous);

        try
        {
            await clientStream.ConnectAsync(cancellationToken).ConfigureAwait(false);
            return clientStream;
        }
        catch
        {
            clientStream.Dispose();
            throw;
        }
    }


}