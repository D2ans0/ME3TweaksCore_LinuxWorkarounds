using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LegendaryExplorerCore.Packages;
using ME3TweaksCore.Localization;
using ME3TweaksCore.NativeMods.Interfaces;

namespace ME3TweaksCore.NativeMods
{
    /// <summary>
    /// ASI mod that is not in the manifest
    /// </summary>
    public class UnknownInstalledASIMod : InstalledASIMod, IUnknownInstalledASIMod
    {
        public UnknownInstalledASIMod(string filepath, string hash, MEGame game) : base(filepath, hash, game)
        {
            UnmappedFilename = Path.GetFileNameWithoutExtension(filepath);
            DllDescription = ReadDllDescription(filepath);
        }
        public string UnmappedFilename { get; set; }
        public string DllDescription { get; set; }

        /// <summary>
        /// Reads dll information for display of this file
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public static string ReadDllDescription(string filepath)
        {
            string retInfo = LC.GetString(LC.string_unknownASIDescription) + "\n";
            var info = FileVersionInfo.GetVersionInfo(filepath);
            if (!string.IsNullOrWhiteSpace(info.ProductName))
            {
                retInfo += '\n' + LC.GetString(LC.string_interp_productNameX, info.ProductName.Trim());
            }

            if (!string.IsNullOrWhiteSpace(info.FileDescription))
            {
                retInfo += '\n' + LC.GetString(LC.string_interp_descriptionX, info.FileDescription.Trim());
            }

            if (!string.IsNullOrWhiteSpace(info.CompanyName))
            {
                retInfo += '\n' + LC.GetString(LC.string_interp_companyX, info.CompanyName.Trim());
            }

            if (!string.IsNullOrWhiteSpace(info.ProductVersion))
            {
                retInfo += '\n' + LC.GetString(LC.string_interp_versionX, info.ProductVersion.Trim());
            }

            return retInfo.Trim();
        }
    }
}
