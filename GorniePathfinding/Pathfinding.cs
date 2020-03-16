using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AI;

namespace GorniePathfinding
{
    public class Pathfinding : MonoBehaviour
    {
        GameObject goalPoint;
        EnemyAI ai;
        LineRenderer lr;
        Player player;
        public static bool debugEnabled = false;
        bool aiStarted;


        void Start()
        {
            ai = gameObject.GetComponent<Enemy>().AI;
            lr = gameObject.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.material.color = Color.red;
            player = GameController.Player;
            aiStarted = false; //So the Gornie will walk into the arena first before doing pathfinding

            if (goalPoint == null)
            {
                goalPoint = new GameObject();
                Instantiate(goalPoint, transform.position, Quaternion.identity);
                Debug.Log("Added goalPoint to " + gameObject);
            }
        }


        void Update()
        {

            if(ai.state == EnemyAI.State.Fighting)
            {
                aiStarted = true; //Once the Gornie starts fighting, when it enters the arena fully, enable the pathfinding from then on 
            }

            if (!ai.guy.dead && aiStarted)
            {

                NavMeshHit playerHit;
                NavMesh.SamplePosition(player.position, out playerHit, 20.0f, NavMesh.AllAreas); //See if the player is on a NavMesh

                NavMeshHit gornieHit;
                //NavMesh.SamplePosition(gameObject.transform.position, out gornieHit, 20.0f, NavMesh.AllAreas);
                NavMesh.SamplePosition(ai.guy.head.skullAttachPoint.position, out gornieHit, 20.0f, NavMesh.AllAreas); //See if this current Gornie is on the NavMesh


                NavMeshPath path = new NavMeshPath(); 
                bool canYou = NavMesh.CalculatePath(gornieHit.position, playerHit.position, NavMesh.AllAreas, path); //If there's a path now, make it.

                if (canYou) //From everything we did earlier, could we find a path between a player and this enemy?
                {
                    ai.state = EnemyAI.State.MoveToPoint; //Change the enemy to move to point

                    if (debugEnabled) //Draw debug lines
                    {
                        lr.SetPositions(path.corners);
                        lr.positionCount = path.corners.Length;
                        lr.enabled = true;
                    }
                    else
                    {
                        lr.enabled = false;
                    }


                    //target.position = path.corners[1]; 
                    ai.moveTarget = goalPoint.transform;

                    if (path.corners.Length == 2) //Two corners means point 1 is where the enemy is, point 2 is player. There's a straight line
                    {
                        ai.moveTarget = player.transform;

                        if(debugEnabled)
                            Debug.Log(gameObject.name + "is going straight to the player!");

                    }
                    else
                    {
                        goalPoint.transform.position = path.corners[1];
                        ai.moveTarget = goalPoint.transform;
                        if(debugEnabled)
                            Debug.Log(gameObject.name + " next point is: " + path.corners[1]);
                    }

                }
                else //If there's no path, just do default behaviour
                {
                    ai.moveTarget = player.transform;
                    ai.state = EnemyAI.State.Fighting;
                    
                }

                if (debugEnabled) //More debug
                {
                    Debug.Log("---------------------------------------------------");
                    Debug.Log("Is there a path for? " + canYou);
                    Debug.Log("Closest Player NavMesh pos: " + playerHit.position);
                    Debug.Log("Closest " + gameObject.name + " NavMesh pos: " + gornieHit.position);
                    Debug.Log("Player Position = " + player.position);
                    Debug.Log(gameObject.name + " Position = " + transform.position);
                }
            }
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

    public class NavMeshAdder : MonoBehaviour {
       NavBuilder builder;
    
       public void Update()
        {
            if (GameController.IsInCombatScene)
            {
                if (GameController.Player.GetComponent<NavBuilder>() == null && GameController.GateIsDown)
                {
                    builder = GameController.Player.gameObject.AddComponent<NavBuilder>();
                    builder.UpdateNavmeshData();
                    Debug.Log("Building NavMesh");
                }

            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                Pathfinding.debugEnabled = !Pathfinding.debugEnabled;
                Console.WriteLine("Debug Enabled for Pathfinding: " + Pathfinding.debugEnabled);
            }
        }
    }


}
