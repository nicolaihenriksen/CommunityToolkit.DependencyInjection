using CommunityToolkit.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

namespace SampleApp;

/// <summary>
/// Interaction logic for MyUserControl.xaml
/// </summary>
[InjectDependenciesFromDefaultConstructor]
public partial class MyUserControl
{
    private readonly IMessenger _messenger;

    public MyUserControl(MyUserControlViewModel viewModel, IMessenger messenger)
    {
        DataContext = viewModel;
        _messenger = messenger;

        InitializeComponent();
    }
}