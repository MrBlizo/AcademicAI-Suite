using System.Collections.ObjectModel;
using AcademicAI.Academic;
using AcademicAI.Core.Interfaces;
using AcademicAI.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicAI.App.ViewModels;

public partial class StudyHubViewModel : ObservableObject
{
    [ObservableProperty] private string _topic = "";
    [ObservableProperty] private int _cardCount = 5;
    [ObservableProperty] private bool _isGenerating;
    [ObservableProperty] private double _deckProgress;
    [ObservableProperty] private int _currentCardIndex;
    [ObservableProperty] private FlashCard? _currentCard;
    [ObservableProperty] private bool _isAnswerRevealed;

    public ObservableCollection<FlashCard> Cards { get; } = [];

    public bool HasCards => Cards.Count > 0;

    public int AgainCount => Cards.Count(c => c.Rating == "Again");
    public int HardCount => Cards.Count(c => c.Rating == "Hard");
    public int EasyCount => Cards.Count(c => c.Rating == "Easy");
    public int NewCount => Cards.Count(c => c.Rating == "New");

    private CancellationTokenSource? _cts;

    public StudyHubViewModel()
    {
        Cards.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasCards));
    }

    partial void OnCurrentCardIndexChanged(int value)
    {
        UpdateCurrentCard();
    }

    private void UpdateCurrentCard()
    {
        if (Cards.Count == 0)
        {
            CurrentCard = null;
            DeckProgress = 0;
            return;
        }

        CurrentCardIndex = Math.Clamp(CurrentCardIndex, 0, Cards.Count - 1);
        CurrentCard = Cards[CurrentCardIndex];
        IsAnswerRevealed = false;
        DeckProgress = (double)CurrentCardIndex / Cards.Count * 100;
        OnPropertyChanged(nameof(AgainCount));
        OnPropertyChanged(nameof(HardCount));
        OnPropertyChanged(nameof(EasyCount));
        OnPropertyChanged(nameof(NewCount));
    }

    [RelayCommand]
    private async Task GenerateCards()
    {
        if (string.IsNullOrWhiteSpace(Topic) || IsGenerating) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsGenerating = true;
        try
        {
            var processor = App.Services.GetRequiredService<AcademicTextProcessor>();
            var result = await processor.GenerateFlashcardsAsync(Topic, CardCount);

            token.ThrowIfCancellationRequested();

            Cards.Clear();
            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            FlashCard? current = null;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("Q:", StringComparison.OrdinalIgnoreCase))
                {
                    if (current != null) Cards.Add(current);
                    current = new FlashCard { Question = trimmed[2..].Trim() };
                }
                else if (trimmed.StartsWith("A:", StringComparison.OrdinalIgnoreCase) && current != null)
                {
                    current.Answer = trimmed[2..].Trim();
                }
                else if (trimmed.StartsWith("H:", StringComparison.OrdinalIgnoreCase) && current != null)
                {
                    current.Hint = trimmed[2..].Trim();
                }
            }
            if (current != null) Cards.Add(current);

            CurrentCardIndex = 0;
            UpdateCurrentCard();
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            var notificationService = App.Services.GetRequiredService<INotificationService>();
            notificationService.Show("Error", $"Failed to generate cards: {ex.Message}");
        }
        finally
        {
            IsGenerating = false;
            _cts = null;
        }
    }

    [RelayCommand]
    private void Cancel() { _cts?.Cancel(); }

    [RelayCommand]
    private void NextCard()
    {
        if (Cards.Count == 0) return;
        if (CurrentCardIndex < Cards.Count - 1) CurrentCardIndex++;
        UpdateCurrentCard();
    }

    [RelayCommand]
    private void PrevCard()
    {
        if (Cards.Count == 0) return;
        if (CurrentCardIndex > 0) CurrentCardIndex--;
        UpdateCurrentCard();
    }

    [RelayCommand]
    private void RevealAnswer()
    {
        IsAnswerRevealed = true;
    }

    [RelayCommand]
    private void RateAgain()
    {
        if (CurrentCard == null) return;
        CurrentCard.Rating = "Again";
        CurrentCard.ReviewCount++;
        OnPropertyChanged(nameof(AgainCount));
        NextCard();
    }

    [RelayCommand]
    private void RateHard()
    {
        if (CurrentCard == null) return;
        CurrentCard.Rating = "Hard";
        CurrentCard.ReviewCount++;
        OnPropertyChanged(nameof(HardCount));
        NextCard();
    }

    [RelayCommand]
    private void RateEasy()
    {
        if (CurrentCard == null) return;
        CurrentCard.Rating = "Easy";
        CurrentCard.ReviewCount++;
        OnPropertyChanged(nameof(EasyCount));
        NextCard();
    }
}
