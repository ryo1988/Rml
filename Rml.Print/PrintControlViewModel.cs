using System;
using System.Linq;
using System.Printing;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SUT.PrintEngine.Extensions;
using SUT.PrintEngine.Paginators;
using SUT.PrintEngine.Views;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Core.Utilities;
using MessageBox = System.Windows.MessageBox;

namespace Rml.Print
{
    public class PrintControlViewModel : SUT.PrintEngine.ViewModels.PrintControlViewModel, IDisposable
    {
        private CompositeDisposable _cd;

        public ReactivePropertySlim<double> BaseScale;

        public PrintControlViewModel(PrintControlView view)
            : base(view)
        {
            _cd = new CompositeDisposable();
            BaseScale = new ReactivePropertySlim<double>(1.0).AddTo(_cd);
            LocalizationToJapanese(view).AddTo(_cd);
        }

        private IDisposable LocalizationToJapanese(PrintControlView view)
        {
            var cd = new CompositeDisposable();
            {
                var actualSizeButton = (Button) view.FindName("ActualSizeButton");
                if (actualSizeButton is null)
                    throw new InvalidOperationException();

                actualSizeButton.Content = "実際のサイズ";
            }

            {
                var allPagesButton = (Button) view.FindName("AllPagesButton");
                if (allPagesButton is null)
                    throw new InvalidOperationException();

                allPagesButton.Content = "全てのページ";
                allPagesButton.ToolTip = "全てのページを表示";
            }

            {
                var menu = VisualTreeHelperEx.FindDescendantByType<Menu>(view);
                if (menu is null)
                    throw new InvalidOperationException();
                var menuItem = menu.Items.OfType<MenuItem>().Single();

                menuItem.Header = "印刷オプション";

                var grid = (Grid) menuItem.Items.OfType<MenuItem>().Single().Header;
                // Panel.ZIndex="1"が後にくるので0
                var border = (Border) VisualTreeHelper.GetChild(grid, 0);
                var paperSizeLabel = GetChild<Label>(border, 0, 0, 0);
                paperSizeLabel.Content = "用紙サイズ";
                var paperSizeChangeButton = GetChild<Button>(border, 0, 0, 1, 1);
                paperSizeChangeButton.Content = "変更";

                var orientationLabel = GetChild<Label>(border, 0, 3, 0);
                orientationLabel.Content = "向き";
                var orientationPortraitRadioButton = GetChild<RadioButton>(border, 0, 3, 1, 0);
                orientationPortraitRadioButton.Content = "縦";
                var orientationLandscapeRadioButton = GetChild<RadioButton>(border, 0, 3, 1, 1);
                orientationLandscapeRadioButton.Content = "横";

                var selectPrinterLabel = GetChild<Label>(border, 0, 6, 0, 0, 0, 0);
                selectPrinterLabel.Content = "選択プリンター";

                var copiesLabel = GetChild<Label>(border, 0, 6, 0, 1, 0, 0);
                copiesLabel.Content = "部数";

                var markPagePositionLabel =
                    (Label) ((CheckBox) view.FindName("PageNumberMarker") ?? throw new InvalidOperationException())
                    .Content;
                if (markPagePositionLabel is null)
                    throw new InvalidOperationException();
                markPagePositionLabel.Content = "ページ位置を印字";

                var cancelSetButton = (Button) view.FindName("CancelSetButton");
                if (cancelSetButton is null)
                    throw new InvalidOperationException();
                cancelSetButton.Content = "キャンセル";

                var setButton = (Button) view.FindName("SetButton");
                if (setButton is null)
                    throw new InvalidOperationException();
                setButton.Content = "適用";
            }

            {
                var menu = (Menu) view.FindName("ScaleMenu");
                if (menu is null)
                    throw new InvalidOperationException();
                var menuItem = menu.Items.OfType<MenuItem>().Single();

                menuItem.Header = "印刷サイズ";

                var border = (Border) menuItem.Items.OfType<MenuItem>().Single().Header;
                var stackPanel = GetChild<StackPanel>(border, 0);

                var fitToPageCheckBox = (CheckBox) view.FindName("cb_FitToPage");
                if (fitToPageCheckBox is null)
                    throw new InvalidOperationException();
                fitToPageCheckBox.Content = "用紙にフィット";

                var okCancelDockPanel = GetChild<DockPanel>(border, 0, 5);
                var cancelButton = GetChild<Button>(border, 0, 5, 0);
                cancelButton.Content = "キャンセル";
                var applyButton = GetChild<Button>(border, 0, 5, 1);
                applyButton.Content = "適用";

                {
                    var fitToPageCheckBoxGrid = GetChild<Grid>(stackPanel, 0);
                    fitToPageCheckBoxGrid.Children.Remove(fitToPageCheckBox);

                    stackPanel.Children.Clear();

                    cancelButton.Visibility = Visibility.Collapsed;

                    var grid = new Grid
                    {
                        ColumnDefinitions =
                        {
                            new ColumnDefinition
                            {
                                Width = GridLength.Auto,
                            },
                            new ColumnDefinition
                            {
                                Width = new GridLength(1.0, GridUnitType.Star),
                            },
                            new ColumnDefinition
                            {
                                Width = GridLength.Auto,
                            },
                            new ColumnDefinition
                            {
                                Width = new GridLength(1.0, GridUnitType.Star),
                            },
                        },
                        RowDefinitions =
                        {
                            new RowDefinition(),
                            new RowDefinition(),
                            new RowDefinition(),
                        },
                    };
                    stackPanel.Children.Add(grid);

                    Grid.SetRow(fitToPageCheckBox, 0);
                    Grid.SetColumn(fitToPageCheckBox, 0);
                    Grid.SetColumnSpan(fitToPageCheckBox, 4);
                    grid.Children.Add(fitToPageCheckBox);
                    fitToPageCheckBox.HorizontalAlignment = HorizontalAlignment.Left;
                    fitToPageCheckBox.HorizontalContentAlignment = HorizontalAlignment.Left;

                    var titleTextBlock = new TextBlock
                    {
                        Text = "尺度"
                    };
                    Grid.SetRow(titleTextBlock, 1);
                    Grid.SetColumn(titleTextBlock, 0);
                    grid.Children.Add(titleTextBlock);

                    var numeratorIntegerUpDown = new IntegerUpDown
                    {
                        Minimum = 1,
                        Value = 1,
                    };
                    Grid.SetRow(numeratorIntegerUpDown, 1);
                    Grid.SetColumn(numeratorIntegerUpDown, 1);
                    grid.Children.Add(numeratorIntegerUpDown);
                    Observable
                        .FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(
                            h => numeratorIntegerUpDown.MouseUp += h,
                            h => numeratorIntegerUpDown.MouseUp -= h)
                        .Subscribe(o => o.EventArgs.Handled = true)
                        .AddTo(cd);

                    var slashTextBlock = new TextBlock
                    {
                        Text = "/"
                    };
                    Grid.SetRow(slashTextBlock, 1);
                    Grid.SetColumn(slashTextBlock, 2);
                    grid.Children.Add(slashTextBlock);

                    var denominatorIntegerUpDown = new IntegerUpDown
                    {
                        Minimum = 1,
                        Value = 100,
                    };
                    Grid.SetRow(denominatorIntegerUpDown, 1);
                    Grid.SetColumn(denominatorIntegerUpDown, 3);
                    grid.Children.Add(denominatorIntegerUpDown);
                    Observable
                        .FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(
                            h => denominatorIntegerUpDown.MouseUp += h,
                            h => denominatorIntegerUpDown.MouseUp -= h)
                        .Subscribe(o => o.EventArgs.Handled = true)
                        .AddTo(cd);

                    stackPanel.Children.Add(okCancelDockPanel);

                    var numeratorIntegerUpDownValueChanged = Observable
                        .FromEventPattern<RoutedPropertyChangedEventHandler<object>, object>(
                            h => numeratorIntegerUpDown.ValueChanged += h,
                            h => numeratorIntegerUpDown.ValueChanged -= h)
                        .ToUnit();
                    var denominatorIntegerUpDownValueChanged = Observable
                        .FromEventPattern<RoutedPropertyChangedEventHandler<object>, object>(
                            h => denominatorIntegerUpDown.ValueChanged += h,
                            h => denominatorIntegerUpDown.ValueChanged -= h)
                        .ToUnit();
                    var fitToPageCheckBoxUnchecked = Observable
                        .FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                            h => fitToPageCheckBox.Unchecked += h,
                            h => fitToPageCheckBox.Unchecked -= h)
                        .ToUnit();

                    Observable
                        .Merge(numeratorIntegerUpDownValueChanged,
                            denominatorIntegerUpDownValueChanged,
                            fitToPageCheckBoxUnchecked,
                            BaseScale.ToUnit())
                        .Subscribe(_ =>
                        {
                            if ((numeratorIntegerUpDown.Value ?? 0) <= 0 ||
                                (denominatorIntegerUpDown.Value ?? 0) <= 0)
                                return;

                            const double coefficient = /*dpi*/96.0 / /*inch to mm*/25.4;
                            var numerator = (double)numeratorIntegerUpDown.Value.Value;
                            var denominator = (double)denominatorIntegerUpDown.Value.Value;
                            var scale = numerator / denominator * coefficient * BaseScale.Value;
                            SetCurrentValue(ScaleProperty, scale);
                        })
                        .AddTo(cd);

                    var fitToPageCheckBoxChecked = Observable
                        .FromEventPattern<RoutedEventHandler, RoutedEventArgs>(
                            h => fitToPageCheckBox.Checked += h,
                            h => fitToPageCheckBox.Checked -= h)
                        .ToUnit();

                    Observable
                        .Merge(fitToPageCheckBoxChecked, fitToPageCheckBoxUnchecked)
                        .Subscribe(_ =>
                        {
                            if (fitToPageCheckBox.IsChecked.HasValue && fitToPageCheckBox.IsChecked.Value)
                            {
                                numeratorIntegerUpDown.IsEnabled = false;
                                slashTextBlock.IsEnabled = false;
                                denominatorIntegerUpDown.IsEnabled = false;
                            }
                            else
                            {
                                numeratorIntegerUpDown.IsEnabled = true;
                                slashTextBlock.IsEnabled = true;
                                denominatorIntegerUpDown.IsEnabled = true;
                            }
                        })
                        .AddTo(cd);
                }
            }

