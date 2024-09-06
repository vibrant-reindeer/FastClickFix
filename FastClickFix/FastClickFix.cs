using StardewModdingAPI;
using StardewValley;
using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using System.Runtime.InteropServices;
using System.Reflection;

namespace FastClickFix
{
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        private static IMonitor? staticMonitor;
        private static Sdl.d_sdl_pollevent? originalPollEvent;

        public static int PollEventWrapper(out Sdl.Event ev)
        {
            staticMonitor?.Log("Hi, this is the PollEvent wrapper #2");
            int retVal = originalPollEvent!(out ev);
            return retVal;
        }

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            staticMonitor = this.Monitor;

            // Harmony harmony = new(ModManifest.UniqueID);

            // Sdl.PollEvent is a delegate so we have to wrap it.
            originalPollEvent = (Sdl.d_sdl_pollevent)(AccessTools.Field("Sdl:PollEvent").GetValue(null) ?? throw new Exception("Sdl:PollEvent not found"));
            staticMonitor?.Log($"originalPollEvent = '{originalPollEvent}'");
            Sdl.PollEvent = PollEventWrapper;

            staticMonitor?.Log($"FastClickFix loaded without exceptions");
        }
    }
}
