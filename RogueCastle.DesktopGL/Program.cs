using System;
using System.IO;
using SteamWorksWrapper;

namespace RogueCastle
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            bool loadGame = true;

            if (LevelEV.CREATE_RETAIL_VERSION == true)// && LevelEV.CREATE_INSTALLABLE == false)
            {
                Steamworks.Init();
                loadGame = Steamworks.WasInit;
            }

            // Don't really need this anymore... -flibit
            //if (loadGame == true)
            {
#if true
                // Dave's custom EV settings for localization testing
                //LevelEV.RUN_TESTROOM = true;// false; // true; // false;
                //LevelEV.LOAD_SPLASH_SCREEN = false; // true; // false;
                //LevelEV.CREATE_RETAIL_VERSION = false;
                //LevelEV.SHOW_DEBUG_TEXT = false; // true;
#endif

                LevelEV.LoadRetail();   

                if (args.Length == 1 && LevelEV.CREATE_RETAIL_VERSION == false)
                {
                    using (Game game = new Game(args[0]))
                    {
                        LevelEV.RUN_TESTROOM = true;
                        LevelEV.DISABLE_SAVING = true;
                        game.Run();
                    }
                }
                else
                {
                    if (LevelEV.RUN_CRASH_LOGS == true)
                    {
                        try
                        {
                            using (Game game = new Game())
                            {
                                game.Run();
                            }
                        }
                        catch (Exception e)
                        {
                            string date = DateTime.Now.ToString("dd-mm-yyyy_HH-mm-ss");
                            if (!Directory.Exists(Platform.OSDir))
                                Directory.CreateDirectory(Platform.OSDir);
                            string configFilePath = Path.Combine(Platform.OSDir, "CrashLog_" + date + ".log");

                            //using (StreamWriter writer = new StreamWriter("CrashLog_" + date + ".log", false))
                            using (StreamWriter writer = new StreamWriter(configFilePath, false))
                            {
                                writer.WriteLine(e.ToString());
                            }

                            Console.WriteLine(e.ToString());
                        }
                    }
                    else
                    {
                        using (Game game = new Game())
                        {
                            game.Run();
                        }
                    }
                }
            }
            //else
            //{
            //    #if STEAM
            //    SDL.SDL_ShowSimpleMessageBox(
            //        SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR,
            //        "Launch Error",
            //        "Please load Rogue Legacy from the Steam client",
            //        IntPtr.Zero
            //    );
            //    #endif
            //}
            Steamworks.Shutdown();
        }
    }
}

