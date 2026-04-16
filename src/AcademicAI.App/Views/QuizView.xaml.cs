using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class QuizView : Page
{
    private readonly QuizViewModel _vm;
    private int _score;
    private int _totalQuestions;
    private int _answered;

    public QuizView()
    {
        InitializeComponent();
        _vm = new QuizViewModel();
        DataContext = _vm;
        _vm.PropertyChanged += Vm_PropertyChanged;
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(QuizViewModel.ResultText))
        {
            RenderQuiz(_vm.ResultText);
        }
    }

    private void RenderQuiz(string rawText)
    {
        QuizQuestionsList.Items.Clear();
        _score = 0;
        _answered = 0;
        _totalQuestions = 0;
        if (string.IsNullOrWhiteSpace(rawText)) return;

        // Split by question blocks
        var questionBlocks = Regex.Split(rawText, @"(?=\*\*Question\s+\d+)", RegexOptions.IgnoreCase);

        foreach (var block in questionBlocks)
        {
            var trimmed = block.Trim();
            if (string.IsNullOrWhiteSpace(trimmed) || trimmed.Length < 10) continue;

            // Extract question text
            var qMatch = Regex.Match(trimmed, @"\*\*Question\s+\d+[.:]*\s*(.+?)\*\*", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (!qMatch.Success) continue;

            var questionText = qMatch.Groups[1].Value.Trim();

            // Extract options (A) B) C) D))
            var options = Regex.Matches(trimmed, @"(?:^|\n)\s*([A-D])\)\s*(.+?)(?=\n|$)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (options.Count == 0) continue;

            // Extract correct answer
            var correctMatch = Regex.Match(trimmed, @"\*\*Correct\s*answer[:\s]*([A-D])\)?", RegexOptions.IgnoreCase);
            var correctLetter = correctMatch.Success ? correctMatch.Groups[1].Value.ToUpper() : "";

            _totalQuestions++;
            var card = CreateQuizCard(_totalQuestions, questionText, options, correctLetter);
            QuizQuestionsList.Items.Add(card);
        }

        UpdateScore();
    }

    private Border CreateQuizCard(int num, string question, MatchCollection options, string correctLetter)
    {
        var panel = new StackPanel { Margin = new Thickness(0) };

        // Question number badge + text
        var headerPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 14) };
        var numBadge = new Border
        {
            Width = 28, Height = 28,
            CornerRadius = new CornerRadius(14),
            Background = new SolidColorBrush(Color.FromArgb(0x26, 0x63, 0x66, 0xF1)),
            Margin = new Thickness(0, 0, 10, 0),
            Child = new TextBlock
            {
                Text = num.ToString(),
                FontSize = 12, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x81, 0x8C, 0xF8)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            }
        };
        headerPanel.Children.Add(numBadge);
        headerPanel.Children.Add(new TextBlock
        {
            Text = question,
            FontSize = 14, FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            Foreground = (Brush)Application.Current.FindResource("TextFillColorPrimaryBrush"),
            VerticalAlignment = VerticalAlignment.Center,
        });
        panel.Children.Add(headerPanel);

        // Feedback text (hidden initially)
        var feedbackText = new TextBlock
        {
            FontSize = 12, FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 10, 0, 0),
            Visibility = Visibility.Collapsed,
        };

        // Option buttons
        var optionBorders = new List<Border>();
        foreach (Match opt in options)
        {
            var letter = opt.Groups[1].Value.ToUpper();
            var text = opt.Groups[2].Value.Trim();
            var isCorrect = letter == correctLetter;

            var optPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var letterBadge = new Border
            {
                Width = 28, Height = 28,
                CornerRadius = new CornerRadius(14),
                Background = new SolidColorBrush(Color.FromArgb(0x15, 0xFF, 0xFF, 0xFF)),
                Margin = new Thickness(0, 0, 10, 0),
                Child = new TextBlock
                {
                    Text = letter,
                    FontSize = 12, FontWeight = FontWeights.SemiBold,
                    Foreground = (Brush)Application.Current.FindResource("TextFillColorSecondaryBrush"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                }
            };
            optPanel.Children.Add(letterBadge);
            optPanel.Children.Add(new TextBlock
            {
                Text = text,
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Foreground = (Brush)Application.Current.FindResource("TextFillColorPrimaryBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 600,
            });

            var optBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(0x0A, 0xFF, 0xFF, 0xFF)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0x18, 0xFF, 0xFF, 0xFF)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 0, 0, 8),
                Cursor = Cursors.Hand,
                Child = optPanel,
                Tag = letter,
            };

            optionBorders.Add(optBorder);
            panel.Children.Add(optBorder);
        }

        panel.Children.Add(feedbackText);

        bool answered = false;

        // Wire up click handlers
        foreach (var ob in optionBorders)
        {
            var thisLetter = (string)ob.Tag;
            var thisBorder = ob;
            ob.MouseLeftButtonDown += (s, e) =>
            {
                if (answered) return;
                answered = true;
                _answered++;

                bool isCorrect = thisLetter == correctLetter;
                if (isCorrect) _score++;

                // Highlight all options
                foreach (var b in optionBorders)
                {
                    var bLetter = (string)b.Tag;

                    if (bLetter == correctLetter)
                    {
                        // Correct answer — green
                        b.Background = new SolidColorBrush(Color.FromArgb(0x26, 0x10, 0xB9, 0x81));
                        b.BorderBrush = new SolidColorBrush(Color.FromArgb(0x60, 0x34, 0xD3, 0x99));
                    }
                    else if (bLetter == thisLetter && !isCorrect)
                    {
                        // Wrong selection — red
                        b.Background = new SolidColorBrush(Color.FromArgb(0x26, 0xEF, 0x44, 0x44));
                        b.BorderBrush = new SolidColorBrush(Color.FromArgb(0x60, 0xF8, 0x71, 0x71));
                    }
                    else
                    {
                        b.Opacity = 0.5;
                    }
                    b.Cursor = Cursors.Arrow;
                }

                // Show feedback
                feedbackText.Visibility = Visibility.Visible;
                if (isCorrect)
                {
                    feedbackText.Text = "✓ Correct!";
                    feedbackText.Foreground = new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99));
                }
                else
                {
                    feedbackText.Text = $"✗ Wrong — correct answer: {correctLetter}";
                    feedbackText.Foreground = new SolidColorBrush(Color.FromRgb(0xF8, 0x71, 0x71));
                }

                UpdateScore();
            };
        }

        var cardBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0x10, 0xFF, 0xFF, 0xFF)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x18, 0xFF, 0xFF, 0xFF)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 0, 16),
            Child = panel,
        };

        return cardBorder;
    }

    private void UpdateScore()
    {
        if (_totalQuestions > 0)
            ScoreLabel.Text = _answered > 0
                ? $"Score: {_score}/{_answered} ({_totalQuestions} questions)"
                : $"{_totalQuestions} questions";
    }
}
