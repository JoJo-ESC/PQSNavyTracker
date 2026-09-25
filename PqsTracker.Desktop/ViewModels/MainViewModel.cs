using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using PqsTracker.Desktop.Models;
using PqsTracker.Desktop.Services;

namespace PqsTracker.Desktop.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly PqsApiClient _api = new();

    public ObservableCollection<TraineeSummaryDto> Trainees { get; } = [];
    public ObservableCollection<QualificationSummaryDto> Qualifications { get; } = [];
    public ObservableCollection<LineItemDto> LineItems { get; } = [];
    public ObservableCollection<SignOffAuditDto> History { get; } = [];

    // Hand-written INotifyPropertyChanged, via ObservableObject.SetProperty
    // (from ViewModelBase), rather than the [ObservableProperty] source
    // generator — sidesteps a Roslyn/analyzer version mismatch in this
    // environment, and it's exactly what that attribute expands to anyway.
    private TraineeSummaryDto? _selectedTrainee;
    public TraineeSummaryDto? SelectedTrainee
    {
        get => _selectedTrainee;
        set => SetProperty(ref _selectedTrainee, value);
    }

    private QualificationSummaryDto? _selectedQualification;
    public QualificationSummaryDto? SelectedQualification
    {
        get => _selectedQualification;
        set => SetProperty(ref _selectedQualification, value);
    }

    private string _statusMessage = "";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    // Separate from StatusMessage on purpose: this persists as a standing
    // display of current standing, while StatusMessage is transient
    // success/error feedback from the last action.
    private string _completionSummary = "";
    public string CompletionSummary
    {
        get => _completionSummary;
        set => SetProperty(ref _completionSummary, value);
    }

    // Numeric counterpart to CompletionSummary, for the progress bar —
    // kept separate since a ProgressBar needs a plain double, not text.
    private double _completionPercent;
    public double CompletionPercent
    {
        get => _completionPercent;
        set => SetProperty(ref _completionPercent, value);
    }

    // Who is performing the sign-off — deliberately a separate selection
    // from SelectedTrainee (who's being signed off), so picking the same
    // person for both is possible in the UI and gets rejected by the API's
    // self-sign rule rather than being blocked client-side.
    private TraineeSummaryDto? _selectedQualifier;
    public TraineeSummaryDto? SelectedQualifier
    {
        get => _selectedQualifier;
        set => SetProperty(ref _selectedQualifier, value);
    }

    // Bound to the DataGrid's SelectedItem — which outstanding line item
    // "Sign Off Selected" acts on.
    private LineItemDto? _selectedLineItem;
    public LineItemDto? SelectedLineItem
    {
        get => _selectedLineItem;
        set => SetProperty(ref _selectedLineItem, value);
    }

    // Bound to the History DataGrid's SelectedItem — which sign-off
    // "Revoke Selected" acts on.
    private SignOffAuditDto? _selectedHistoryEntry;
    public SignOffAuditDto? SelectedHistoryEntry
    {
        get => _selectedHistoryEntry;
        set => SetProperty(ref _selectedHistoryEntry, value);
    }

    private string _revokeReason = "";
    public string RevokeReason
    {
        get => _revokeReason;
        set => SetProperty(ref _revokeReason, value);
    }

    // AsyncRelayCommand is a plain class from CommunityToolkit.Mvvm.Input —
    // constructed directly here instead of via the [RelayCommand] generator.
    public IAsyncRelayCommand LoadProgressCommand { get; }
    public IAsyncRelayCommand SignOffCommand { get; }
    public IAsyncRelayCommand RevokeCommand { get; }

    public MainViewModel()
    {
        LoadProgressCommand = new AsyncRelayCommand(LoadProgressAsync);
        SignOffCommand = new AsyncRelayCommand(SignOffAsync);
        RevokeCommand = new AsyncRelayCommand(RevokeAsync);
    }

    // Called once from MainWindow's Loaded event — see the "why not the
    // constructor" note in MainWindow.axaml.cs.
    public async Task InitializeAsync()
    {
        try
        {
            var trainees = await _api.GetTraineesAsync();
            var qualifications = await _api.GetQualificationsAsync();

            Trainees.Clear();
            foreach (var t in trainees) Trainees.Add(t);

            Qualifications.Clear();
            foreach (var q in qualifications) Qualifications.Add(q);

            StatusMessage = $"Loaded {Trainees.Count} trainees, {Qualifications.Count} qualifications.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load: {ex.Message}";
        }
    }

    private async Task LoadProgressAsync()
    {
        if (SelectedTrainee is null || SelectedQualification is null)
        {
            StatusMessage = "Select a trainee and a qualification first.";
            return;
        }

        try
        {
            var progress = await _api.GetProgressAsync(SelectedTrainee.Id, SelectedQualification.Id);
            var history = await _api.GetSignOffHistoryAsync(SelectedTrainee.Id, SelectedQualification.Id);

            LineItems.Clear();
            foreach (var li in progress.OutstandingLineItems) LineItems.Add(li);

            History.Clear();
            foreach (var h in history) History.Add(h);

            CompletionSummary = $"{progress.QualificationName}: {progress.Completed}/{progress.TotalRequired} " +
                                 $"({progress.PercentComplete}%) complete" + (progress.IsComplete ? " — COMPLETE" : "");
            CompletionPercent = progress.PercentComplete;
            StatusMessage = "";
        }
        catch (Exception ex)
        {
            // A rejected request (e.g. a bad id) shows up here as a message,
            // not a crash.
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task SignOffAsync()
    {
        if (SelectedTrainee is null || SelectedQualifier is null || SelectedLineItem is null)
        {
            StatusMessage = "Select a trainee, a qualifier, and a line item to sign off.";
            return;
        }

        try
        {
            await _api.CreateSignOffAsync(SelectedLineItem.Id, SelectedTrainee.Id, SelectedQualifier.Id);
            StatusMessage = $"Signed off {SelectedLineItem.Number} for {SelectedTrainee.Name}.";
            // Refresh so the grid reflects the new state immediately —
            // same "recompute, don't trust stale state" principle as the
            // backend's progress calculation.
            await LoadProgressAsync();
        }
        catch (Exception ex)
        {
            // Exactly the case the plan calls out: a rejected self-sign-off
            // (or any other business rule violation) shows up here as a
            // message, not a crash.
            StatusMessage = $"Sign-off rejected: {ex.Message}";
        }
    }

    private async Task RevokeAsync()
    {
        if (SelectedHistoryEntry is null)
        {
            StatusMessage = "Select a sign-off in the history grid to revoke.";
            return;
        }

        if (SelectedHistoryEntry.IsRevoked)
        {
            StatusMessage = "That sign-off is already revoked.";
            return;
        }

        if (string.IsNullOrWhiteSpace(RevokeReason))
        {
            StatusMessage = "Enter a reason before revoking.";
            return;
        }

        try
        {
            await _api.RevokeSignOffAsync(SelectedHistoryEntry.Id, RevokeReason);
            StatusMessage = $"Revoked {SelectedHistoryEntry.Number}.";
            RevokeReason = "";
            // Same refresh-after-write principle as SignOffAsync — the
            // history grid picks up the revocation immediately.
            await LoadProgressAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Revoke rejected: {ex.Message}";
        }
    }
}
