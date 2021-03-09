﻿using BlazorWithFluxor.Client.Features.Counter.Store;
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
                Forecasts = action.Forecasts,
                Loading = false
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

        [ReducerMethod(typeof(WeatherLoadForecastsAction))]
        public static WeatherState OnLoadForecasts(WeatherState state)
        {
            return state with
            {
                Loading = true
            };
        }
    }

    public class WeatherEffects 
    {
        private readonly HttpClient Http;
        private readonly IState<CounterState> CounterState;

        public WeatherEffects(HttpClient http, IState<CounterState> counterState)
        {
            Http = http;
            CounterState = counterState;
        }

        [EffectMethod(typeof(WeatherLoadForecastsAction))]
        public async Task LoadForecasts(IDispatcher dispatcher) 
        {
            var forecasts = await Http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
            dispatcher.Dispatch(new WeatherSetForecastsAction(forecasts));
        }

        [EffectMethod(typeof(CounterIncrementAction))]
        public async Task LoadForecastsOnIncrement(IDispatcher dispatcher) 
        {
            await Task.Delay(0);
            if (CounterState.Value.CurrentCount % 10 == 0) 
            {
                dispatcher.Dispatch(new WeatherLoadForecastsAction());
            }
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
    #endregion
}
