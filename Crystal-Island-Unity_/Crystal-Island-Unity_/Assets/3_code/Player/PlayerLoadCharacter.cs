using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    /// <summary>
    /// viewcontroller that loads and instantiates the player characacter on the map
    /// </summary>
    public class PlayerLoadCharacter : VCBehaviour<Player>
    {
        public GameObject characterPrefab = null;
        public GameObject cityCharacterPrefab = null;
        public Material[] lizardBodyMaterials;
        public Material[] lizardCrystalMaterials;
        private GameObject character = null;

        public override void onModelChanged()
        {
            this.model.OnPlayerGenerated.AddListener(this.onPlayerGenerated);
            this.model.OnGameOver.AddListener(this.onGameOver);
        }

        public override void onModelRemoved()
        {
            this.model.OnPlayerGenerated.RemoveListener(this.onPlayerGenerated);
            this.model.OnGameOver.RemoveListener(this.onGameOver);
            Destroy(this.character);
        }

        private void onPlayerGenerated()
        {
            RootLogger.Info(this, "Loading the character model for player '{0}' (netid: {1}, mayor: {2}).", this.model.name, this.model.netId, this.model.Mayor);
            if (this.model.Mayor)
            {
                this.character = Instantiate(this.cityCharacterPrefab, this.model.transform);
                this.character.transform.localPosition = Vector3.zero;
            }
            else
            {
                if (this.lizardBodyMaterials.Length == this.lizardCrystalMaterials.Length)
                {
                    this.character = Instantiate(this.characterPrefab, this.model.transform);
                    this.character.transform.localPosition = Vector3.zero;
                    SkinnedMeshRenderer renderer = this.character.GetComponentInChildren<SkinnedMeshRenderer>();

                    List<Player> sortedPlayerList = Level.instance.allPlayers.OrderBy(e => e.netId.Value).ToList();
                    long playerId = sortedPlayerList.FindIndex(e => e.Equals(this.model));

                    int matIdx = Array.FindIndex(renderer.materials, e => e.name.Contains("lizard_crystals"));
                    renderer.materials[matIdx].CopyPropertiesFromMaterial(this.lizardCrystalMaterials[playerId]);
                    matIdx = Array.FindIndex(renderer.materials, e => e.name.Contains("lizard_body"));
                    renderer.materials[matIdx].CopyPropertiesFromMaterial(this.lizardBodyMaterials[playerId]);
                }
                else
                {
                    RootLogger.Exception(this, "There must be an equal number of lizard crystal and body materials.");
                }
            }
            VC<Player>.addModelToAllControllers(model, this.character, true);
            this.model.LoadedCharacter = this.character.GetComponent<Character>();
        }

        private void onGameOver()
        {
            this.model.LoadedCharacter = null;
            Destroy(this.character);
        }
    }
}
