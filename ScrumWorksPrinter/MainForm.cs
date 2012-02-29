using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using ScrumWorksPrinter.ScrumWorksService;

namespace ScrumWorksPrinter
{
    public partial class MainForm : Form
    {
        private readonly ScrumWorksService.ScrumWorksService _service = InitService();

        private readonly List<Color> _colors = new List<Color>
        {
            Color.LightPink, Color.LightYellow, Color.BlanchedAlmond, Color.LightGreen, Color.LightCyan, Color.BurlyWood, Color.Snow,
            Color.LightPink, Color.LightYellow, Color.BlanchedAlmond, Color.LightGreen, Color.LightCyan, Color.BurlyWood, Color.Snow,
            Color.LightPink, Color.LightYellow, Color.BlanchedAlmond, Color.LightGreen, Color.LightCyan, Color.BurlyWood, Color.Snow,
        };

        private static ScrumWorksService.ScrumWorksService InitService()
        {
            return new ScrumWorksService.ScrumWorksService
            {

                Credentials = new NetworkCredential(ConfigurationManager.AppSettings["username"], ConfigurationManager.AppSettings["password"]),
                PreAuthenticate = true
            };
        }

        public MainForm()
        {
            InitializeComponent();

            var products = _service.getProducts(new getProducts());
            lbProduct.DisplayMember = "displayName";
            lbProduct.DataSource = products;
        }

        public IEnumerable<ScrumWorksPrintDocument.PrintableItem> GetTasks(SprintWSO sprint)
        {
            var colorEnumerator = _colors.GetEnumerator();

            foreach (var x in _service.getActiveBacklogItemsForSprint(new getActiveBacklogItemsForSprint { SprintWSO_1 = sprint }))
            {
                if (!colorEnumerator.MoveNext())
                {
                    colorEnumerator = _colors.GetEnumerator();
                    colorEnumerator.MoveNext();
                }

                var backlogItem = new ScrumWorksPrintDocument.BackLogItem
                {
                    Title = x.title,
                    Estimate = x.estimate ?? 0,
                    Description = x.description,
                    Color = colorEnumerator.Current,
                };
                yield return backlogItem;

                foreach (var t in _service.getTasks(new getTasks { BacklogItemWSO_1 = x }))
                {
                    var task = new ScrumWorksPrintDocument.Task
                                   {
                                       Title = t.title,
                                       Estimate = t.estimatedHours,
                                       Description = t.description,
                                       Color = colorEnumerator.Current
                                   };

                    yield return task;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var tasks = GetTasks(lbSprint.SelectedItem as SprintWSO);
            var printDocument = new ScrumWorksPrintDocument(tasks);

            var pp = new PrintPreviewDialog { Document = printDocument };
            pp.ShowDialog();
        }

        private void lbProduct_SelectedValueChanged(object sender, EventArgs e)
        {
            var product = lbProduct.SelectedItem as ProductWSO;
            if (product == null)
                return;

            lbSprint.DisplayMember = "displayName";
            lbSprint.DataSource = _service.getSprints(new getSprints { ProductWSO_1 = product }).OrderByDescending(s => s.startDate).ToList();
        }

        private void lbSprint_SelectedValueChanged(object sender, EventArgs e)
        {
            var sprint = lbSprint.SelectedItem as SprintWSO;
            if (sprint == null)
                return;

            lbItems.DisplayMember = "title";
            lbItems.DataSource = _service.getActiveBacklogItemsForSprint(new getActiveBacklogItemsForSprint { SprintWSO_1 = sprint });
        }

        private void lbItems_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            var bounds = e.Bounds;
            bounds.Height -= 10;
            bounds.Y += 5;
            var idx = e.Index % _colors.Count;
            using (var brush = new SolidBrush(_colors[idx]))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }
            var item = (BacklogItemWSO)lbItems.Items[e.Index];
            e.Graphics.DrawString(item.title, Font, SystemBrushes.ControlText, bounds);
        }

        private void lbItems_DoubleClick(object sender, EventArgs e)
        {
            if (lbItems.SelectedItem == null)
                return;
            if (colorDialog1.ShowDialog() != DialogResult.OK)
                return;

            var idx = lbItems.SelectedIndex % _colors.Count;
            _colors[idx] = colorDialog1.Color;
            lbItems.Refresh();
        }
    }
}
