## Proposed addition to CommunityToolkit
This repository illustrates a proof-of-concept of a feature which I hope could end up being part of the [.NET Community Toolkit](https://github.com/CommunityToolkit/dotnet).


## Problem to solve
The problem described below is probably the most compelling use case for this feature, however I do see other use cases for it as well.

Consider a simple WPF application using the `GenericHost` to enable dependency injection following the standard .NET scheme.
The approach is nicely described in [this video](https://www.youtube.com/watch?v=j3pl2tkBM1A&t=6s) by [@keboo](https://github.com/Keboo).

This allows us to [extract the "top-most" view from the DI container](https://github.com/nicolaihenriksen/CommunityToolkit.DependencyInjection/blob/26eda98270d5d0393ea3a07613b6bcf165fd5b16/SampleApp/App.xaml.cs#L40) (and thus use a non-default constructor). However, nested controls inside this view are problematic,
because they are "instantiated by the XAML parser" and thus require an empty default constructor.

Consider a "top-most" view called `MainWindow` using a `UserControl` (or a custom control):

**MainWindow.xaml**
```xaml
<Window x:Class="SampleApp.MainWindow" ...>
  <local:MyUserControl />
</Window>
```

We now want the nested view (i.e. `MyUserControl`) to have a non-default constructor so we can use dependency injection:

**MyUserControl.xaml.cs** (code-behind)
```csharp
public class MyUserControl : UserControl
{
  private readonly IMessenger _messenger;

  public MyUserControl(IMessenger messenger) // This does not work out of the box - default ctor is needed!
  {
    _messenger = messenger;
  }
}
```

As mentioned, this will not work out of the box, because there needs to be an empty default constructor.

This is just one example of the issue. I could easily imagine other scenarios where this could be useful. For example a "plugin architecture" scenario where plugins are instantiated via reflection using the default constructor.

## Proposed solution
The general idea is quite simple:
> Use an incremental source generator to create an empty default constructor which in turn invokes the DI-enabled constructor by looking up the dependencies in the DI container.

For this to work, basically 2 things are needed:
1. An incremental source generator producing the empty default constructor for (partial) types decorated with a marker attribute.
2. A means of getting (static) access to the `IServiceProvider` from the generated constructors in order to lookup the required services.

#### Desired usage
I want the usage of this to be as simple as possible. Hopefully only 2 steps are needed:
1. Decorate the type (e.g. `MyUserControl`) which should be "DI-enabled" with a marker attribute.
2. Inject the "necessary stuff" into the `IHostBuilder` using a simple extension method.

Ideally something like this:
```csharp
[InjectDependenciesFromDefaultConstructor]  // This instructs the source generator to generate an empty constructor
public class MyUserControl : UserControl
...
```

```csharp
using IHost host = CreateHostBuilder(args)
                     .UseSourceGeneratedDefaultConstructors()   // This registers the static access to the IServiceProvider
                     .Build();
```

## Disclaimer
I have never written an incremental source generator before, so this is a very naive approach - heavily inspired by Andrew Locks [initial blog post](https://andrewlock.net/creating-a-source-generator-part-1-creating-an-incremental-source-generator/) - and probably needs a lot of refactoring
to be fast and effective. However, it does the job for the example I have provided, and showcases what the desired end result should be.

## Known issues
- [ ] Naming is hard! Proposals for better named types are always welcome!
- [ ] The `HostBuilderExtensions.UseSourceGeneratedDefaultConstructors()` does not work as desired. Kind of a "chicken and the egg" problem.
  - Current workaround is to use `HostBuilderExtensions.BuildWithSourceGeneratedDefaultConstructors()` instead
(effectively replacing the `.Build()` call), which deviates from the standard way of using the `IHostBuilder`.
- [ ] The code assumes all services should be pulled from the "root scope" of the DI container.
- [ ] The code assumes there is only ever one instance of the required services.

## Unanswered questions
* The current implementation uses a custom `ServiceProviderAccessor`, would it be possible to use the `Microsoft.Extensions.DependencyInjection.ActivatorUtilities` instead?
* Is there a way to "clear the source generator cache". Currently when I change the source generator, I need to restart Visual Studio
in order for the changes to take effect.
* Is it possible to ("unit") test source generators?