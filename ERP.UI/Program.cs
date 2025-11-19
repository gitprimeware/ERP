using ERP.DAL;
using ERP.UI.Forms;
using System;
using System.Windows.Forms;

namespace ERP.UI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Veritabanını başlat
            try
            {
                DatabaseInitializer.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veritabanı başlatma hatası: {ex.Message}\n\nUygulama devam edecek ancak veritabanı işlemleri çalışmayabilir.", 
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
            Application.Run(new MainForm());
        }
    }
}

