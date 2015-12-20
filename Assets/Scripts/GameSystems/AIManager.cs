﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIManager : MonoBehaviour {

    public static AIManager staticManager;

    public static string PLAYER_LEADER = "PlayerLeader";
    public static string ENEMY_LEADER = "EnemyLeader";
    public static string PLAYER_PELOTON = "PlayerPeloton";
    public static string ENEMY_PELOTON = "EnemyPeloton";

    static float MERGE_DISTANCE = 25f;
    static float WATCH_DISTANCE = 30f;

    public int MAXIMUM_MINIONS = 15;

    GameObject playerLeader;
    GameObject enemyLeader;
    Leader playerLeaderScript;
    Leader enemyLeaderScript;
    List<Peloton> playerTeam = new List<Peloton>();
    List<Peloton> enemyTeam = new List<Peloton>();
    Door orangeDoor;
    Door purpleDoor;

    List<Totem> totems = new List<Totem>();

    Fruit fruitScript;


    private const float unitAdvantage = 1.2f; // Advantage in proportion of units in a conflict that makes an action decently profitable

    private const float minionWeight = 1f;
    private const float distanceWeight = 0.1f;//0.05f; // 1 minion = 20m

    private const float totemsValue = 10f; // 1 totem = 10 minions
    //private const float pushingValue = 0.01f;
    private const float pushingBaseValue = 10f;

    void Awake()
    {
        Application.targetFrameRate = 60;

        staticManager = this;

        GameObject[] totemGameObjects = GameObject.FindGameObjectsWithTag("Totem");
        foreach (GameObject t in totemGameObjects)
        {
            totems.Add(t.GetComponent<Totem>());
        }

        fruitScript = GameObject.Find("Fruit").GetComponent<Fruit>();
        orangeDoor = GameObject.Find(Names.PLAYER_DOOR).GetComponent<Door>();
        purpleDoor = GameObject.Find(Names.ENEMY_DOOR).GetComponent<Door>();
    }

    // Use this for initialization
    void Start () {
        playerLeader = GameObject.Find(PLAYER_LEADER);
        enemyLeader = GameObject.Find(ENEMY_LEADER);
        playerLeaderScript = playerLeader.GetComponent<PlayerLeader>();
        enemyLeaderScript = enemyLeader.GetComponent<EnemyLeader>();

        // Pelotón inicial del Lider // --DEPRECATED--
        //playerTeam.Add(playerLeaderScript.myPeloton);
        //enemyTeam.Add(enemyLeaderScript.myPeloton);  
    }

    public void AddPlayerPeloton(Peloton peloton)
    {
        playerTeam.Add(peloton);
    }
    public void AddEnemyPeloton(Peloton peloton)
    {
        enemyTeam.Add(peloton);
    }
    public void RemovePlayerPeloton(Peloton p)
    {
        playerTeam.Remove(p);
    }
    public void RemoveEnemyPeloton(Peloton p)
    {
        enemyTeam.Remove(p);
    }

    public void AddPeloton(Peloton peloton)
    {
        if (peloton.gameObject.name == Names.PLAYER_PELOTON) playerTeam.Add(peloton);
        else enemyTeam.Add(peloton);
    }

    public List<Peloton> GetNeighbourPelotons(Peloton peloton)
    {
        List<Peloton> neighbours = new List<Peloton>();
        if(peloton.GetLeader() == playerLeader)
        {
            foreach(Peloton p in playerTeam)
            {
                if (p != peloton && Vector3.Distance(p.transform.position, peloton.transform.position) < MERGE_DISTANCE)
                    neighbours.Add(p);
            }
        }
        else
        {
            foreach (Peloton p in enemyTeam)
            {
                if (p != peloton && Vector3.Distance(p.transform.position, peloton.transform.position) < MERGE_DISTANCE)
                    neighbours.Add(p);
            }
        }
        return neighbours;
    }

    public List<Minion> GetTeamMinions(string leader)
    {
        List<Minion> team = new List<Minion>();
        if(leader == Names.PLAYER_LEADER)
        {
            foreach(Peloton p in playerTeam)
            {
                foreach (Minion m in p.GetMinionList())
                    team.Add(m);
            }
        }
        else
        {
            foreach(Peloton p in enemyTeam)
            {
                foreach (Minion m in p.GetMinionList())
                    team.Add(m);
            }
        }

        return team;
    }

    public int GetTeamMinionsCount(string leader)
    {
        int count = 0;
        if (leader == PLAYER_LEADER)
        {
            foreach (Peloton p in playerTeam)
            {
                count += p.Size();
            }
        }
        else if (leader == ENEMY_LEADER)
        {
            foreach (Peloton p in enemyTeam)
            {
                count += p.Size();
            }
        }

        return count;
    }

    public int GetLeaderMinionsCount(string leader)
    {
        int count = 0;
        if (leader == PLAYER_LEADER) count = playerLeaderScript.myPeloton.Size();

        else if (leader == ENEMY_LEADER) count = enemyLeaderScript.myPeloton.Size();

        return count;
    }

    public List<Minion> GetMinionsInRange(float range, Vector3 position, string leader)
    {
        List<Minion> minions = new List<Minion>();

        if(leader == PLAYER_LEADER)
        {
            foreach(Peloton p in playerTeam)
            {
                foreach(Minion m in p.GetMinionList())
                {
                    if(m.peloton != playerLeaderScript.myPeloton && Vector3.Distance(position, m.transform.position) <= range)
                    {
                        minions.Add(m);
                    }
                }
            }
        }
        else
        {
            foreach (Peloton p in enemyTeam)
            {
                foreach (Minion m in p.GetMinionList())
                {
                    if (m.peloton != enemyLeaderScript.myPeloton && Vector3.Distance(position, m.transform.position) <= range)
                    {
                        minions.Add(m);
                    }
                }
            }
        }

        return minions;
    }


    public int GetAlignedTotemsCount(string owner)
    {
        int count = 0;

        if (owner == PLAYER_LEADER)
        {
            foreach (Totem t in totems)
            {
                if (t.alignment == 25f) count++;
            }
        }
        else if (owner == ENEMY_LEADER)
        {
            foreach (Totem t in totems)
            {
                if (t.alignment == -25f) count++;
            }
        }

        return count;
    }
    public int GetTotemCount()
    {
        return totems.Count;
    }


    public Leader GetLeaderByName(string name)
    {
        if (name == Names.PLAYER_LEADER) return playerLeaderScript;
        /*else if (name == Names.ENEMY_LEADER)*/ return enemyLeaderScript;
    }

    // this should depend on the state, to be implemented
    public List<Peloton> GetPelotonsByObjective(string leader, string objective)
    {
        List<Peloton> pelotons = new List<Peloton>();

        if (leader == Names.PLAYER_LEADER)
        {
            foreach (Peloton p in playerTeam)
            {
                if (p.GetObjectiveType() == objective) pelotons.Add(p);
            }
        }
        /*else
        {
            foreach (Peloton p in enemyTeam)
            {
                if (p.GetObjectiveType() == objective) pelotons.Add(p);
            }
        }*/

        return pelotons;
    }

    public List<Peloton> GetNearbyEnemies(Peloton peloton)
    {
        List<Peloton> neighbours = new List<Peloton>();
        if (peloton.GetLeader() == playerLeader)
        {
            foreach (Peloton p in enemyTeam)
            {
                if (p != peloton && Vector3.Distance(p.transform.position, peloton.transform.position) < WATCH_DISTANCE)
                    neighbours.Add(p);
            }
        }
        else
        {
            foreach (Peloton p in playerTeam)
            {
                if (p != peloton && Vector3.Distance(p.transform.position, peloton.transform.position) < WATCH_DISTANCE)
                    neighbours.Add(p);
            }
        }
        return neighbours;
    }

    public List<Peloton> GetPelotonsInRange(float range, Vector3 position, string leader)
    {
        List<Peloton> pelotons = new List<Peloton>();

        if(leader == Names.PLAYER_LEADER)
        {
            foreach (Peloton p in playerTeam)
            {
                if (Vector3.Distance(p.transform.position, position) < range) pelotons.Add(p);
            }
        }
        else /*if (leader.name == Names.ENEMY_LEADER)*/
        {
            foreach (Peloton p in enemyTeam)
            {
                if (Vector3.Distance(p.transform.position, position) < range) pelotons.Add(p);
            }
        }

        return pelotons;
    }

    /*public List<Strategy> GetAIStrategies()
    {
        List<Strategy> options = new List<Strategy>();

        float tacticCost, tacticReward;
        int necessaryMinions, remainingMinions;
        Stack<Tactic> strategyPlan = new Stack<Tactic>();
        List<Tactic> gathering = new List<Tactic>();



        // TOTEMS
        foreach (Totem t in totems)
        {
            necessaryMinions = 5; // Minimum minions to be reasonable
            necessaryMinions += Mathf.FloorToInt(GetMinionsInRange(20f, t.transform.position, Names.PLAYER_LEADER).Count * unitAdvantage);

            // Get those MINIONS!!
            remainingMinions = necessaryMinions - enemyLeaderScript.myPeloton.Size();
            gathering = MinionGathering(remainingMinions);

            tacticCost = necessaryMinions * minionWeight;
            tacticCost += Vector3.Distance(enemyLeader.transform.position, t.transform.position) * distanceWeight;
            
            // Add the cost of each sub-process
            foreach (Tactic tc in gathering)
            {
                tacticCost += tc.cost;
            }

            tacticReward = 1f / (GetAlignedTotemsCount(Names.ENEMY_LEADER) + 1f/int.MaxValue);
            tacticReward += Mathf.Pow(GetAlignedTotemsCount(Names.PLAYER_LEADER), 2);
            tacticReward *= totemsValue;

            strategyPlan.Push(new Tactic(tacticCost, tacticReward, t.gameObject, true, necessaryMinions)); //MainTactic
            foreach (Tactic tc in gathering) // sub-tactics
                strategyPlan.Push(tc);

            Strategy newStrategy = new Strategy(strategyPlan, tacticCost, tacticReward);
            options.Add(newStrategy);
        }


        // PUSH MELON
        necessaryMinions = 5; // Minimum minions to be reasonable
        List<Peloton> pelotons = GetPelotonsByObjective(Names.PLAYER_LEADER, Names.OBJECTIVE_PUSH);
        foreach (Peloton p in pelotons)
            necessaryMinions += Mathf.FloorToInt(p.Size() * unitAdvantage);
        pelotons = GetPelotonsByObjective(Names.ENEMY_LEADER, Names.OBJECTIVE_PUSH);
        foreach (Peloton p in pelotons)
            necessaryMinions -= p.Size();

        // Get those MINIONS!!
        remainingMinions = necessaryMinions - enemyLeaderScript.myPeloton.Size();
        gathering = MinionGathering(remainingMinions);

        tacticCost = necessaryMinions * minionWeight;
        tacticCost += Vector3.Distance(enemyLeader.transform.position, fruitScript.transform.position) * distanceWeight;

        // Add the cost of each sub-process
        foreach (Tactic tc in gathering)
        {
            tacticCost += tc.cost;
        }

        tacticReward = Mathf.Pow(fruitScript.transform.position.z, 2) * pushingValue + pushingBaseValue;
        //if(doorUp && fruitScript.transform.position.z > 0) tacticReward = 1/DistanceToYourBase;





        return options;
    }*/

    public List<Strategy> GetAIStrategies()
    {
        List<Strategy> options = new List<Strategy>();

        //  TOTEM STRATEGIES
        List<Strategy> totemStrategies = GetTotemStrategies();
        foreach (Strategy ts in totemStrategies)
            options.Add(ts);

        // PUSH STRATEGY
        options.Add(GetPushStrategy());

        // ATTACK DOOR STRATEGY
        options.Add(GetAttackDoorStrategy());

        return options;
    }

    private List<Strategy> GetTotemStrategies()
    {
        List<Strategy> options = new List<Strategy>();


        // TOTEMS
        foreach (Totem t in totems)
        {
            float tacticCost, tacticReward;
            int necessaryMinions, remainingMinions;
            Stack<Tactic> strategyPlan = new Stack<Tactic>();
            List<Tactic> gathering = new List<Tactic>();

            necessaryMinions = 5; // Minimum minions to be reasonable
            necessaryMinions += Mathf.FloorToInt(GetMinionsInRange(20f, t.transform.position, Names.PLAYER_LEADER).Count * unitAdvantage);

            // Get those MINIONS!!
            remainingMinions = necessaryMinions - enemyLeaderScript.myPeloton.Size();
            gathering = MinionGathering(remainingMinions);

            tacticCost = necessaryMinions * minionWeight;
            tacticCost += Vector3.Distance(enemyLeader.transform.position, t.transform.position) * distanceWeight;

            //tacticReward = 1f / (GetAlignedTotemsCount(Names.ENEMY_LEADER) + 1f / int.MaxValue);
            //tacticReward += Mathf.Pow(GetAlignedTotemsCount(Names.PLAYER_LEADER), 2); // Urgencia
            tacticReward = (-GetAlignedTotemsCount(Names.ENEMY_LEADER) + 12) * 8.333f;
            tacticReward += (GetAlignedTotemsCount(Names.PLAYER_LEADER)) * 8.333f;
            //tacticReward *= totemsValue;



            strategyPlan.Push(new Tactic(tacticCost, tacticReward, t.gameObject, false, necessaryMinions)); //MainTactic
            foreach (Tactic tc in gathering) // sub-tactics
                strategyPlan.Push(tc);

            Strategy newStrategy = new Strategy(strategyPlan);
            options.Add(newStrategy);
        }

        return options;
    }

    private Strategy GetPushStrategy()
    {
        float tacticCost, tacticReward;
        int necessaryMinions, remainingMinions;
        Stack<Tactic> strategyPlan = new Stack<Tactic>();
        List<Tactic> gathering = new List<Tactic>();

        int playerMinionsPushing = 0;
        int enemyMinionsPushing = 0;

        // PUSH MELON
        necessaryMinions = 5; // Minimum minions to be reasonable
        List<Peloton> pelotons = GetPelotonsByObjective(Names.PLAYER_LEADER, Names.OBJECTIVE_PUSH);
        foreach (Peloton p in pelotons) {
            necessaryMinions += Mathf.FloorToInt(p.Size() * unitAdvantage);
            playerMinionsPushing += p.Size();
        }
        pelotons = GetPelotonsByObjective(Names.ENEMY_LEADER, Names.OBJECTIVE_PUSH);
        foreach (Peloton p in pelotons)
        {
            necessaryMinions -= p.Size();
            enemyMinionsPushing += p.Size();
        }
            

        // Get those MINIONS!!
        remainingMinions = necessaryMinions - enemyLeaderScript.myPeloton.Size();
        gathering = MinionGathering(remainingMinions);

        tacticCost = necessaryMinions * minionWeight;
        tacticCost += Vector3.Distance(enemyLeader.transform.position, fruitScript.transform.position) * distanceWeight;

        // Distancia Pusheo
        if (!orangeDoor.doorsUp)
            tacticReward = Mathf.Abs(fruitScript.transform.position.z) / 3.6f;                                          //tacticReward = Mathf.Pow(fruitScript.transform.position.z, 2) * pushingValue + pushingBaseValue;
        else
            tacticReward = Vector3.Distance(fruitScript.transform.position, orangeDoor.transform.position) / 7.2f;      //tacticReward = 500000f / Mathf.Pow(Vector3.Distance(fruitScript.transform.position, purpleDoor.transform.position), 2);

        //Proporción minions que empujan
        tacticReward += playerMinionsPushing / (enemyMinionsPushing + 1f);  //tacticReward += playerMinionsPushing / (enemyMinionsPushing + 1f / int.MaxValue); //Urgencia

        //Ventaja de partida
        tacticReward += GameAdvantage();

        //Base
        tacticReward += pushingBaseValue;

        strategyPlan.Push(new Tactic(tacticCost, tacticReward, fruitScript.gameObject, false, necessaryMinions));
        foreach (Tactic tc in gathering) // sub-tactics
            strategyPlan.Push(tc);

        return new Strategy(strategyPlan);
    }

    private Strategy GetAttackDoorStrategy()
    {
        float tacticCost = 0;
        float tacticReward = 0;
        int necessaryMinions = 0;
        Stack<Tactic> strategyPlan = new Stack<Tactic>();

        List<Tactic> gathering = GatherAllOptimalMinionsToCall();

        foreach (Tactic tc in gathering)
            necessaryMinions += Mathf.FloorToInt(tc.targetElement.GetComponent<Peloton>().Size());

        tacticCost = necessaryMinions * minionWeight;
        tacticCost += Vector3.Distance(enemyLeader.transform.position, orangeDoor.transform.position) * distanceWeight;

        // Melon position
        tacticReward = Vector3.Distance(purpleDoor.transform.position, fruitScript.transform.position) / 7.2f; //   2 * 360/100      //tacticReward = 1f/Vector3.Distance(fruitScript.transform.position, orangeDoor.transform.position);
        tacticReward += GameAdvantage();

        strategyPlan.Push(new Tactic(tacticCost, tacticReward, orangeDoor.gameObject, Random.value > 0.5f, necessaryMinions));

        return new Strategy(strategyPlan);
    }

    private List<Tactic> MinionGathering(int remainingMinions)
    {
        List<Tactic> gathering = new List<Tactic>();
        if (remainingMinions > 0)
        {
            gathering = SearchMinionsToCall(remainingMinions);
            gathering.Sort((c1, c2) => (int)(c2.determination - c1.determination)); // Chapuza para priorizar los que están en objetivo defend // Comentario por si acaso esto no tira, ja, ilusa, yolanda... ¿de verdad crees que no me va a tirar..?
            int minionCount = 0;
            int index = 0;
            while (minionCount < remainingMinions && index < gathering.Count)
            {
                minionCount += gathering[index].targetElement.GetComponent<Peloton>().Size();
                index++;
            }
            gathering.RemoveRange(index, gathering.Count - index);
        }

        return TacticRouteOptimization(gathering);
    }

    private List<Tactic> SearchMinionsToCall(int cant)
    {
        List<Tactic> recruits = new List<Tactic>();
        foreach (Peloton p in GetPelotonsInRange(60f, enemyLeader.transform.position, enemyLeader.name))
        {
            float minionReward = p.Size() * minionWeight;
            float distanceCost = Vector3.Distance(enemyLeader.transform.position, p.transform.position) * distanceWeight;
            if (minionReward > distanceCost)
            {
                recruits.Add(new Tactic(distanceCost, minionReward, p.gameObject, true, 0));
            }
        }
        return recruits;
    }

    private List<Tactic> GatherAllOptimalMinionsToCall()
    {
        List<Tactic> recruits = new List<Tactic>();
        foreach (Peloton p in enemyTeam)
        {
            float minionReward = p.Size() * minionWeight;
            float distanceCost = Vector3.Distance(enemyLeader.transform.position, p.transform.position) * distanceWeight;
            if (minionReward > distanceCost)
            {
                recruits.Add(new Tactic(distanceCost, minionReward, p.gameObject, true, 0));
            }
        }

        return TacticRouteOptimization(recruits); 
    }

    private float GameAdvantage()
    {
        float gAdv = 0;

        //gAdv = Mathf.Pow(GetTeamMinionsCount(Names.ENEMY_LEADER), 2) / GetTeamMinionsCount(Names.PLAYER_LEADER) * minionWeight;
        //gAdv += GetAlignedTotemsCount(Names.ENEMY_LEADER) * totemsValue;

        gAdv = (GetTeamMinionsCount(Names.ENEMY_LEADER) - GetTeamMinionsCount(Names.PLAYER_LEADER)) * 3.3f;
        gAdv += GetAlignedTotemsCount(Names.ENEMY_LEADER) * 8.333f;

        if (gAdv < 0) gAdv = 0;

        return gAdv;
    }

    private List<Tactic> TacticRouteOptimization(List<Tactic> plan)
    {
        if (plan.Count == 0)
            return plan;

        List<Tactic> finalPlan = new List<Tactic>();
        Tactic lastTactic = new Tactic(0, 0, enemyLeader, false, 0);
        Tactic nearestTactic = plan[0];

        int iterations = plan.Count;

        for (int i = 0; i < iterations; i++)
        {
            /*foreach (Tactic tc in plan)
            {
                if (Vector3.Distance(tc.targetElement.transform.position, lastTactic.targetElement.transform.position) < Vector3.Distance(nearestTactic.targetElement.transform.position, lastTactic.targetElement.transform.position))
                    nearestTactic = tc;
            }
            finalPlan.Add(nearestTactic);
            plan.Remove(nearestTactic);
            lastTactic = nearestTactic;*/

            int tci = 0;
            while(tci < plan.Count)
            {
                if (Vector3.Distance(plan[tci].targetElement.transform.position, lastTactic.targetElement.transform.position) < Vector3.Distance(nearestTactic.targetElement.transform.position, lastTactic.targetElement.transform.position))
                    nearestTactic = plan[tci];
                tci++;
            }
            finalPlan.Add(nearestTactic);
            plan.Remove(nearestTactic);
            lastTactic = nearestTactic;
        }

        return finalPlan;
    }
}
