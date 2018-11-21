using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using MemeMansMods;


#region Error help

/*Q = Question
 *A = Answer
 *O = Optional
 *I = Additional Information
 * - - - - - - - - - - - - - 
 * -Q: It says I'm missing an assembly reference?-
 * ==============================================
 * A: View>Solution Explorer>References>Right-click>Add>Clear all(if any show up)(Right-click one and clear all)>Browse>Project root>Plugins>Select All.
 * 
 * -Q: My mod won't load!- 
 * ========================
 *  A: Did you remove Init()? If not, everything should work, it'll be your code, double check!
 *  I: I keep dlSpy(.dll deassembler) open so I can see the source to understand what I'm modifying.
 *  
 *  -Q: I accidentally broke the game, help!-
 *  =========================================
 *  A: Delete: GORN_Data>Managed>Assembly-CSharp.dll and the most recent mod you broke it with.
 *  O: Verify file integrity, launching the game will start this automatically.
 */

#endregion

namespace MyModNameSpace {

    public class GorniePathFinding {


        #region Mod Information
        //Name set in Project>ProjectName Properties>Application>Assembly Name
        public string Creator = "theCH33F" , Version = "V0.1.0";

        public string Description = "Enabled Gornie's to intelligently path to you! Press D to see their calculated paths! Advised to only be enabled on custom maps.";
        
        public string ModName { get {return Assembly.GetExecutingAssembly().GetName().Name; } } //IGNORE THIS (Thanks Taco)
        #endregion

        public void Init ()
        {
            //This is called when the game starts.



            Debug.Log ( ModName + " has finished initializing!" );
        }

        #region Additional Methods



        public void OnUpdate ()
        {
            if (GameController.IsInCombatScene)
            {
                if(Player.reff.GetComponent<NavBuilder>() == null && !MemeMansMods.LoadManager.reff.loader.activeInHierarchy && GameController.GateIsDown)
                {
                    builder = Player.reff.gameObject.AddComponent<NavBuilder>();
                    builder.UpdateNavmeshData();
                    Debug.Log("Building NavMesh");
                }

            }

        }


        public void OnFixedUpdate ()
        {
            //called every physics frame
        }

        //NavMeshSurface mesh;
        NavBuilder builder;

        public void OnLevelLoaded (Scene scene, LoadSceneMode mode)
        {
            //called when a level is loaded
           
            
        }


        public void OnGUI ()
        {
            //called several times per frame (one call per event) - Used for GUI elements
        }
        #endregion

        #region Gorn callbacks
        public void OnEnemyDie (Enemy enemy, Crab crab, Bird bird)
        {
            //Called when an enemy dies.

            if (enemy != null) {
                
            } else if (crab != null) {
                //Crab died
            } else if (bird != null) {
                //Bird died
            }
        }

        public void OnEnemySetUp (EnemySetupInfo esi)
        {
            //Called before OnEnemySpawn
        }

        public void OnEnemySpawn (Enemy enemy, Crab crab)
        {
            
            if (enemy != null) {
                enemy.gameObject.AddComponent<PathFinder>();
                //Player.reff.gameObject.GetComponent<NavBuilder>().UpdateNavmeshData();
            } else if (crab != null) {
                //Crab Spawned
            }
        }

        public void OnPlayerDied ()
        {
            //Called when player dies
        }

        #region User Requests

        public void OnRegisterCrab ()
        {
            //Called when Crab registers in scene - [USER REQUEST]
        }

        public void OnBowFire (Bow bow)
        {
            //Called when a bow is fired
        }

        #endregion

        #endregion
    }

    public class CubeTarget : MonoBehaviour, AITargetable
    {
        public Vector3 position
        {
            get; set;
        }

        public Vector3 leftArmPos
        {
            get;
        }

        public Vector3 rightArmPos
        {
            get;
        }

        public bool IsDead
        {
           get;
        }

        public bool HasLeftArm()
        {
            return true;
        }

        public bool HasRightArm()
        {
            return true;
        }

        public bool IsPlayer()
        {
            return false;
        }
    }

    public class NavBuilder : MonoBehaviour
    {

        Vector3 BoundsCenter = Vector3.zero;
        Vector3 BoundsSize = new Vector3(512f, 4000f, 512f);

        LayerMask BuildMask;
        LayerMask NullMask;

        NavMeshData NavMeshData;
        NavMeshDataInstance NavMeshDataInstance;

        void Start()
        {
            AddNavMeshData();
            BuildMask = ~0;
            NullMask = 0;
            Debug.Log("Build " + Time.realtimeSinceStartup.ToString());
            Build();
            Debug.Log("Build finished " + Time.realtimeSinceStartup.ToString());
            Debug.Log("Update " + Time.realtimeSinceStartup.ToString());
            UpdateNavmeshData();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                Debug.Log("Build " + Time.realtimeSinceStartup.ToString());
                Build();
                Debug.Log("Build finished " + Time.realtimeSinceStartup.ToString());
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                Debug.Log("Update " + Time.realtimeSinceStartup.ToString());
                UpdateNavmeshData();
            }
        }

