using System;
using System.Printing;
using System.Windows;
using System.Windows.Media;
using SUT.PrintEngine.Extensions;
using SUT.PrintEngine.Paginators;
using SUT.PrintEngine.Views;

namespace Rml.Print
{
    public class PrintControlViewModel : SUT.PrintEngine.ViewModels.PrintControlViewModel
    {
        public PrintControlViewModel(PrintControlView view)
            : base(view)
        {
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
    }
}