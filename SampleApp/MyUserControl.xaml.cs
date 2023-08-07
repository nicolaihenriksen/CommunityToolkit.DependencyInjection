using System.Windows.Controls;

namespace SampleApp;

/// <summary>
/// Interaction logic for MyUserControl.xaml
/// </summary>
public partial class MyUserControl : UserControl
{
    public MyUserControl()
    {
        DataContext = new MyUserControlViewModel();
        InitializeComponent();
    }
}