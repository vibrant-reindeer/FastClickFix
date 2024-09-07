using StardewModdingAPI;
using StardewValley;
using HarmonyLib;
using Microsoft.Xna.Framework.Input;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FastClickFix
{
    internal sealed class ModEntry : Mod
    {
        private static IMonitor? staticMonitor;
        private static Sdl.d_sdl_pollevent? originalSdlPollEvent;

        public override void Entry(IModHelper helper)
        {
            staticMonitor = this.Monitor;

            // Sdl.PollEvent is a delegate so we have to wrap it.
            originalSdlPollEvent = Sdl.PollEvent;
            staticMonitor?.Log($"originalSdlPollEvent = '{originalSdlPollEvent}'");
            Sdl.PollEvent = PollEventWrapper;

            Harmony harmony = new(ModManifest.UniqueID);
            harmony.Patch(
              original: AccessTools.Method(typeof(Game1), "Update"),
              prefix: new HarmonyMethod(typeof(ModEntry), nameof(Update_Prefix))
            );

            harmony.Patch(
              original: AccessTools.Method(typeof(Game1), "Draw"),
              postfix: new HarmonyMethod(typeof(ModEntry), nameof(Draw_Postfix))
            );

            harmony.Patch(
              original: AccessTools.Method(typeof(InputState), "GetMouseState"),
              postfix: new HarmonyMethod(typeof(ModEntry), nameof(GetMouseState_Postfix))
            );


            staticMonitor?.Log($"FastClickFix loaded without exceptions");
        }

        private const uint SdlMouseButtonDownEventType = 1025u;
        // Tap directly into SDL mouse button events.
        public static int PollEventWrapper(out Sdl.Event ev)
        {
            int retVal = originalSdlPollEvent!(out ev);
            if (ev.Type == (Sdl.EventType)SdlMouseButtonDownEventType)
            {
                staticMonitor?.Log("Intercepted a MouseButtonDown event!");
                var eventConverter = new EventConverter { MonoGameSdlEvent = ev };
                OnMouseDown(eventConverter.SdlMouseButtonEvent.Button);
            }
            return retVal;
        }

        // The start of Game1.Update marks the start of a new frame.
        public static void Update_Prefix()
        {
            try
            {
                // staticMonitor?.Log($"{nameof(Update_Prefix)}");
                OnFrameStart();
            }
            catch (Exception e)
            {
                staticMonitor?.Log($"Failed in {nameof(Update_Prefix)}:\n{e}", LogLevel.Error);
            }
        }

        // The end of Game1.Draw marks the end of a frame.
        public static void Draw_Postfix()
        {
            try
            {
                // staticMonitor?.Log($"{nameof(Draw_Postfix)}");
                OnFrameEnd();
            }
            catch (Exception e)
            {
                staticMonitor?.Log($"Failed in {nameof(Draw_Postfix)}:\n{e}", LogLevel.Error);
            }
        }

        // Override mouse button state to "down" if matching event was detected.
        public static void GetMouseState_Postfix(ref MouseState __result)
        {
            try
            {
                bool eventLeftDown = wasDownBetweenFrames[BUTTON_LEFT];
                bool eventRightDown = wasDownBetweenFrames[BUTTON_RIGHT];
                if (eventLeftDown || eventRightDown)
                {
                    var polled = __result;
                    var combinedLeft = eventLeftDown ? ButtonState.Pressed : polled.LeftButton;
                    var combinedRight = eventRightDown ? ButtonState.Pressed : polled.RightButton;
                    DebugPrintButtonOverrides(combinedLeft, combinedRight, polled.LeftButton, polled.RightButton);
                    var newResult = new MouseState(polled.X, polled.Y, polled.ScrollWheelValue, combinedLeft,
                        polled.MiddleButton, combinedRight, polled.XButton1, polled.XButton2, polled.HorizontalScrollWheelValue);
                    __result = newResult;
                }
            }
            catch (Exception e)
            {
                staticMonitor?.Log($"Failed in {nameof(GetMouseState_Postfix)}:\n{e}", LogLevel.Error);
            }
        }

        private static void DebugPrintButtonOverrides(ButtonState combinedLeft, ButtonState combinedRight, ButtonState polledLeft, ButtonState polledRight)
        {
            if (combinedLeft != polledLeft)
            {
                staticMonitor?.Log("Overriding for left button");
            }

            if (combinedRight != polledRight)
            {
                staticMonitor?.Log("Overriding for right button");
            }
        }

        private const byte BUTTON_LEFT = 1;
        private const byte BUTTON_RIGHT = 3;
        private static bool areWeBetweenFrames = false;
        private static readonly bool[] wasDownBetweenFrames = new bool[16];

        private static void OnMouseDown(byte button)
        {
            if (areWeBetweenFrames)
            {
                staticMonitor?.Log($"Button {button} down between frames!!!");
                if (button < wasDownBetweenFrames.Length)
                {
                    wasDownBetweenFrames[button] = true;
                }
            }
            else
            {
                staticMonitor?.Log($"Button {button} down not between frames.");
            }
        }

        private static void OnFrameStart()
        {
            areWeBetweenFrames = false;
            for (int i = 0; i != wasDownBetweenFrames.Length; ++i)
            {
                if (wasDownBetweenFrames[i])
                {
                    staticMonitor?.Log($"Button {i} was down between frames!!!");
                }
            }
        }

        private static void OnFrameEnd()
        {
            areWeBetweenFrames = true;
            for (int i = 0; i != wasDownBetweenFrames.Length; ++i)
            {
                wasDownBetweenFrames[i] = false;
            }
        }

        // MonoGame does not define an equivalent to SDL_MouseButtonEvent so we do it ourselves.
        public struct SdlMouseButtonEvent
        {
#pragma warning disable 0649
#pragma warning disable 0169
            public Sdl.EventType Type;

            public uint TimeStamp;

            public uint WindowId;

            public uint Which;

            public byte Button;

            public byte State;

            public byte clicks;

            private byte _padding1;

            public int X;

            public int Y;
#pragma warning restore 0169
#pragma warning restore 0649
        }

        // "Reinterprets" the MonoGame Sdl.Event struct as SDL_MouseButtonEvent
        [StructLayout(LayoutKind.Explicit)]
        public struct EventConverter
        {
            [FieldOffset(0)]
            public Sdl.Event MonoGameSdlEvent;

            [FieldOffset(0)]
            public SdlMouseButtonEvent SdlMouseButtonEvent;
        }
    }
}