        void AddNavMeshData()
        {
            if (NavMeshData != null)
            {
                if (NavMeshDataInstance.valid)
                {
                    Debug.Log("Seems to be a NavMesh, removing.");
                    NavMesh.RemoveNavMeshData(NavMeshDataInstance);
                }
                NavMeshDataInstance = NavMesh.AddNavMeshData(NavMeshData);
            }
        }

        public void UpdateNavmeshData()
        {
            StartCoroutine(UpdateNavmeshDataAsync());
        }

        System.Collections.IEnumerator UpdateNavmeshDataAsync()
        {
            AsyncOperation op = NavMeshBuilder.UpdateNavMeshDataAsync(
                NavMeshData,
                NavMesh.GetSettingsByID(0),
                GetBuildSources(BuildMask),
                new Bounds(BoundsCenter, BoundsSize));
            yield return op;

            AddNavMeshData();
            Debug.Log("Update finished " + Time.realtimeSinceStartup.ToString());
        }

        void Build()
        {
            NavMeshData = NavMeshBuilder.BuildNavMeshData(
                NavMesh.GetSettingsByID(0),
                GetBuildSources(NullMask),
                new Bounds(BoundsCenter, BoundsSize),
                Vector3.zero,
                Quaternion.identity);
            AddNavMeshData();
        }

        List<NavMeshBuildSource> GetBuildSources(LayerMask mask)
        {
            List<NavMeshBuildSource> sources = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(
                new Bounds(BoundsCenter, BoundsSize),
                mask,
                NavMeshCollectGeometry.PhysicsColliders,
                0,
                new List<NavMeshBuildMarkup>(),
                sources);
            Debug.Log("Sources found: " + sources.Count.ToString());
            return sources;
        }

    }


    public class PathFinder : MonoBehaviour
    {
        GameObject goalPoint;
        EnemyAI ai;
        CubeTarget target;
        LineRenderer lr;
        static bool debugEnabled = false;

        void Start()
        {
            ai = gameObject.GetComponent<Enemy>().AI;
            lr = gameObject.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.material.color = Color.red;

            if (goalPoint == null)
            {
                goalPoint = new GameObject();
                Instantiate(goalPoint, transform.position, Quaternion.identity);
                target = goalPoint.AddComponent<CubeTarget>();
                Debug.Log("Added goalPoint to " + gameObject);
            }
        }


        void Update()
        {
            if(Input.GetKeyDown(KeyCode.D))
            {
                debugEnabled = !debugEnabled;
            }

            if (!ai.guy.dead)
            {

                NavMeshHit playerHit;
                NavMesh.SamplePosition(Player.reff.position, out playerHit, 20.0f, NavMesh.AllAreas);
                
                

                NavMeshHit gornieHit;
                //NavMesh.SamplePosition(gameObject.transform.position, out gornieHit, 20.0f, NavMesh.AllAreas);
                NavMesh.SamplePosition(ai.guy.head.skullAttachPoint.position, out gornieHit, 20.0f, NavMesh.AllAreas);
                

                NavMeshPath path = new NavMeshPath();
                bool canYou = NavMesh.CalculatePath(gornieHit.position, playerHit.position, NavMesh.AllAreas, path);
                
                if (canYou)
                {
                    ai.state = EnemyAI.State.MoveToPoint;

                    if(debugEnabled)
                    {
                        lr.SetPositions(path.corners);
                        lr.positionCount = path.corners.Length;
                        lr.enabled = true;
                    } else
                    {
                        lr.enabled = false;
                    }
                    

                    target.position = path.corners[1];
                    //ai.target = target;
                    ai.moveTarget = goalPoint.transform;

                    if (path.corners.Length == 2)
                    {
                       // ai.target = Player.reff;
                        ai.moveTarget = Player.reff.transform;
                        Debug.Log(gameObject.name + "is going straight to the player!");

                    }
                    else
                    {
                        goalPoint.transform.position = path.corners[1];
                        target.position = path.corners[1];
                        //ai.target = target;
                        ai.moveTarget = goalPoint.transform;
                        Debug.Log(gameObject.name + " next point is: " + path.corners[1]);
                    }

                }
                else
                {
                  //  ai.target = Player.reff;
                    ai.moveTarget = Player.reff.transform;
                    ai.state = EnemyAI.State.Fighting;
                    Debug.Log("No NavMesh path, going straight for player!");
                }

                if (debugEnabled)
                {
                    Debug.Log("---------------------------------------------------");
                    Debug.Log("Is there a path? " + canYou);
                    Debug.Log("Closest Player NavMesh pos: " + playerHit.position);
                    Debug.Log("Closest " + gameObject.name + " NavMesh pos: " + gornieHit.position);
                    Debug.Log("Player Position = " + Player.reff.position);
                    Debug.Log(gameObject.name + " Position = " + transform.position);
                    Debug.Log(gameObject.name + " target position: " + target.transform.position);
                    Debug.Log(gameObject.name + " target position according to AITargetable: " + target.position);
                }


                
            }
        }

    }
}