using System.Collections.ObjectModel;
using PqsTracker.Desktop.Models;

namespace PqsTracker.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    // ObservableCollection raises its own change notifications for
    // add/remove, so this doesn't need [ObservableProperty] — only the
    // collection's contents change, not the property reference itself.
    public ObservableCollection<FakeLineItem> LineItems { get; } =
    [
        new FakeLineItem { Section = "Fundamentals", Number = "101.1", Description = "Discuss the purpose and administrative requirements of the PQS program.", IsRequired = true },
        new FakeLineItem { Section = "Fundamentals", Number = "102.1", Description = "Explain the fundamentals of heat transfer and fluid flow.", IsRequired = true },
        new FakeLineItem { Section = "Systems", Number = "201.1", Description = "Trace the flowpath of the reactor coolant system.", IsRequired = true },
        new FakeLineItem { Section = "Watchstations", Number = "301.1", Description = "Stand a supervised watch under normal operating conditions.", IsRequired = true },
        new FakeLineItem { Section = "Watchstations", Number = "304.1", Description = "Perform a normal reactor plant shutdown, as an advanced watch task.", IsRequired = false },
    ];
}
