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
            // staticMonitor?.Log("Hi, this is the PollEvent wrapper #2");
            int retVal = originalPollEvent!(out ev);

            if(ev.Type == (Sdl.EventType)1025u)
            {
                staticMonitor?.Log("Intercepted a MouseButtonDown event!");
            }

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
            originalPollEvent = Sdl.PollEvent;
            staticMonitor?.Log($"originalPollEvent = '{originalPollEvent}'");
            Sdl.PollEvent = PollEventWrapper;

            staticMonitor?.Log($"FastClickFix loaded without exceptions");
        }
    }
}
