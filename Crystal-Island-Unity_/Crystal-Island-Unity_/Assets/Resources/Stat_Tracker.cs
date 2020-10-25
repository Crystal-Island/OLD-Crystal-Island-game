using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine;


namespace Polymoney
{
    public class Stat_Tracker : NetworkBehaviour
    {
        public bool urlOpened = false;

        //Holds the statistics for the each player
        public struct PlayerStatistics
        {
            public bool active;
            public int playerID;
            public int playerFreeTime;
            public int numWCOffers, valueWCOffers, numWCBought, valueWCBought, numWCSold, valueWCSold;
            public int numGoldOffers, valueGoldOffers, numGoldBought, valueGoldBought, numGoldSold, valueGoldSold;

            public PlayerStatistics(int id)
            {
                active = false;
                playerID = id;
                playerFreeTime = 0;
                numWCOffers = 0;
                valueWCOffers = 0;
                numWCBought = 0;
                valueWCBought = 0;
                numWCSold = 0;
                valueWCSold = 0;
                numGoldOffers = 0;
                valueGoldOffers = 0;
                numGoldBought = 0;
                valueGoldBought = 0;
                numGoldSold = 0;
                valueGoldSold = 0;
            }

            public void PrintStats()
            {
                Debug.Log("For " + playerID + ":\n WC Stats:" + numWCOffers + " " + numWCBought + " " + numWCSold + "\nGold Stats: " + numGoldOffers);
            }
        }

        public static bool gameOver = false;

        private int listSynced = 0;

        [SyncVar]
        public PlayerStatistics player1Stats = new PlayerStatistics(0);
        [SyncVar]
        public PlayerStatistics player2Stats = new PlayerStatistics(1);
        [SyncVar]
        public PlayerStatistics player3Stats = new PlayerStatistics(2);
        [SyncVar]
        public PlayerStatistics player4Stats = new PlayerStatistics(3);
        [SyncVar]
        public PlayerStatistics player5Stats = new PlayerStatistics(4);
        [SyncVar]
        public PlayerStatistics player6Stats = new PlayerStatistics(5);
        [SyncVar]
        public PlayerStatistics player7Stats = new PlayerStatistics(6);
        [SyncVar]
        public PlayerStatistics player8Stats = new PlayerStatistics(7);

        [SyncVar]
        public float player1Score, player2Score, player3Score, player4Score, player5Score, player6Score, player7Score, player8Score;

        //Stat Messages for Mayor
        private string mayorLight = "Congratulations! You guided Crystal Island through the darkest of times. You worked with citizens and prospered together.";
        private string mayorCommons = "Well done! You kept the community working, even in challenging times.";
        private string mayorCompassion = "You were a selfless mayor. You supported your citizens even when funds were low. Good try!";
        private string mayorHeart = "Instead of supporting citizens in need, you accumulated Gold. Unfortunately, when some fail, everyone fails.";

        //Stat Messages Win Citizens
        private string crystalGuard = "<color=yellow>Crystal Guardian</color> --  Magnificent! You used your talents to prosper and help others.";
        private string communityBuild = "<color=yellow>Community Builder</color> -- Fantastic! You saw opportunities and supported your community.";
        private string talentShape = "<color=yellow>Talent Shaper</color> -- Awesome! You really applied your talents to solve problems.";
        private string resourceExplore = "<color=yellow>Resource Explorer</color> -- Terrific! You worked to meet your needs and help others.";
        private string tradeMaker = "<color=yellow>Trade Maker</color> -- Wonderful!  You collaborated with others so everyone wins!";
        //Stat Messages Lose Citizens
        private string talentBuild = "<color=yellow>Talent Builder</color> -- Bravo!  You applied your talents and built the community, but not quite enough.  Next time you'll do it!";
        private string communityShape = "<color=yellow>Community Shaper</color> -- Right on! You helped out the Mayor, but not quite enough.  Next time you'll do it!";
        private string resourceExplore2 = "<color=yellow>Resource Explorer</color> -- Cool! You solved some problems, but not quite enough.  Next time you'll do it!";
        private string crystalTrade = "<color=yellow>Crystal Trader</color> -- Nice! You worked with your talents, but not quite enough.  Next time you'll do it! ";
        private string crystalKeep = "<color=yellow>Crystal Keeper</color> -- OK! You survived a few rounds, but not quite enough.  Next time you'll do it!";

