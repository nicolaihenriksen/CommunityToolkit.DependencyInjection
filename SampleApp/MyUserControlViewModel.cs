using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SampleApp;

internal partial class MyUserControlViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DecrementCountCommand))]
    private int _count = 100;

    [RelayCommand(CanExecute = nameof(CanDecrementCount))]
    private void DecrementCount()
    {
        Count--;
    }

    private bool CanDecrementCount() => Count > 0;
}