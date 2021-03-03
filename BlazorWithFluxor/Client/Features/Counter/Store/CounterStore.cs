using Fluxor;

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

    public class CounterIncrementAction {}

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
    }

    // Effect Methods
}
