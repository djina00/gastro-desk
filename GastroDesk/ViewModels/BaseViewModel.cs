using CommunityToolkit.Mvvm.ComponentModel;

namespace GastroDesk.ViewModels
{
    /// <summary>
    /// Base class for all ViewModels implementing INotifyPropertyChanged.
    /// Uses the Observer pattern through property change notifications.
    /// </summary>
    public abstract class BaseViewModel : ObservableObject
    {
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        protected void ClearError()
        {
            ErrorMessage = null;
        }

        protected void SetError(string message)
        {
            ErrorMessage = message;
        }
    }
}