        public Image badgeImage;
        public Text badgeText;
        public Text earnedText;


        // Use this for initialization
        void Start()
        {
            StartCoroutine(InitializationRoutine());
            
        }

        // Update is called once per frame
        void Update()
        {
            GetStatistics();
            /*
            if(playerNetworkID.Count < Level.instance.allPlayers.Count && Level.instance.authoritativePlayer.isServer)
            {
                //Get a list of the network ids of all the players
                for (int i = 0; i < Level.instance.allPlayers.Count; i++)
                {
                    playerNetworkID.Add(Level.instance.allPlayers[i].netId.ToString());
                }
            }
            */
        }

        IEnumerator InitializationRoutine()
        {
            
            while(!gameOver)
            {
                yield return null;
            }
            
            if(!Level.instance.authoritativePlayer.Mayor)
            {
                if(Level.instance.authoritativePlayer.isServer)
                {
                    var currentPlayer = Level.instance.authoritativePlayer;
                    int index = currentPlayer.Person.Id;
                    PlayerStatistics playerStats = new PlayerStatistics(index);
                    playerStats.playerFreeTime = (100 - currentPlayer.Job.TimeCost);
                    playerStats.numWCOffers = (currentPlayer.numWCOffers);
                    playerStats.valueWCOffers = (currentPlayer.valueWCOffers);
                    playerStats.numWCBought = (currentPlayer.numWCBought);
                    playerStats.valueWCBought = (currentPlayer.numWCBought);
                    playerStats.numWCSold = (currentPlayer.numWCSold);
                    playerStats.valueWCSold = (currentPlayer.valueWCSold);
                    playerStats.numGoldOffers = (currentPlayer.numGoldOffers);
                    playerStats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    playerStats.numGoldBought = (currentPlayer.numGoldBought);
                    playerStats.valueGoldBought = (currentPlayer.valueGoldBought);
                    playerStats.numGoldSold = (currentPlayer.numGoldSold);
                    playerStats.valueGoldSold = (currentPlayer.valueGoldSold);
                    SendStatistics(index, playerStats);
                }
                else
                {
                    var currentPlayer = Level.instance.authoritativePlayer;
                    int index = currentPlayer.Person.Id;
                    PlayerStatistics playerStats = new PlayerStatistics(index);
                    playerStats.playerFreeTime = (100 - currentPlayer.Job.TimeCost);
                    playerStats.numWCOffers = (currentPlayer.numWCOffers);
                    playerStats.valueWCOffers = (currentPlayer.valueWCOffers);
                    playerStats.numWCBought = (currentPlayer.numWCBought);
                    playerStats.valueWCBought = (currentPlayer.numWCBought);
                    playerStats.numWCSold = (currentPlayer.numWCSold);
                    playerStats.valueWCSold = (currentPlayer.valueWCSold);
                    playerStats.numGoldOffers = (currentPlayer.numGoldOffers);
                    playerStats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    playerStats.numGoldBought = (currentPlayer.numGoldBought);
                    playerStats.valueGoldBought = (currentPlayer.valueGoldBought);
                    playerStats.numGoldSold = (currentPlayer.numGoldSold);
                    playerStats.valueGoldSold = (currentPlayer.valueGoldSold);
                    CmdSendStatistics(index, playerStats);
                }
            }
            yield return new WaitForSeconds(5f);
            //Open the feedback page
            Debug.Log("Opening Feedback");
            Application.OpenURL("https://www.polymoney.org/CI-Feedback");
        }

