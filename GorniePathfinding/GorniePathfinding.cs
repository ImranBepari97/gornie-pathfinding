using GorniePathfinding;
using HarmonyLib;
using MemeLoader;
using MemeLoader.Modding;
using static MemeLoader.Logging.Logger;

namespace GornMods
{
    public class GorniePathfinding: ModEntry
    {
        /// <summary>
        /// Called when your mod is first loaded by the loader, best used to initialize your mod.
        /// </summary>
        public override void OnModInitialized(Mod mod)
        {
            // Your code goes here.

            new Harmony("com.GORN.Mods.GorniePathfinding").PatchAll();
            base.OnModInitialized(mod);
        }

       /// <summary>
       /// Called when your mod is unloaded, you should destroy any objects you created or any instances, remove any traces of your mod here.
       /// </summary>
       public override void OnModUnLoaded()
       {
            // Unload your mod here.
            
            
            base.OnModUnLoaded();
       }
    }

    [HarmonyPatch(typeof(GameController), "SetupLevel")]
    public class Spawn_Weapon
    {
        public static void Postfix(GameController __instance)
        {
            GameController.Player.gameObject.AddComponent<NavMeshAdder>();
        }

    }

    [HarmonyPatch(typeof(Enemy), "Start")]
    public class AddAI
    {
        public static void Postfix(Enemy __instance)
        {
            __instance.gameObject.AddComponent<Pathfinding>();
        }

    }
}