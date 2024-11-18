using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using WeatherForecast = BlazorWebAppGrpc.Models.WeatherForecast;

namespace BlazorWebAppGrpc.Services
{
    public class WeatherService : WeatherForcastService.WeatherForcastServiceBase
    {
        public override async Task<GetWeatherForcastsResponse> GetWeatherForcasts(GetWeatherForcastsRequest request, ServerCallContext context)
        {
            await Task.Delay(500);

            var startDate = DateOnly.FromDateTime(DateTime.Now);
            var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
            var forecasts = WeatherForecasts();
            var result = new GetWeatherForcastsResponse();
            result.List.AddRange(forecasts.Select(forecast => new Weather()
            {
                Date = forecast.Date.ToString(),
                TemperatureC = forecast.TemperatureC,
                Summary = forecast.Summary,
            }));
            return result;
        }

        public override async Task SubstcribeWeatherForcasts(Empty request, IServerStreamWriter<GetWeatherForcastsResponse> responseStream, ServerCallContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var forecasts = WeatherForecasts();
                var result = new GetWeatherForcastsResponse();
                result.List.AddRange(forecasts.Select(forecast => new Weather()
                {
                    Date = forecast.Date.ToString(),
                    TemperatureC = forecast.TemperatureC,
                    Summary = forecast.Summary,
                }));
                await responseStream.WriteAsync(result);
                Console.WriteLine("Server sent data");
                await Task.Delay(1000);
            }
        }

        private static WeatherForecast[] WeatherForecasts()
        {

            var startDate = DateOnly.FromDateTime(DateTime.Now);
            var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast()
            {
                Date = startDate.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)]
            }).ToArray();
            return forecasts;
        }


        public static GrpcChannel CreateChannel()
        {
            var connectionFactory = new NamedPipesConnectionFactory("MyPipeName");
            var socketsHttpHandler = new SocketsHttpHandler
            {
                ConnectCallback = connectionFactory.ConnectAsync
            };

            return GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
            {
                HttpHandler = socketsHttpHandler,
            });
        }
    }



}


