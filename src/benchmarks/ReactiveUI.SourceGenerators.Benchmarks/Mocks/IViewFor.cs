using ReactiveUI.Reactive;
using ReactiveUI.SourceGenerators;

namespace Mocks.IViewFor__N__;

public sealed class LoginViewModel : ReactiveObject
{
}

public sealed class DashboardViewModel : ReactiveObject
{
}

[IViewFor<LoginViewModel>]
public partial class LoginView : System.Windows.Controls.UserControl
{
}

[IViewFor<DashboardViewModel>]
public partial class DashboardView : System.Windows.Forms.UserControl
{
}

[IViewFor("Mocks.IViewFor__N__.LoginViewModel")]
public partial class LoginWindow : System.Windows.Window
{
}
