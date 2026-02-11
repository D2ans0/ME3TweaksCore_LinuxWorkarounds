using System.Collections.Generic;
using System.Text;
using LegendaryExplorerCore.GameFilesystem;
using LegendaryExplorerCore.Packages;
using ME3TweaksCore.Targets;

namespace ME3TweaksCore.ME3Tweaks.StarterKit
{
    public class StarterKitOptions
    {
        public string ModDescription;
        public string ModDeveloper;
        public string ModName;
        public string ModInternalName;
        public string ModDLCFolderNameSuffix;
        public int ModMountPriority;
        public int ModInternalTLKID;
        public string ModURL;
        public MountFlag ModMountFlag;
        public MEGame ModGame;

        public int ModModuleNumber;

        /// <summary>
        /// If a Mod object should be generated via moddesc.ini. Defaults to true
        /// </summary>
        public bool GenerateModdesc { get; set; } = true;
        /// <summary>
        /// Directory to place the DLC folder at. Set to null to use the mod library
        /// </summary>
        public string OutputFolderOverride { get; set; }


        #region FEATURE OPTIONS
        /// <summary>
        /// If a startup file should be added after the mod has been generated
        /// </summary>
        public bool AddStartupFile { get; set; }
        public bool AddPlotManagerData { get; set; }
        public bool AddModSettingsMenu { get; set; }
        public List<Bio2DAOption> Blank2DAsToGenerate { get; set; } = new();

        // Shared LE2/3
        public bool AddGarrusSQM { get; set; }
        public bool AddTaliSQM { get; set; }

        // LE2 specific
        public bool AddMirandaSQM { get; set; }
        public bool AddJacobSQM { get; set; }
        public bool AddMordinSQM { get; set; }
        public bool AddJackSQM { get; set; }
        public bool AddGruntSQM { get; set; }
        public bool AddSamaraSQM { get; set; }
        public bool AddThaneSQM { get; set; }
        public bool AddLegionSQM { get; set; }
        public bool AddKasumiSQM { get; set; }
        public bool AddZaeedSQM { get; set; }


        // ME3/LE3 specific
        public bool AddAshleySQM { get; set; }
        public bool AddLiaraSQM { get; set; }
        public bool AddJamesSQM { get; set; }
        public bool AddEDISQM { get; set; }
        public bool AddKaidanSQM { get; set; }
        public bool AddJavikSQM { get; set; }

        /// <summary>
        /// Game target for the game this is being generated for
        /// </summary>
        public GameTarget Target { get; set; }

        #endregion


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"Game: " + ModGame);
            sb.AppendLine(@"ModName: " + ModName);
            sb.AppendLine(@"ModDescription: " + ModDescription);
            sb.AppendLine(@"ModDeveloper: " + ModDLCFolderNameSuffix);
            sb.AppendLine(@"ModDLCFolderName: " + ModDLCFolderNameSuffix);
            sb.AppendLine(@"ModInternalTLKID: " + ModInternalTLKID);
            sb.AppendLine(@"ModMountPriority: " + ModMountPriority);
            sb.AppendLine(@"ModModuleNumber: " + ModModuleNumber);
            sb.AppendLine(@"ModURL: " + ModURL);
            sb.AppendLine(@"Mount flag: " + ModMountFlag);
            return sb.ToString();
        }
    }
}
