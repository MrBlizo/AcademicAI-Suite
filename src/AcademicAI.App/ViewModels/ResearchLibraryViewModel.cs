using System.Collections.ObjectModel;
using System.Windows;
using AcademicAI.Core.Models;
using AcademicAI.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AcademicAI.App.ViewModels;

public partial class ResearchLibraryViewModel : ObservableObject
{
    [ObservableProperty] private string _searchQuery = "";
    [ObservableProperty] private bool _isSearching;
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _hasSearched;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<SearchResult> Results { get; } = [];

    public bool HasResults => Results.Count > 0;

    private CancellationTokenSource? _cts;
    private readonly ResearchScraperService _scraper = new();

    public ResearchLibraryViewModel()
    {
        Results.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasResults));
        };
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || IsSearching) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsSearching = true;
        ErrorMessage = "";
        StatusMessage = "Searching Anna's Archive & Library Genesis...";

        try
        {
            var results = await _scraper.SearchAllAsync(SearchQuery.Trim(), token);

            token.ThrowIfCancellationRequested();

            Results.Clear();
            foreach (var result in results)
            {
                Results.Add(result);
            }

            HasSearched = true;
            StatusMessage = Results.Count > 0
                ? $"Found {Results.Count} results from Anna's Archive & LibGen"
                : "";
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Search failed: {ex.Message}";
            StatusMessage = "";
        }
        finally
        {
            IsSearching = false;
            _cts = null;
        }
    }

    [RelayCommand]
    private void Cancel() { _cts?.Cancel(); }

    [RelayCommand]
    private void CopyCitation(SearchResult result)
    {
        if (result != null && !string.IsNullOrEmpty(result.Citation))
            Clipboard.SetText(result.Citation);
    }
}
