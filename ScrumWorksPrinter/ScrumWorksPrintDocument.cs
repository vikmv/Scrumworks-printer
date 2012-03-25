using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using ScrumWorksPrinter.Properties;

namespace ScrumWorksPrinter
{
    public class ScrumWorksPrintDocument : PrintDocument
    {
        public class PrintableItem
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public int Estimate { get; set; }
            public Color Color { get; set; }
        }

        public class BackLogItem : PrintableItem
        {

        }

        public class Task : PrintableItem
        {
        }

        private readonly IEnumerable<PrintableItem> _tasksEnumerable;
        private IEnumerator<PrintableItem> _tasks;

        public ScrumWorksPrintDocument(IEnumerable<PrintableItem> tasks)
        {
            _tasksEnumerable = tasks;
            DefaultPageSettings.Margins = new Margins(40, 40, 40, 40);

            foreach (var ps in PrinterSettings.PaperSizes.Cast<PaperSize>())
                Console.WriteLine(ps.Kind);

            DefaultPageSettings.PaperSize = PrinterSettings.PaperSizes.Cast<PaperSize>().First(p => p.Kind == PaperKind.A4);
            BeginPrint += OnBeginPrint;
            PrintPage += OnPrintPage;
        }

        private void OnBeginPrint(object sender, PrintEventArgs e)
        {
            _tasks = _tasksEnumerable.GetEnumerator();
            if (!_tasks.MoveNext())
                e.Cancel = true;
        }

        private void OnPrintPage(object sender, PrintPageEventArgs e)
        {
            const int columns = 3;
            const int rows = 5;

            int x = e.MarginBounds.Left;
            int y = e.MarginBounds.Top;

            int width = e.MarginBounds.Width / columns;
            int height = e.MarginBounds.Height / rows;

            for (int j = 0; j < rows; j++)
                for (int i = 0; i < columns; i++)
                {
                    int nx = x + i * width;
                    int ny = y + j * height;
                    PrintTask(e.Graphics, new Rectangle(nx, ny, width, height), _tasks.Current);

                    if (!_tasks.MoveNext())
                    {
                        e.HasMorePages = false;
                        return;
                    }
                }

            e.HasMorePages = true;
        }

        private static void PrintTask(Graphics graphics, Rectangle rect, PrintableItem task)
        {
            var brush = new SolidBrush(Color.Black);
            var pen = new Pen(brush);

            //HatchBrush(HatchStyle.DiagonalCross, task.Color, Color.White)
            graphics.FillRectangle(new SolidBrush(task.Color), rect);
            graphics.DrawRectangle(pen, rect);

            rect.Inflate(-5, -5);

            var titleFont = new Font("Arial", 10, FontStyle.Bold);
            var titleAlignment = new StringFormat(StringFormat.GenericDefault)
                                     {
                                         Alignment = StringAlignment.Center,
                                         LineAlignment = StringAlignment.Near,
                                     };

            var titleHeight = (int)graphics.MeasureString(task.Title, titleFont, rect.Width).Height + 5;
            var descFont = new Font("Arial", 7, FontStyle.Regular);
            var descRect = new Rectangle(rect.X, rect.Y + titleHeight, rect.Width, rect.Height - titleHeight);
            var descAlignment = new StringFormat(StringFormat.GenericDefault)
                                    {
                                        Alignment = StringAlignment.Near,
                                        LineAlignment = StringAlignment.Near,
                                    };

            var estimateFont = new Font("Arial", 12, FontStyle.Bold);
            var estimateAlignment = new StringFormat(StringFormat.GenericDefault)
                                        {
                                            Alignment = StringAlignment.Far,
                                            LineAlignment = StringAlignment.Far,
                                        };

            graphics.DrawString(task.Title, titleFont, brush, rect, titleAlignment);
            graphics.DrawString(task.Description, descFont, brush, descRect, descAlignment);

            if (task is BackLogItem)
            {
                var imgHeight = 100;
                var imgWidth = (int)(Resources.tux.Width * ((float)imgHeight / Resources.tux.Height));
                var nrect = new Rectangle(rect.Right - imgWidth, rect.Bottom - imgHeight, imgWidth, imgHeight);

                graphics.DrawImage(Resources.tux, nrect);

                nrect.Offset(0, 57);
                graphics.DrawString(task.Estimate.ToString(), estimateFont, brush, nrect, titleAlignment);
            }
            else
            {
                graphics.DrawString(task.Estimate.ToString(), estimateFont, brush, rect, estimateAlignment);
            }
        }

    }
}