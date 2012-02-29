using System;
using System.Net;
using System.Windows.Forms;

namespace ScrumWorksPrinter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}


namespace ScrumWorksPrinter.ScrumWorksService
{
    public partial class ScrumWorksService
    {
        private readonly Version _protocolVersion = HttpVersion.Version10;

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest request = base.GetWebRequest(uri);
            ((HttpWebRequest)request).ProtocolVersion = _protocolVersion;
            return request;
        }
    }

    public partial class ProductWSO
    {
        public string displayName
        {
            get { return name; }
        }
    }

    public partial class SprintWSO
    {
        public string displayName
        {
            get { return string.Format("{0} ({1:yyyy-MM-dd}-{2:yyyy-MM-dd})", name, startDate, endDate); }
        }
    }

}
