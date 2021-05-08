using Fluxor;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace BlazorWithFluxor.Client.Features.Hubs.CounterHub
{
    public record CounterHubState
    {
        public bool Connected { get; init; }
    };

    public class CounterHubFeature : Feature<CounterHubState>
    {
        public override string GetName() => "CounterHub";

        protected override CounterHubState GetInitialState()
        {
            return new CounterHubState 
            {
                Connected = false
            };
        }
    }

    public static class CounterHubReducers 
    {
        [ReducerMethod]
        public static CounterHubState OnSetConnected(CounterHubState state, CounterHubSetConnectedAction action) 
        {
            return state with
            {
                Connected = action.Connected
            };
        }
    }

    public class CounterHubEffects
    {
        private readonly HubConnection _hubConnection;

        public CounterHubEffects(NavigationManager navigationManager)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(navigationManager.ToAbsoluteUri("/counterhub"))
                .WithAutomaticReconnect()
                .Build();
        }

        [EffectMethod]
        public async Task SendCount(CounterHubSendCountAction action, IDispatcher dispatcher)
        {
            try
            {
                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    await _hubConnection.SendAsync("SendCount", action.Count);
                }
                else 
                {
                    dispatcher.Dispatch(new CounterHubSendCountFailedAction("Not connected to hub."));
                }
            }
            catch (Exception ex)
            {
                dispatcher.Dispatch(new CounterHubSendCountFailedAction(ex.Message));
            }
        }

        [EffectMethod(typeof(CounterHubStartAction))]
        public async Task Start(IDispatcher dispatcher) 
        {
            await _hubConnection.StartAsync();

            _hubConnection.Reconnecting += (ex) => 
            {
                dispatcher.Dispatch(new CounterHubSetConnectedAction(false));
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += (connectionId) =>
            {
                dispatcher.Dispatch(new CounterHubSetConnectedAction(true));
                return Task.CompletedTask;
            };

            _hubConnection.On<int>("ReceiveCount", (count) => dispatcher.Dispatch(new CounterHubReceiveCountAction(count)));

            dispatcher.Dispatch(new CounterHubSetConnectedAction(true));
        }
    }

    public record CounterHubSetConnectedAction(bool Connected);
    public record CounterHubStartAction();
    public record CounterHubReceiveCountAction(int Count);
    public record CounterHubSendCountAction(int Count);
    public record CounterHubSendCountFailedAction(string Message);

}
