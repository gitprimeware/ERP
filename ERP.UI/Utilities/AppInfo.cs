using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ERP.UI.Utilities
{
    internal class AppInfo
    {
        /// <summary>
        /// Program version
        /// </summary>
        public static string Version => Application.ProductVersion;

        /// <summary>
        /// Program title
        /// </summary>
        public static string Title => Application.ProductName;

        /// <summary>
        /// Program full title with emoji
        /// </summary>
        public static string FullTitle => $"📊 {Title}";

        /// <summary>
        /// Program description
        /// </summary>
        public static string Description => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        /// <summary>
        /// Company name
        /// </summary>
        public static string Company => Application.CompanyName;

        /// <summary>
        /// Copyright information
        /// </summary>
        public static string Copyright => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

        /// <summary>
        /// Gets version string with "Versiyon" prefix
        /// </summary>
        public static string VersionString => $"Versiyon {Version}";
    }
}
