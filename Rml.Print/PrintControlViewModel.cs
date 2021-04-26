using System;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SUT.PrintEngine.Extensions;
using SUT.PrintEngine.Paginators;
using SUT.PrintEngine.Views;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Rml.Print
{
    public class PrintControlViewModel : SUT.PrintEngine.ViewModels.PrintControlViewModel
    {
        public PrintControlViewModel(PrintControlView view)
            : base(view)
        {
            LocalizationToJapanese(view);
        }

        private static void LocalizationToJapanese(PrintControlView view)
        {
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

                var fitToPageCheckBox = (CheckBox) view.FindName("cb_FitToPage");
                if (fitToPageCheckBox is null)
                    throw new InvalidOperationException();
                fitToPageCheckBox.Content = "ページに合わせる";

                var border = (Border) menuItem.Items.OfType<MenuItem>().Single().Header;
                var smallLabel = GetChild<Label>(border, 0, 1, 0, 0, 0);
                smallLabel.Content = "小さい";
                var lessPagesLabel = GetChild<Label>(border, 0, 1, 0, 0, 1);
                lessPagesLabel.Content = "少ないページ数";
                var bigLabel = GetChild<Label>(border, 0, 1, 0, 1, 0);
                bigLabel.Content = "大きい";
                var morePagesLabel = GetChild<Label>(border, 0, 1, 0, 1, 1);
                morePagesLabel.Content = "多いページ数";

                var cancelButton = GetChild<Button>(border, 0, 5, 0);
                cancelButton.Content = "キャンセル";
                var applyButton = GetChild<Button>(border, 0, 5, 1);
                applyButton.Content = "適用";
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
    }
}