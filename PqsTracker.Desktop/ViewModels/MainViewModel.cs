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

    // AsyncRelayCommand is a plain class from CommunityToolkit.Mvvm.Input —
    // constructed directly here instead of via the [RelayCommand] generator.
    public IAsyncRelayCommand LoadProgressCommand { get; }
    public IAsyncRelayCommand SignOffCommand { get; }

    public MainViewModel()
    {
        LoadProgressCommand = new AsyncRelayCommand(LoadProgressAsync);
        SignOffCommand = new AsyncRelayCommand(SignOffAsync);
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

            LineItems.Clear();
            foreach (var li in progress.OutstandingLineItems) LineItems.Add(li);

            StatusMessage = $"{progress.QualificationName}: {progress.Completed}/{progress.TotalRequired} " +
                             $"({progress.PercentComplete}%) complete" + (progress.IsComplete ? " — COMPLETE" : "");
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
}