        public void SendStatistics(int id, PlayerStatistics currentPlayer)
        {
            var currentPlayerStats = player8Stats;
            switch (id)
            {
                case 0:
                    Debug.Log("Recieving data from player 1");
                    currentPlayerStats = player1Stats;
                    player1Stats.playerFreeTime = currentPlayer.playerFreeTime;
                    player1Stats.numWCOffers = currentPlayer.numWCOffers;
                    player1Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player1Stats.numWCBought = (currentPlayer.numWCBought);
                    player1Stats.valueWCBought = (currentPlayer.numWCBought);
                    player1Stats.numWCSold = (currentPlayer.numWCSold);
                    player1Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player1Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player1Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player1Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player1Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player1Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player1Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player1Stats.active = true;
                    player1Score = GetScore(currentPlayer);
                    break;
                case 1:
                    Debug.Log("Recieving data from player 2");
                    currentPlayerStats = player2Stats;
                    player2Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player2Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player2Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player2Stats.numWCBought = (currentPlayer.numWCBought);
                    player2Stats.valueWCBought = (currentPlayer.numWCBought);
                    player2Stats.numWCSold = (currentPlayer.numWCSold);
                    player2Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player2Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player2Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player2Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player2Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player2Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player2Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player2Stats.active = true;
                    player2Score = GetScore(currentPlayer);
                    break;
                case 2:
                    Debug.Log("Recieving data from player 3");
                    currentPlayerStats = player3Stats;
                    player3Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player3Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player3Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player3Stats.numWCBought = (currentPlayer.numWCBought);
                    player3Stats.valueWCBought = (currentPlayer.numWCBought);
                    player3Stats.numWCSold = (currentPlayer.numWCSold);
                    player3Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player3Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player3Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player3Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player3Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player3Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player3Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player3Stats.active = true;
                    player3Score = GetScore(currentPlayer);
                    break;
                case 3:
                    Debug.Log("Recieving data from player 4");
                    currentPlayerStats = player4Stats;
                    player4Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player4Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player4Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player4Stats.numWCBought = (currentPlayer.numWCBought);
                    player4Stats.valueWCBought = (currentPlayer.numWCBought);
                    player4Stats.numWCSold = (currentPlayer.numWCSold);
                    player4Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player4Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player4Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player4Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player4Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player4Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player4Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player4Stats.active = true;
                    player4Score = GetScore(currentPlayer);
                    break;
                case 4:
                    Debug.Log("Recieving data from player 5");
                    currentPlayerStats = player5Stats;
                    player5Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player5Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player5Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player5Stats.numWCBought = (currentPlayer.numWCBought);
                    player5Stats.valueWCBought = (currentPlayer.numWCBought);
                    player5Stats.numWCSold = (currentPlayer.numWCSold);
                    player5Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player5Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player5Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player5Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player5Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player5Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player5Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player5Stats.active = true;
                    player5Score = GetScore(currentPlayer);
                    break;
                case 5:
                    Debug.Log("Recieving data from player 6");
                    currentPlayerStats = player6Stats;
                    player6Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player6Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player6Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player6Stats.numWCBought = (currentPlayer.numWCBought);
                    player6Stats.valueWCBought = (currentPlayer.numWCBought);
                    player6Stats.numWCSold = (currentPlayer.numWCSold);
                    player6Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player6Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player6Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player6Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player6Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player6Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player6Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player6Stats.active = true;
                    player6Score = GetScore(currentPlayer);
                    break;
                case 6:
                    Debug.Log("Recieving data from player 7");
                    currentPlayerStats = player7Stats;
                    player7Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player7Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player7Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player7Stats.numWCBought = (currentPlayer.numWCBought);
                    player7Stats.valueWCBought = (currentPlayer.numWCBought);
                    player7Stats.numWCSold = (currentPlayer.numWCSold);
                    player7Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player7Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player7Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player7Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player7Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player7Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player7Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player7Stats.active = true;
                    player7Score = GetScore(currentPlayer);
                    break;
                case 7:
                    Debug.Log("Recieving data from player 8");
                    currentPlayerStats = player8Stats;
                    player8Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player8Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player8Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player8Stats.numWCBought = (currentPlayer.numWCBought);
                    player8Stats.valueWCBought = (currentPlayer.numWCBought);
                    player8Stats.numWCSold = (currentPlayer.numWCSold);
                    player8Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player8Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player8Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player8Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player8Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player8Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player8Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player8Stats.active = true;
                    player8Score = GetScore(currentPlayer);
                    break;
            }
        }

