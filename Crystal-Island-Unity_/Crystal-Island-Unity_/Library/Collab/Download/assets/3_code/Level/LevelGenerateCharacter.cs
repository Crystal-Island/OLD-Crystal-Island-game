using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    /// <summary>
    /// Generates unique characters for all players in a level when GameFlow enters the character generation state on teh server.
    /// </summary>
    public class LevelGenerateCharacter : NetworkBehaviour
    {
        /// <summary>
        /// Defines the number of unemployed players.
        /// </summary>
        public int numUnemployedPlayers = 1;
        /// <summary>
        /// Defines the number of city players / mayors.
        /// </summary>
        public int numMayors = 1;
        public MarketplaceSet marketDb = null;
        public Marketplace complementaryMarket = null;
        public string marketplaceCityTitleTextId = "marketplaceCityTitle";
        public string marketplaceCitySubtitleTextId = "marketplaceCitySubtitle";
        public string marketplaceCitizenSubtitleTextId = "marketplaceCitizenSubtitle";

        /// <summary>
        /// Holds a reference to the <see cref="Level"/> instance.
        /// </summary>
        private Level level = null;

        /// <summary>
        /// Wait for the level data to appear, then generate the players. This
        /// will also assign mayor status.
        /// </summary>
        public IEnumerator Start()
        {
            // Only generate characters on the server.
            if (!this.isServer)
            {
                yield break;
            }

            // Obtain a reference to the level.
            this.level = this.GetComponentInChildren<Level>();

            // Wait for all players to load on all clients before generating the characters.
            this.level.onAllPlayersReady.AddListener(this.RunCharacterGeneration);
        }

        private void RunCharacterGeneration()
        {
            RootLogger.Info(this, "Server: Running character generation");

            int numPlayers = this.level.allPlayers.Count;
            int numRegularPlayers = numPlayers - this.numMayors;
            int[] numPht = new [] {
                this.level.levelData.Persons.Count,
                this.level.levelData.Homes.Count,
                this.level.levelData.Talents.Count
            };
            int numUnemployed = this.level.levelData.Unemployed.Count;
            int numJobs = this.level.levelData.Jobs.Count;

            // Randomize the persons, homes and jobs.
            this.ShuffleLevelData();

            // Set all player's names.
            foreach (Player player in this.level.allPlayers)
            {
                player.ServerSetName(player.name);
            }

            // Ensure that every player (except mayor) may receive a character description, home, talent, and job (or be unemployed).
            if (numPlayers == 1 && numPlayers <= numJobs && numPlayers <= numPht.Min())
            {
                this.GenerateSinglePlayerGame();
            }
            else if (numRegularPlayers >= this.numUnemployedPlayers && numRegularPlayers <= numPht.Min() && (numRegularPlayers - this.numUnemployedPlayers) <= numJobs && this.numUnemployedPlayers <= numUnemployed)
            {
                this.GenerateRegularGame();
            }
            else
            {
                RootLogger.Exception(this, "There are more players than available data for character generation.");
            }

            // Finally, tell all clients that their players have finished generating.
            foreach (Player player in this.level.allPlayers)
            {
                player.ServerFinalizePlayerGeneration();
            }
        }

        private void ShuffleLevelData()
        {
            LevelData data = this.level.levelData;
            data.Persons.Shuffle();
            data.MayorPersons.Shuffle();
            data.Homes.Shuffle();
            data.MayorHomes.Shuffle();
            data.Unemployed.Shuffle();
            data.Jobs.Shuffle();
            data.MayorJobs.Shuffle();
        }

        private void CreateMarketplace(Player player, string title, string description)
        {
            // setup marketplace and use syncprovider of the marketplace set to rpc creation over the network
            Marketplace newMarketplace = ScriptableObject.CreateInstance<Marketplace>();
            newMarketplace.init(title, description);

            string marketplaceGuid = Guid.NewGuid().ToString();
            this.marketDb.syncProvider.AddMarketplace(JsonUtility.ToJson(newMarketplace), marketplaceGuid);
            this.marketDb.syncProvider.SetMarketSeller(marketplaceGuid, player.GetComponent<NetworkIdentity>());
            player.ServerSetOwnMarketplace(marketplaceGuid);
            Destroy(newMarketplace);
        }

        private void GenerateSinglePlayerGame()
        {
            LevelData data = this.level.levelData;
            Player player = this.level.allPlayers[0];
            if (player.RunsForMayor)
            {
                player.ServerSetMayorStatus(true);
                player.ServerSetPerson(data.MayorPersons[0]);
                player.ServerSetHome(data.MayorHomes[0]);
                player.ServerSetJob(data.MayorJobs[0]);
                player.ServerAddTalent(data.MayorTalents[player.Person.TalentId]);
                Pocket pocket = new Pocket();
                pocket.SetBalance(Currency.FIAT, this.level.MayorBaseStartingMoney);
                player.ServerSetPocket(pocket);
                this.CreateMarketplace(player, Localisation.instance.getLocalisedText(this.marketplaceCityTitleTextId), Localisation.instance.getLocalisedFormat(this.marketplaceCitySubtitleTextId, player.name));
                this.marketDb.syncProvider.SetMarketSeller(this.complementaryMarket.guid.ToString(), player.GetComponent<NetworkIdentity>());
            }
            else
            {
                Person person = data.Persons[0];
                Home home = data.Homes[0];
                Job job = data.Jobs[0];
                Talent talent = data.Talents[person.TalentId];
                Pocket pocket = new Pocket();
                pocket.SetBalance(Currency.FIAT, this.level.RegularStartingMoney);

                player.ServerSetPocket(pocket);
                player.ServerSetPerson(person);
                player.ServerSetHome(home);
                player.ServerSetJob(job);
                player.ServerAddTalent(talent);
                this.CreateMarketplace(player, person.Title, Localisation.instance.getLocalisedFormat(this.marketplaceCitizenSubtitleTextId, player.name));
            }

        }

        private void GenerateRegularGame()
        {
            // Select the mayor or city player by first finding all candidates,
            // then randomly selecting the defined number of mayors (usually
            // only one). If nobody wanted to be the mayor, select the players
            // at random from the whole set.
            LevelData data = this.level.levelData;
            List<Player> mayors = this.DetermineMayors(this.level.allPlayers);
            List<Player> regularPlayers = this.level.allPlayers.FindAll(p => !mayors.Contains(p));
            List<Player> unemployedPlayers = regularPlayers.SelectRandom(this.numUnemployedPlayers);
            List<Player> employedPlayers = regularPlayers.FindAll(p => !unemployedPlayers.Contains(p)).ToList();
            List<Home> cheapHomes = data.Homes.OrderBy(e => e.Rent).Take(unemployedPlayers.Count).ToList();
            List<Home> otherHomes = data.Homes.FindAll(e => !cheapHomes.Contains(e)).ToList();
            RootLogger.Info(this, "Server: We have {0} mayor(s), {1} unemployed, and {2} employed persons.", mayors.Count, unemployedPlayers.Count, employedPlayers.Count);

            // Assign data to the mayors.
            {
                int i = 0;
                foreach (Player player in mayors)
                {
                    // Set the mayor flag.
                    player.ServerSetMayorStatus(true);

                    // Assign person, home, job and talent to the mayor.
                    player.ServerSetPerson(data.MayorPersons[i]);
                    player.ServerSetHome(data.MayorHomes[i]);
                    player.ServerSetJob(data.MayorJobs[i]);
                    player.ServerAddTalent(data.MayorTalents[player.Person.TalentId]);

                    // Assign the starting money.
                    Pocket pocket = new Pocket();
                    pocket.SetBalance(Currency.FIAT, Mathf.FloorToInt(this.level.MayorBaseStartingMoney * regularPlayers.Count * this.level.MayorStartingMoneyFactor));
                    player.ServerSetPocket(pocket);

                    // Create their own personal marketplace.
                    this.CreateMarketplace(player, Localisation.instance.getLocalisedText(this.marketplaceCityTitleTextId), Localisation.instance.getLocalisedFormat(this.marketplaceCitySubtitleTextId, player.name));

                    // Assign ownership over the complementary introduction marketplace.
                    this.marketDb.syncProvider.SetMarketSeller(this.complementaryMarket.guid.ToString(), player.GetComponent<NetworkIdentity>());

                    i += 1;
                }
            }

            // Assign the person to each player.
            {
                int i = 0;
                foreach (Player player in regularPlayers)
                {
                    player.ServerSetPerson(data.Persons[i]);
                    i += 1;
                }
            }

            // Assign homes to unemployed players.
            {
                int i = 0;
                foreach (Player player in unemployedPlayers)
                {
                    player.ServerSetHome(cheapHomes[i]);
                    i += 1;
                }
            }

            // Assign homes to employed players.
            {
                int i = 0;
                foreach (Player player in employedPlayers)
                {
                    player.ServerSetHome(otherHomes[i]);
                    i += 1;
                }
            }

            // Assign jobs to unemployed players.
            {
                int i = 0;
                foreach (Player player in unemployedPlayers)
                {
                    player.ServerSetJob(data.Unemployed[i]);
                    i += 1;
                }
            }

            // Assign jobs to all other players.
            {
                int i = 0;
                foreach (Player player in employedPlayers)
                {
                    player.ServerSetJob(data.Jobs[i]);
                    i += 1;
                }
            }

            // Assign the talent to each player.
            foreach (Player player in regularPlayers)
            {
                player.ServerAddTalent(data.Talents[player.Person.TalentId]);
            }

            // Assign the starting money to each player.
            foreach (Player player in regularPlayers)
            {
                Pocket pocket = new Pocket();
                pocket.SetBalance(Currency.FIAT, this.level.RegularStartingMoney);
                player.ServerSetPocket(pocket);
            }

            // Create a marketplace for each player.
            foreach (Player player in regularPlayers)
            {
                this.CreateMarketplace(player, player.Person.Title, Localisation.instance.getLocalisedFormat(this.marketplaceCitizenSubtitleTextId, player.name));
            }

            // Make sure that for every game, at least one player has the food talent.
            Talent foodTalent = data.Talents.SingleOrDefault(e => e.EquivalentTags(this.level.foodTags));
            if (foodTalent != null)
            {
                Player foodRecipient = regularPlayers.FindAll(e => e.Job.TimeCost < 100).SelectRandom(1).SingleOrDefault();
                if (foodRecipient.Talents.Count(e => e.EquivalentTags(foodTalent.Tags)) == 0)
                {
                    foodRecipient.ServerAddTalent(foodTalent);
                }
            }
        }

        private List<Player> DetermineMayors(List<Player> players)
        {
            List<Player> mayorCandidates = players.FindAll(p => p.RunsForMayor).ToList();
            if (mayorCandidates.Count == 0)
            {
                mayorCandidates = players;
            }

            return mayorCandidates.SelectRandom(this.numMayors);
        }
    }
}
