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
            staticMonitor?.Log("Hi, this is the PollEvent wrapper");
            object[] parameters = new object[1];
            // originalPollEvent(out )
            object? retVal = originalPollEvent!.DynamicInvoke(parameters);
            ev = (Sdl.Event)parameters[0];
            return (int)(retVal ?? 0);
        }

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            staticMonitor = this.Monitor;

            Harmony harmony = new(ModManifest.UniqueID);

            // Sdl.PollEvent is a delegate so we have to wrap it.
            // Delegate originalPollEvent = (Delegate)(AccessTools.Field("Sdl:PollEvent").GetValue(null) ?? throw new Exception("Sdl:PollEvent not found"));
            originalPollEvent = (Sdl.d_sdl_pollevent)(AccessTools.Field("Sdl:PollEvent").GetValue(null) ?? throw new Exception("Sdl:PollEvent not found"));

            staticMonitor?.Log($"originalPollEvent = '{originalPollEvent}'");

/*
            Delegate newPollEvent = int (out Sdl.Event ev) =>
            {
                staticMonitor?.Log("Hi, this is the PollEvent wrapper");
                object[] parameters = new object[1];
                object? retVal = originalPollEvent.DynamicInvoke(parameters);
                ev = (Sdl.Event)parameters[0];
                return (int)(retVal ?? 0);
            };
            */

            // Sdl.PollEvent = (Sdl.d_sdl_pollevent)newPollEvent;

            Sdl.PollEvent = PollEventWrapper;

            /*
                        Type sdlDelegateType1 = AccessTools.TypeByName("Sdl:d_sdl_pollevent");
                        staticMonitor?.Log($"sdlDelegateType1 = {sdlDelegateType1}");

                        Type sdlDelegateType2 = AccessTools.TypeByName("Sdl.d_sdl_pollevent");
                        staticMonitor?.Log($"sdlDelegateType2 = {sdlDelegateType2}");

                        Type sdlDelegateType3 = AccessTools.TypeByName("Sdl+d_sdl_pollevent");
                        staticMonitor?.Log($"sdlDelegateType3 = {sdlDelegateType3}");

                        // Delegate convertedNewPollEvent = newPollEvent.GetMethodInfo().CreateDelegate(sdlDelegateType3, newPollEvent);
                        MethodInfo wrapperMethodInfo = AccessTools.Method("FastClickFix.ModEntry:PollEventWrapper");
                        staticMonitor?.Log($"wrapperMethodInfo = {wrapperMethodInfo}");
                        Delegate convertedNewPollEvent = wrapperMethodInfo.CreateDelegate(sdlDelegateType3);

                        AccessTools.Field("Sdl:PollEvent").SetValue(null, convertedNewPollEvent);
            */
            // Event Sdl.Event _outEvent;
            // originalPollEvent.DynamicInvoke()

            // harmony.Patch(
            //     original: AccessTools.Method("Sdl:PollEvent"),
            //     postfix: new HarmonyMethod(typeof(ModEntry), nameof(SdlPollEvent_PostFix))
            // );

            staticMonitor?.Log($"FastClickFix loaded without exceptions");

        }

        // public static bool SdlPollEvent_PostFix(out Event _event)
        // {
        //     return true
        // }
    }
}