        [Command]
        public void CmdSendStatistics(int id, PlayerStatistics currentPlayer)
        {
            var currentPlayerStats = player8Stats;
            switch(id)
            {
                case 0:
                    Debug.Log("Recieving data from player 1");
                    currentPlayerStats = player1Stats;
                    player1Stats.playerFreeTime = currentPlayer.playerFreeTime;
                    player1Stats.numWCOffers = currentPlayer.numWCOffers;
                    player1Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player1Stats.numWCBought = (currentPlayer.numWCBought);
                    player1Stats.valueWCBought = (currentPlayer.numWCBought);
                    player1Stats.numWCSold = (currentPlayer.numWCSold);
                    player1Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player1Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player1Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player1Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player1Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player1Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player1Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player1Stats.active = true;
                    player1Score = GetScore(currentPlayer);
                    break;
                case 1:
                    Debug.Log("Recieving data from player 2");
                    currentPlayerStats = player2Stats;
                    player2Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player2Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player2Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player2Stats.numWCBought = (currentPlayer.numWCBought);
                    player2Stats.valueWCBought = (currentPlayer.numWCBought);
                    player2Stats.numWCSold = (currentPlayer.numWCSold);
                    player2Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player2Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player2Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player2Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player2Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player2Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player2Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player2Stats.active = true;
                    player2Score = GetScore(currentPlayer);
                    break;
                case 2:
                    Debug.Log("Recieving data from player 3");
                    currentPlayerStats = player3Stats;
                    player3Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player3Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player3Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player3Stats.numWCBought = (currentPlayer.numWCBought);
                    player3Stats.valueWCBought = (currentPlayer.numWCBought);
                    player3Stats.numWCSold = (currentPlayer.numWCSold);
                    player3Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player3Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player3Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player3Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player3Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player3Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player3Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player3Stats.active = true;
                    player3Score = GetScore(currentPlayer);
                    break;
                case 3:
                    Debug.Log("Recieving data from player 4");
                    currentPlayerStats = player4Stats;
                    player4Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player4Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player4Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player4Stats.numWCBought = (currentPlayer.numWCBought);
                    player4Stats.valueWCBought = (currentPlayer.numWCBought);
                    player4Stats.numWCSold = (currentPlayer.numWCSold);
                    player4Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player4Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player4Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player4Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player4Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player4Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player4Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player4Stats.active = true;
                    player4Score = GetScore(currentPlayer);
                    break;
                case 4:
                    Debug.Log("Recieving data from player 5");
                    currentPlayerStats = player5Stats;
                    player5Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player5Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player5Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player5Stats.numWCBought = (currentPlayer.numWCBought);
                    player5Stats.valueWCBought = (currentPlayer.numWCBought);
                    player5Stats.numWCSold = (currentPlayer.numWCSold);
                    player5Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player5Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player5Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player5Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player5Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player5Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player5Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player5Stats.active = true;
                    player5Score = GetScore(currentPlayer);
                    break;
                case 5:
                    Debug.Log("Recieving data from player 6");
                    currentPlayerStats = player6Stats;
                    player6Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player6Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player6Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player6Stats.numWCBought = (currentPlayer.numWCBought);
                    player6Stats.valueWCBought = (currentPlayer.numWCBought);
                    player6Stats.numWCSold = (currentPlayer.numWCSold);
                    player6Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player6Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player6Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player6Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player6Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player6Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player6Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player6Stats.active = true;
                    player6Score = GetScore(currentPlayer);
                    break;
                case 6:
                    Debug.Log("Recieving data from player 7");
                    currentPlayerStats = player7Stats;
                    player7Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player7Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player7Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player7Stats.numWCBought = (currentPlayer.numWCBought);
                    player7Stats.valueWCBought = (currentPlayer.numWCBought);
                    player7Stats.numWCSold = (currentPlayer.numWCSold);
                    player7Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player7Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player7Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player7Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player7Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player7Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player7Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player7Stats.active = true;
                    player7Score = GetScore(currentPlayer);
                    break;
                case 7:
                    Debug.Log("Recieving data from player 8");
                    currentPlayerStats = player8Stats;
                    player8Stats.playerFreeTime = (100 - currentPlayer.playerFreeTime);
                    player8Stats.numWCOffers = (currentPlayer.numWCOffers);
                    player8Stats.valueWCOffers = (currentPlayer.valueWCOffers);
                    player8Stats.numWCBought = (currentPlayer.numWCBought);
                    player8Stats.valueWCBought = (currentPlayer.numWCBought);
                    player8Stats.numWCSold = (currentPlayer.numWCSold);
                    player8Stats.valueWCSold = (currentPlayer.valueWCSold);
                    player8Stats.numGoldOffers = (currentPlayer.numGoldOffers);
                    player8Stats.valueGoldOffers = (currentPlayer.valueGoldOffers);
                    player8Stats.numGoldBought = (currentPlayer.numGoldBought);
                    player8Stats.valueGoldBought = (currentPlayer.valueGoldBought);
                    player8Stats.numGoldSold = (currentPlayer.numGoldSold);
                    player8Stats.valueGoldSold = (currentPlayer.valueGoldSold);
                    player8Stats.active = true;
                    player8Score = GetScore(currentPlayer);
                    break;
            }

        }
        [TargetRpc]
        public void TargetRpcSetStatistics(NetworkConnection target)
        {
            //CmdSendStatistics();
        }