            {
                var grid = (Grid) view.FindName("ButtonPane");
                if (grid is null)
                    throw new InvalidOperationException();
                var stackPanel = grid.Children.OfType<StackPanel>().Last();
                var buttons = Enumerable
                    .Range(0, VisualTreeHelper.GetChildrenCount(stackPanel))
                    .Select(o => VisualTreeHelper.GetChild(stackPanel, o))
                    .OfType<Button>()
                    .ToArray();
                var cancelButton = buttons
                    .Single(o => o.Content is "Cancel");
                var printButton = buttons
                    .Single(o => o.Content is "Print");
                cancelButton.Content = "キャンセル";
                printButton.Content = "印刷";
            }

            static T GetChild<T>(DependencyObject dependencyObject, params int[] childIndices)
                where T : DependencyObject
            {
                foreach (var childIndex in childIndices)
                {
                    dependencyObject = VisualTreeHelper.GetChild(dependencyObject, childIndex);
                }

                return (T) dependencyObject;
            }

            return cd;
        }

        public PrintControlViewModel(Visual visual, Rect viewBox, Rect viewport)
            : this(new PrintControlView())
        {
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                var visualBrush = new VisualBrush(visual)
                {
                    Stretch = Stretch.None,
                    Viewbox = viewBox,
                };
                drawingContext.DrawRectangle(visualBrush, null, viewport);
                drawingContext.PushOpacityMask(Brushes.White);
            }

