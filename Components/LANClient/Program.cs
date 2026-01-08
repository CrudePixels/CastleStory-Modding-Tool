using System;
using System.Windows.Forms;

namespace CastleStoryLANClient
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LANClientGUI());
        }
    }
}
