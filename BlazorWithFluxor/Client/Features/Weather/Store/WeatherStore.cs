using Blazored.LocalStorage;
using BlazorWithFluxor.Client.Features.Counter.Store;
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

        [ReducerMethod]
        public static WeatherState OnWeatherSetState(WeatherState state, WeatherSetStateAction action)
        {
            return action.WeatherState;
        }
    }

    public class WeatherEffects 
    {
        private readonly HttpClient Http;
        private readonly IState<CounterState> CounterState;
        private readonly ILocalStorageService _localStorageService;
        private const string WeatherStatePersistenceName = "BlazorWithFluxor_WeatherState";

        public WeatherEffects(HttpClient http, IState<CounterState> counterState, ILocalStorageService localStorageService)
        {
            Http = http;
            CounterState = counterState;
            _localStorageService = localStorageService;
        }

        [EffectMethod(typeof(WeatherLoadForecastsAction))]
        public async Task LoadForecasts(IDispatcher dispatcher) 
        {
            var forecasts = await Http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast");
            dispatcher.Dispatch(new WeatherSetForecastsAction(forecasts));
            dispatcher.Dispatch(new WeatherLoadForecastsSuccessAction());
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

        [EffectMethod]
        public async Task PersistState(WeatherPersistStateAction action, IDispatcher dispatcher)
        {
            try
            {
                await _localStorageService.SetItemAsync(WeatherStatePersistenceName, action.WeatherState);
                dispatcher.Dispatch(new WeatherPersistStateSuccessAction());
            }
            catch (Exception ex)
            {
                dispatcher.Dispatch(new WeatherPersistStateFailureAction(ex.Message));
            }
        }

        [EffectMethod(typeof(WeatherLoadStateAction))]
        public async Task LoadState(IDispatcher dispatcher)
        {
            try
            {
                var weatherState = await _localStorageService.GetItemAsync<WeatherState>(WeatherStatePersistenceName);
                if (weatherState is not null)
                {
                    dispatcher.Dispatch(new WeatherSetStateAction(weatherState));
                    dispatcher.Dispatch(new WeatherLoadStateSuccessAction());
                }
            }
            catch (Exception ex)
            {
                dispatcher.Dispatch(new WeatherLoadStateFailureAction(ex.Message));
            }
        }

        [EffectMethod(typeof(WeatherClearStateAction))]
        public async Task ClearState(IDispatcher dispatcher)
        {
            try
            {
                await _localStorageService.RemoveItemAsync(WeatherStatePersistenceName);
                dispatcher.Dispatch(new WeatherSetStateAction(new WeatherState 
                {
                    Initialized = false,
                    Loading = false,
                    Forecasts = Array.Empty<WeatherForecast>()
                }));
                dispatcher.Dispatch(new WeatherClearStateSuccessAction());
            }
            catch (Exception ex)
            {
                dispatcher.Dispatch(new WeatherClearStateFailureAction(ex.Message));
            }
        }
    }

    #region WeatherActions
    public class WeatherSetInitializedAction { }
    public class WeatherLoadForecastsAction { }
    public class WeatherLoadForecastsSuccessAction { }
    public class WeatherSetForecastsAction
    {
        public WeatherForecast[] Forecasts { get; }

        public WeatherSetForecastsAction(WeatherForecast[] forecasts)
        {
            Forecasts = forecasts;
        }
    }

    public class WeatherSetStateAction
    {
        public WeatherState WeatherState { get; }
        public WeatherSetStateAction(WeatherState weatherState)
        {
            WeatherState = weatherState;
        }
    }

    public class WeatherLoadStateAction { }
    public class WeatherLoadStateSuccessAction { }
    public class WeatherLoadStateFailureAction
    {
        public string ErrorMessage { get; }
        public WeatherLoadStateFailureAction(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }

    public class WeatherPersistStateAction
    {
        public WeatherState WeatherState { get; }
        public WeatherPersistStateAction(WeatherState weatherState)
        {
            WeatherState = weatherState;
        }
    }
    public class WeatherPersistStateSuccessAction { }
    public class WeatherPersistStateFailureAction
    {
        public string ErrorMessage { get; }
        public WeatherPersistStateFailureAction(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }

    public class WeatherClearStateAction { }
    public class WeatherClearStateSuccessAction { }
    public class WeatherClearStateFailureAction
    {
        public string ErrorMessage { get; }
        public WeatherClearStateFailureAction(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
    #endregion
}