            DrawingVisual = drawingVisual;
        }

        public override void ExecutePrint(object parameter)
        {
            try
            {
                // 何故かうまくプリントできないので直接プリント
                ShowProgressDialog();
                ((VisualPaginator) Paginator).PageCreated += PrintControlPresenterPageCreated;
                var writer = PrintQueue.CreateXpsDocumentWriter(CurrentPrinter);
                writer.Write(Paginator, CurrentPrinter.UserPrintTicket);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            finally
            {
                ProgressDialog.Hide();
            }
        }

        private void PrintControlPresenterPageCreated(object sender, PageEventArgs e)
        {
            ProgressDialog.CurrentProgressValue = e.PageNumber;
            ProgressDialog.Message = GetStatusMessage();
            Application.Current.DoEvents();
        }

        public override void InitializeProperties()
        {
            base.InitializeProperties();

            FullScreenPrintWindow.Title = "印刷プレビュー";
        }

        // 元の実装だと謎のマージンを入れられたので、マージンなしに
        protected override void CreatePaginator(DrawingVisual visual, Size printSize)
        {
            this.Paginator = new VisualPaginator(visual, printSize, new Thickness(0), new Thickness(0));
        }

        public void Dispose()
        {
            _cd?.Dispose();
            _cd = null;
        }
    }
}