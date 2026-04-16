using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using AcademicAI.App.ViewModels;

namespace AcademicAI.App.Views;

public partial class FlashcardsView : Page
{
    private readonly FlashcardsViewModel _vm;

    public FlashcardsView()
    {
        InitializeComponent();
        _vm = new FlashcardsViewModel();
        DataContext = _vm;
        _vm.PropertyChanged += Vm_PropertyChanged;
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FlashcardsViewModel.ResultText))
        {
            RenderCards(_vm.ResultText);
        }
    }

    private void RenderCards(string rawText)
    {
        FlashcardsList.Items.Clear();
        if (string.IsNullOrWhiteSpace(rawText)) return;

        // Parse cards: split by --- or **Card N**
        var cards = Regex.Split(rawText, @"(?:^|\n)---\s*\n?|(?:^|\n)\*\*Card\s+\d+\*\*\s*\n?",
            RegexOptions.IgnoreCase);

        int cardNum = 0;
        foreach (var cardBlock in cards)
        {
            var trimmed = cardBlock.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            cardNum++;

            // Extract Front/Back/Hint
            var frontMatch = Regex.Match(trimmed, @"(?:Front|Question|Q)[:\s]*(.+?)(?=\n|$)", RegexOptions.IgnoreCase);
            var backMatch = Regex.Match(trimmed, @"(?:Back|Answer|A)[:\s]*(.+?)(?=\nH|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var hintMatch = Regex.Match(trimmed, @"(?:Hint|H)[:\s]*(.+?)$", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            var front = frontMatch.Success ? frontMatch.Groups[1].Value.Trim() : trimmed;
            var back = backMatch.Success ? backMatch.Groups[1].Value.Trim() : "";
            var hint = hintMatch.Success ? hintMatch.Groups[1].Value.Trim() : "";

            if (front.Length < 3) continue;

            var card = CreateFlipCard(cardNum, front, back, hint);
            FlashcardsList.Items.Add(card);
        }

        CardCountLabel.Text = $"{cardNum} cards generated";
    }

    private Border CreateFlipCard(int num, string front, string back, string hint)
    {
        // Front content
        var frontPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(20),
        };
        frontPanel.Children.Add(new TextBlock
        {
            Text = $"Card {num}",
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x81, 0x8C, 0xF8)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8),
        });
        frontPanel.Children.Add(new TextBlock
        {
            Text = front,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            Foreground = (Brush)Application.Current.FindResource("TextFillColorPrimaryBrush"),
        });
        if (!string.IsNullOrEmpty(hint))
        {
            frontPanel.Children.Add(new TextBlock
            {
                Text = $"💡 {hint}",
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Foreground = (Brush)Application.Current.FindResource("TextFillColorTertiaryBrush"),
                Margin = new Thickness(0, 10, 0, 0),
            });
        }
        frontPanel.Children.Add(new TextBlock
        {
            Text = "Click to reveal answer",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0x99, 0x99, 0x99)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 12, 0, 0),
        });

        // Back content
        var backPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(20),
            Visibility = Visibility.Collapsed,
        };
        backPanel.Children.Add(new TextBlock
        {
            Text = "Answer",
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8),
        });
        backPanel.Children.Add(new TextBlock
        {
            Text = back,
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            Foreground = (Brush)Application.Current.FindResource("TextFillColorPrimaryBrush"),
        });
        backPanel.Children.Add(new TextBlock
        {
            Text = "Click to show question",
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromArgb(0x80, 0x99, 0x99, 0x99)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 12, 0, 0),
        });

        // Card container
        var cardGrid = new Grid();
        cardGrid.Children.Add(frontPanel);
        cardGrid.Children.Add(backPanel);

        var cardBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0x18, 0xFF, 0xFF, 0xFF)),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x20, 0xFF, 0xFF, 0xFF)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            MinHeight = 140,
            Padding = new Thickness(16),
            Margin = new Thickness(0, 0, 0, 12),
            Cursor = Cursors.Hand,
            Child = cardGrid,
            RenderTransformOrigin = new Point(0.5, 0.5),
            RenderTransform = new ScaleTransform(1, 1),
        };

        bool isFlipped = false;

        cardBorder.MouseLeftButtonDown += (s, e) =>
        {
            isFlipped = !isFlipped;

            // Animate flip
            var scaleX = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
            scaleX.Completed += (_, _) =>
            {
                frontPanel.Visibility = isFlipped ? Visibility.Collapsed : Visibility.Visible;
                backPanel.Visibility = isFlipped ? Visibility.Visible : Visibility.Collapsed;
                cardBorder.Background = isFlipped
                    ? new SolidColorBrush(Color.FromArgb(0x12, 0x34, 0xD3, 0x99))
                    : new SolidColorBrush(Color.FromArgb(0x18, 0xFF, 0xFF, 0xFF));

                var scaleBack = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
                scaleBack.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                ((ScaleTransform)cardBorder.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleBack);
            };
            scaleX.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn };
            ((ScaleTransform)cardBorder.RenderTransform).BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
        };

        return cardBorder;
    }
}
