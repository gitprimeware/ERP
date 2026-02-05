using ERP.DAL;
using ERP.DAL.Configuration;
using ERP.UI.Forms;
using ERP.UI.Services;
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
            
            // Database initialization
            try
            {
                DatabaseInitializer.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Database initialization error: {ex.Message}\n\n" +
                    "The application will continue but database operations may not work.\n\n" +
                    "Please check your database configuration.",
                    "Warning", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
            }
            
            // Show login screen
            using (var loginForm = new LoginForm())
            {
                if (loginForm.ShowDialog() == DialogResult.OK && loginForm.LoggedInUser != null)
                {
                    // Start user session
                    UserSessionService.CurrentUser = loginForm.LoggedInUser;
                    
                    // Show main form
                    Application.Run(new MainForm());
                }
            }
        }
    }
}

