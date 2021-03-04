using BlazorWithFluxor.Shared;
using Fluxor;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazorWithFluxor.Client.Features.UserFeedback.Store
{
    public record UserFeedbackState 
    {
        public bool Submitting { get; init; }
        public bool Submitted { get; init; }
        public string ErrorMessage { get; init; }
        public UserFeedbackModel Model { get; init; }
    }

    public class UserFeedbackFeature : Feature<UserFeedbackState>
    {
        public override string GetName() => "UserFeedback";

        protected override UserFeedbackState GetInitialState()
        {
            return new UserFeedbackState 
            {
                Submitting = false,
                Submitted = false,
                ErrorMessage = string.Empty,
                Model = new UserFeedbackModel()
            };
        }
    }

    public static class UserFeedbackReducers 
    {
        [ReducerMethod]
        public static UserFeedbackState OnSetSubmitting(UserFeedbackState state, UserFeedbackSetSubmittingAction action) 
        {
            return state with
            {
                Submitting = action.Submitting
            };
        }

        [ReducerMethod]
        public static UserFeedbackState OnSetSubmitted(UserFeedbackState state, UserFeedbackSetSubmittedAction action)
        {
            return state with
            {
                Submitted = action.Submitted
            };
        }

        [ReducerMethod]
        public static UserFeedbackState OnSetErrorMessage(UserFeedbackState state, UserFeedbackSetErrorMessageAction action)
        {
            return state with
            {
                ErrorMessage = action.ErrorMessage
            };
        }
    }

    public class UserFeedbackEffects 
    {
        private readonly HttpClient _httpClient;
        public UserFeedbackEffects(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [EffectMethod]
        public async Task SubmitUserFeedback(UserFeedbackSubmitAction action, IDispatcher dispatcher) 
        {
            dispatcher.Dispatch(new UserFeedbackSetSubmittingAction(true));
            
            await Task.Delay(500); // just so we can see the "submitting" message
            
            var response = await _httpClient.PostAsJsonAsync("Feedback", action.UserFeedbackModel);

            if (response.IsSuccessStatusCode)
            {
                dispatcher.Dispatch(new UserFeedbackSetSubmittedAction(true));
                dispatcher.Dispatch(new UserFeedbackSetSubmittingAction(false));
            }
            else 
            {
                dispatcher.Dispatch(new UserFeedbackSetErrorMessageAction(response.ReasonPhrase));
                dispatcher.Dispatch(new UserFeedbackSetSubmittingAction(false));
            }            
        }
    }

    #region UserFeedbackActions
    public class UserFeedbackSetSubmittingAction 
    {
        public bool Submitting { get; }

        public UserFeedbackSetSubmittingAction(bool submitting)
        {
            Submitting = submitting;
        }
    }

    public class UserFeedbackSetSubmittedAction
    {
        public bool Submitted { get; }

        public UserFeedbackSetSubmittedAction(bool submitted)
        {
            Submitted = submitted;
        }
    }

    public class UserFeedbackSetErrorMessageAction
    {
        public string ErrorMessage { get; }

        public UserFeedbackSetErrorMessageAction(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }

    public class UserFeedbackSubmitAction 
    {
        public UserFeedbackModel UserFeedbackModel { get; }

        public UserFeedbackSubmitAction(UserFeedbackModel userFeedbackModel)
        {
            UserFeedbackModel = userFeedbackModel;
        }
    }
    #endregion 
}
