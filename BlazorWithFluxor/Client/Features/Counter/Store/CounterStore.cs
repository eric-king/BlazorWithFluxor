using Blazored.LocalStorage;
using Fluxor;
using System;
using System.Threading.Tasks;

namespace BlazorWithFluxor.Client.Features.Counter.Store
{
    public record CounterState
    {
        public int CurrentCount { get; init; }
    }

    public class CounterFeature : Feature<CounterState>
    {
        public override string GetName() => "Counter";

        protected override CounterState GetInitialState()
        {
            return new CounterState
            {
                CurrentCount = 0
            };
        }
    }

    public static class CounterReducers
    {
        [ReducerMethod(typeof(CounterIncrementAction))]
        public static CounterState OnIncrement(CounterState state)
        {
            return state with
            {
                CurrentCount = state.CurrentCount + 1
            };
        }

        [ReducerMethod]
        public static CounterState OnCounterSetState(CounterState state, CounterSetStateAction action) 
        {
            return action.CounterState;
        }
    }

    public class CounterEffects
    {
        private readonly ILocalStorageService _localStorageService;
        private const string CounterStatePersistenceName = "BlazorWithFluxor_CounterState";

        public CounterEffects(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        [EffectMethod]
        public async Task PersistState(CounterPersistStateAction action, IDispatcher dispatcher) 
        {
            try
            {
                await _localStorageService.SetItemAsync(CounterStatePersistenceName, action.CounterState);
                dispatcher.Dispatch(new CounterPersistStateSuccessAction());
            }
            catch (Exception ex) 
            {
                dispatcher.Dispatch(new CounterPersistStateFailureAction(ex.Message));
            }
        }

        [EffectMethod(typeof(CounterLoadStateAction))]
        public async Task LoadState(IDispatcher dispatcher)
        {
            try
            {
                var counterState = await _localStorageService.GetItemAsync<CounterState>(CounterStatePersistenceName);
                if (counterState is not null) 
                {
                    dispatcher.Dispatch(new CounterSetStateAction(counterState));
                    dispatcher.Dispatch(new CounterLoadStateSuccessAction());
                }
            }
            catch (Exception ex)
            {
                dispatcher.Dispatch(new CounterLoadStateFailureAction(ex.Message));
            }
        }

        [EffectMethod(typeof(CounterClearStateAction))]
        public async Task ClearState(IDispatcher dispatcher)
        {
            try
            {
                await _localStorageService.RemoveItemAsync(CounterStatePersistenceName);
                dispatcher.Dispatch(new CounterSetStateAction(new CounterState { CurrentCount = 0 }));
                dispatcher.Dispatch(new CounterClearStateSuccessAction());
            }
            catch (Exception ex)
            {
                dispatcher.Dispatch(new CounterClearStateFailureAction(ex.Message));
            }
        }
    }

    #region CounterActions
    public class CounterIncrementAction { }
    
    public class CounterSetStateAction 
    {
        public CounterState CounterState { get; }
        public CounterSetStateAction(CounterState counterState)
        {
            CounterState = counterState;
        }
    }

    public class CounterPersistStateAction
    {
        public CounterState CounterState { get; }
        public CounterPersistStateAction(CounterState counterState)
        {
            CounterState = counterState;
        }
    }
    public class CounterPersistStateSuccessAction { }
    public class CounterPersistStateFailureAction
    {
        public string ErrorMessage { get; }
        public CounterPersistStateFailureAction(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }

    public class CounterLoadStateAction { }
    public class CounterLoadStateSuccessAction { }
    public class CounterLoadStateFailureAction
    {
        public string ErrorMessage { get; }
        public CounterLoadStateFailureAction(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }

    public class CounterClearStateAction { }
    public class CounterClearStateSuccessAction { }
    public class CounterClearStateFailureAction
    {
        public string ErrorMessage { get; }
        public CounterClearStateFailureAction(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
    #endregion
}
