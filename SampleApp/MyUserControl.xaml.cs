﻿using CommunityToolkit.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

namespace SampleApp;

/// <summary>
/// Interaction logic for MyUserControl.xaml
/// </summary>
[InjectDependenciesFromDefaultConstructor]
public partial class MyUserControl
{
    public IMessenger Messenger { get; }

    public MyUserControl(MyUserControlViewModel viewModel, IMessenger messenger)
    {
        DataContext = viewModel;
        Messenger = messenger;

        InitializeComponent();
    }
}