        private void ClearStatistics()
        {
            /*
            playerFreeTimeStat.Clear();
            numWCOffers.Clear();
            valueWCOffers.Clear();
            numWCBought.Clear();
            valueWCBought.Clear();
            numWCSold.Clear();
            valueWCSold.Clear();
            numGoldOffers.Clear();
            valueGoldOffers.Clear();
            numGoldBought.Clear();
            valueGoldBought.Clear();
            numGoldSold.Clear();
            valueGoldSold.Clear();
            */
        }

        public float GetScore(PlayerStatistics currentPlayer)
        {
            int playerFreeTime = currentPlayer.playerFreeTime;
            int modifiedPlayerFreeTime = playerFreeTime * 10;
            float playerScore = ((currentPlayer.numWCOffers + currentPlayer.numWCBought + currentPlayer.numWCSold) / (float)playerFreeTime) +
                ((currentPlayer.numGoldOffers + currentPlayer.numGoldBought + currentPlayer.numGoldSold) / (float)modifiedPlayerFreeTime);
            return playerScore;
        }
        
        public IEnumerator UpdateStatSingle()
        {            
            for (int i = 0; i < Level.instance.allPlayers.Count; i++)
            {
                if (!Level.instance.allPlayers[i].Mayor)
                {
                    //listSynced = 0;
                    TargetRpcSetStatistics(Level.instance.allPlayers[i].connectionToClient);
                    //Wait for the sync list to update
                    /*
                    while(listSynced < 13)
                    {
                        yield return null;
                    }*/
                }
            }
            yield return null;
        }

        //Checks to see if a list is synced up
        void OnSyncListUpdate()
        {
            listSynced++;
        }

