using System.Windows;
using AcademicAI.Academic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicAI.App.ViewModels;

public partial class QuizViewModel : ObservableObject
{
    [ObservableProperty] private string _inputText = "";
    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private string _resultText = "";
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private int _questionCount = 5;
    [ObservableProperty] private string _quizType = "Multiple Choice";

    public List<string> AvailableQuizTypes { get; } = ["Multiple Choice", "True/False", "Short Answer", "Mixed"];

    public bool HasResult => !string.IsNullOrEmpty(ResultText);

    private CancellationTokenSource? _cts;

    [RelayCommand]
    private async Task GenerateQuiz()
    {
        if (string.IsNullOrWhiteSpace(InputText) || IsProcessing) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsProcessing = true;
        ErrorMessage = "";
        ResultText = "";
        try
        {
            var processor = App.Services.GetRequiredService<QuizGenerator>();
            ResultText = await processor.GenerateAsync(InputText, QuestionCount, QuizType);

            token.ThrowIfCancellationRequested();

            OnPropertyChanged(nameof(HasResult));
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorMessage = $"Generating quiz failed: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
            _cts = null;
        }
    }

    [RelayCommand]
    private void Cancel() { _cts?.Cancel(); }

    [RelayCommand]
    private void CopyResult()
    {
        if (!string.IsNullOrEmpty(ResultText))
            Clipboard.SetText(ResultText);
    }
}
