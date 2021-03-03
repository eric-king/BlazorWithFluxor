using BlazorWithFluxor.Shared;
using Fluxor;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorWithFluxor.Client.Features.Weather.Store
{
    public record WeatherState 
    {
        public bool Initialized { get; init; }
        public bool Loading { get; init; }
        public WeatherForecast[] Forecasts { get; init; }
    }

    public class WeatherFeature : Feature<WeatherState>
    {
        public override string GetName() => "Weather";

        protected override WeatherState GetInitialState()
        {
            return new WeatherState
            {
                Initialized = false,
                Loading = false,
                Forecasts = Array.Empty<WeatherForecast>()
            };
        }
    }

    public static class WeatherReducers 
    {
        [ReducerMethod]
        public static WeatherState OnSetForecasts(WeatherState state, WeatherSetForecastsAction action) 
        {
            return state with 
            {
                Forecasts = action.Forecasts
            };
        }

        [ReducerMethod]
        public static WeatherState OnSetLoading(WeatherState state, WeatherSetLoadingAction action)
        {
            return state with
            {
                Loading = action.Loading
            };
        }

        [ReducerMethod(typeof(WeatherSetInitializedAction))]
        public static WeatherState OnSetInitialized(WeatherState state)
        {
            return state with
            {
                Initialized = true
            };
        }
    }

    public class WeatherEffects 
    {
        private readonly HttpClient Http;

        public WeatherEffects(HttpClient http)
        {
            Http = http;
        }

        [EffectMethod(typeof(WeatherLoadForecastsAction))]
        public async Task LoadForecasts(IDispatcher dispatcher) 
        {
            dispatcher.Dispatch(new WeatherSetLoadingAction(true));
            var forecasts = await Http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
            dispatcher.Dispatch(new WeatherSetForecastsAction(forecasts));
            dispatcher.Dispatch(new WeatherSetLoadingAction(false));
        }
    }

    #region WeatherActions
    public class WeatherSetInitializedAction { }
    public class WeatherLoadForecastsAction { }
    public class WeatherSetForecastsAction
    {
        public WeatherForecast[] Forecasts { get; }

        public WeatherSetForecastsAction(WeatherForecast[] forecasts)
        {
            Forecasts = forecasts;
        }
    }

    public class WeatherSetLoadingAction
    {
        public bool Loading { get; }

        public WeatherSetLoadingAction(bool loading)
        {
            Loading = loading;
        }
    }
    #endregion
}