        //Gets the stats of the players
        //(#WC Offers + #WC Buys + #WC Sales)/Free Time  +  (#Gold Offers + #Gold Buys + #Gold Sales)/Free Time/10 = Value
        public void GetStatistics()
        {
            //Show badge and text
            earnedText.gameObject.SetActive(true);
            earnedText.text = "";
            badgeImage.gameObject.SetActive(true);
            badgeText.gameObject.SetActive(true);

            var currentPlayer = Level.instance.authoritativePlayer;
            var gameOverPlayers = Level.instance.gameOverPlayers;
            //Determine which mayor badge to display if not null
            if (currentPlayer != null && currentPlayer.Mayor)
            {
                //Check the state of all buildings
                int buildingCount = 0;
                for(int i = 0; i < Level.instance.Buildings.Count; i++)
                {
                    if(Level.instance.Buildings[i].State == 1)
                    {
                        buildingCount++;
                    }
                }

                //Check Mayor of Light
                if(buildingCount == Level.instance.Buildings.Count && gameOverPlayers.Count <= 0)
                {
                    earnedText.text = KoboldTools.Localisation.instance.getLocalisedText("stats1");
                    badgeImage.sprite = Resources.Load<Sprite>("Badges/Mayor_of_Light");
                    badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats5");
                }
                //Check Mayor of Commons
                else if(buildingCount == Level.instance.Buildings.Count && gameOverPlayers.Count > 0)
                {
                    earnedText.text = KoboldTools.Localisation.instance.getLocalisedText("stats2");
                    badgeImage.sprite = Resources.Load<Sprite>("Badges/Mayor_of_Commons");
                    badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats6");
                }
                //Check Mayor of Compassion
                else if(buildingCount < Level.instance.Buildings.Count && gameOverPlayers.Count <= 0)
                {
                    earnedText.text = KoboldTools.Localisation.instance.getLocalisedText("stats3");
                    badgeImage.sprite = Resources.Load<Sprite>("Badges/Mayor_of_Compassion");
                    badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats7");
                }
                //Check Mayor Lonely Heart
                else if (buildingCount < Level.instance.Buildings.Count && gameOverPlayers.Count > 0)
                {
                    earnedText.text = KoboldTools.Localisation.instance.getLocalisedText("stats4");
                    badgeImage.sprite = Resources.Load<Sprite>("Badges/Mayor_Lonely_Heart");
                    badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats8");
                }
            }
            //Determine which badge to show player
            else
            {
                if(currentPlayer != null && currentPlayer.Job != null)
                {   
                    //Calculate player's Score
                    int playerFreeTime = 100 - currentPlayer.Job.TimeCost;
                    int modifiedPlayerFreeTime = playerFreeTime * 10;
                    float playerScore = ((currentPlayer.numWCOffers + currentPlayer.numWCBought + currentPlayer.numWCSold) / (float)playerFreeTime) +
                        ((currentPlayer.numGoldOffers + currentPlayer.numGoldBought + currentPlayer.numGoldSold) / (float)modifiedPlayerFreeTime);

                    List<float> allScores = new List<float>();
                    List <PlayerStatistics> allPlayerStats = new List<PlayerStatistics>();
                    if(player1Stats.active)
                        allPlayerStats.Add(player1Stats);
                    if (player2Stats.active)
                        allPlayerStats.Add(player2Stats);
                    if (player3Stats.active)
                        allPlayerStats.Add(player3Stats);
                    if (player4Stats.active)
                        allPlayerStats.Add(player4Stats);
                    if (player5Stats.active)
                        allPlayerStats.Add(player5Stats);
                    if (player6Stats.active)
                        allPlayerStats.Add(player6Stats);
                    if (player7Stats.active)
                        allPlayerStats.Add(player7Stats);
                    if (player8Stats.active)
                        allPlayerStats.Add(player8Stats);

                    for(int i = 0; i < allPlayerStats.Count; i++)
                    {
                        if(allPlayerStats[i].active)
                        {
                            int freeTime = 100 - allPlayerStats[i].playerFreeTime;

                            int modifiedFreeTime = 10 * freeTime;
                            float score = ((allPlayerStats[i].numWCOffers + allPlayerStats[i].numWCBought + allPlayerStats[i].numWCSold) / (float)freeTime) +
                                ((allPlayerStats[i].numGoldOffers + allPlayerStats[i].numGoldBought + allPlayerStats[i].numGoldSold) / (float)modifiedFreeTime);
                            allScores.Add(score);
                        }
                    }
                    //Fill in the list if there are too few members
                    if(allScores.Count < 5)
                    {
                        for(int i = allScores.Count; i < 5; i++)
                        {
                            allScores.Add(0);
                        }
                    }

                    int index = 0;
                    allScores.Sort();

                    for (int i = 0; i < allScores.Count; i++)
                    {
                        if (playerScore == allScores[i])
                        {
                            index = i;
                        }
                    }

                    //Check the state of all buildings
                    int buildingCount = 0;
                    for (int i = 0; i < Level.instance.Buildings.Count; i++)
                    {
                        if (Level.instance.Buildings[i].State == 1)
                        {
                            buildingCount++;
                        }
                    }

                    //Win
                    if (Level.instance.months >= Level.instance.maximumMonths)
                    {
                        switch (index)
                        {
                            default:
                                badgeText.text = "No Found";
                                break;
                            //Crystal Guardian
                            case 0:
                                badgeImage.sprite = Resources.Load<Sprite>("Badges/Citizen_Crystal_Guardian");
                                badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats9");
                                earnedText.gameObject.SetActive(true);
                                break;
                            //Community Builder
                            case 1:
                                badgeImage.sprite = Resources.Load<Sprite>("Badges/Citizen_Community_Builder");
                                badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats10");
                                earnedText.gameObject.SetActive(true);
                                break;
                            //Talent Shaper
                            case 2:
                                badgeImage.sprite = Resources.Load<Sprite>("Badges/Talent_Shaper");
                                badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats11");
                                earnedText.gameObject.SetActive(true);
                                break;
                            //Resource Explorer
                            case 3:
                                badgeImage.sprite = Resources.Load<Sprite>("Badges/Citizen_Resource_Explorer");
                                badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats12");
                                earnedText.gameObject.SetActive(true);
                                break;
                            //Trade Maker
                            case 4:
                                badgeImage.sprite = Resources.Load<Sprite>("Badges/Citizen_Trade_Maker");
                                badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats13");
                                earnedText.gameObject.SetActive(true);
                                break;
                        }
                    }
                    else
                    {
                        switch (index)
                        {
                            default:
                                badgeText.text = "No Found";
                                break;
                            //Talent Builder
                            case 0:
                                badgeImage.sprite = Resources.Load<Sprite>("Badges/Citizen_Talent_Shaper");
                                badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats14");
                                earnedText.gameObject.SetActive(true);
                                break;
                            //Community Shaper
                            case 1:
                                badgeImage.sprite = Resources.Load<Sprite>("Badges/Citizen_Community_Builder");
                                badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats15");
                                earnedText.gameObject.SetActive(true);
                                break;
                            //Resource Explorer
                            case 2:
                                badgeImage.sprite = Resources.Load<Sprite>("Badges/Citizen_Resource_Explorer");
                                badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats16");
                                earnedText.gameObject.SetActive(true);
                                break;
                            //Crystal Trader
                            case 3:
                                badgeImage.sprite = Resources.Load<Sprite>("Badges/Citizen_Trade_Maker");
                                badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats17");
                                earnedText.gameObject.SetActive(true);
                                break;
                            //Crystal Keeper
                            case 4:
                                badgeImage.sprite = Resources.Load<Sprite>("Badges/Citizen_Crystal_Guardian");
                                badgeText.text = KoboldTools.Localisation.instance.getLocalisedText("stats18");
                                earnedText.gameObject.SetActive(true);
                                break;
                        }

                    }

                }


            } 
        }
    }
}

