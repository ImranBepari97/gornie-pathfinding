using System;
using LoaderExtras;
using MemeLoader.ModUtilities;
using UnityEngine;
using Harmony;
using UnityEngine.AI;
using GorniePathfinding;


#region ERROR HELP
/* Q: I can't use any of Gorn's classes??
* -
* A: You need to add a reference to Gorn's assemblies, do so via:
* 
* View>Solution Explorer>Right-Click References>Add Reference>Browse>Navigate to Gorn's folder>Gorn_Data>Managed and from here add whatever you need.
* --
* Q: My mod won't load!
* -
* A: Have you filled out your ModInformation property? If so, check the CMD window when Gorn launches and look at the error, it'll tell you where the issue is occuring.
* ---
* Q: Gorn keeps saying NullReferenceException!!
* -
* A: 
* 
* Are you setting your variables inside the class deriving from IMod? Create a new class and derive it from MonoBehaviour or whatever.
* Don't use the entry class as the logic holder for your mod, it's supposed to be an overseer for your mod, creating/removing instances of your mod or initializing them.
* ----
* Q: Where's Harmony?
* -
* A: Add a reference to it, it's located where the MemeLauncher is installed.
*/
#endregion

namespace MyModNameSpace
{
    public class MyModEntry : IMod //Only have one class derive from this, it's an entry point.
    {


        public void OnLoaded()
        {
            //This is called when the mods are loaded, use as an initializer.
            Console.WriteLine($"Hello, world! Calling from {ModInformation.Name}!!");
            HarmonyInstance.Create("net.GornMods.Pathfinding").PatchAll();
            Debug.Log(" has finished setting up...");

            Console.WriteLine($"{ModInformation.Name} has loaded."); //The CMD window will pick up both Debug.Log and Console.WriteLine.
        }

        public void OnUnload()
        {
            //This is called when mods are unloaded, destroy any instances you've created here.

            Console.WriteLine($"{ModInformation.Name} has unloaded.");
        }


        public ModInformation ModInformation => new ModInformation
        {
            Name = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, // CHANGE AT => Project>MyModTemplate Properties>Build>Assembly Name

            Creator = "theCH33F",

            Description = "Adds real pathfinding to Gornies. Press D to disable/enable debug lines and logs.", //Explination of what mod does, e.g; Slows time down with magical powers!

            Version = "V2.0.0" //Mod version, you can name your versions whatever, example; V1/Version One/VER1/AAAAAAAA1
        };
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
            __instance.gameObject.AddComponent<PathFinder>();
        }

    }
}

    