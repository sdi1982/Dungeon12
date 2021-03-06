﻿#define TRACE

using Dungeon.Data;
using Dungeon.Resources;
using Dungeon12;
using System;
using System.Diagnostics;

namespace Dungeon.Monogame
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            DungeonGlobal.BindGlobal<Global>();
            Console.WriteLine(Global.Version);
#if DEBUG
            var resCompiler = new ResourceCompiler();
            resCompiler.Compile();

            Global.ExceptionRethrow = true;
            Global.GlobalExceptionHandling();
            //ResourceLoader.NotDisposingResources = true;
            //ResourceLoader.CacheImagesAndMasks = false;
            Store.Init(Global.GetSaveSerializeSettings());
#endif      
            Store.LoadAllAssemblies();

            Run();
        }

        static void Run(bool FATAL=false)
        {
            try
            {
                using (var game = new XNADrawClient())
                {
                    game.isFatal = FATAL;
                    Global.Exit += () =>
                    {
                        game.Dispose();
                        game.Exit();
                        Environment.Exit(0);
                    };
                    game.Run();
                }
            }
            catch (Exception ex)
            {
                throw;
                //Global.Logger.Log(ex.ToString());
                Run(true);
            }
        }
    }
}