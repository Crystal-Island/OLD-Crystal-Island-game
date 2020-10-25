using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine;

namespace Polymoney
{
    public class Mayor_Panel : NetworkBehaviour
    {
        public Button hideButton;

        public Animator panelAnimator;
        public GameObject mayorPanel, panelFrame;

        public List<GameObject> panelList = new List<GameObject>();


        // Use this for initialization
        void Start()
        {            
            hideButton.onClick.AddListener(InteractMenu);



        }

        // Update is called once per frame
        void Update()
        {
            //Is the player the mayor?
            if (Level.instance != null && Level.instance.authoritativePlayer != null && Level.instance.authoritativePlayer.Mayor)
            {
                mayorPanel.SetActive(true);
                hideButton.gameObject.SetActive(true);
                //print("Is Mayor");

                for (int i = 0; i < Level.instance.allPlayers.Count; i++)
                {
                    if (panelList[i].activeInHierarchy && !Level.instance.allPlayers[i].Mayor)
                    {
                        //Initialize the panel
                        if (Level.instance.allPlayers[i].Person != null)
                        {
                            panelList[i].transform.Find("Player_Name_Text").GetComponent<Text>().text = GetName(Level.instance.allPlayers[i].Person.Title);
                        }
                        else
                        {
                            panelList[i].transform.Find("Player_Name_Text").GetComponent<Text>().text = Level.instance.allPlayers[i].name;
                        }

                        int playerBalance = 0;
                        Level.instance.allPlayers[i].Pocket.TryGetBalance(Currency.FIAT, out playerBalance);
                        panelList[i].transform.Find("Money_IMG").transform.Find("Player_Money_Text").GetComponent<Text>().text = playerBalance.ToString();
                        int playerCrystals = 0;
                        Level.instance.allPlayers[i].Pocket.TryGetBalance(Currency.Q, out playerCrystals);
                        panelList[i].transform.Find("Water_IMG").transform.Find("Player_Water_Text").GetComponent<Text>().text = playerCrystals.ToString();
                        //Clear the needs frame
                        foreach (Transform need in panelList[i].transform.Find("Player_Needs_Frame").transform)
                        {
                            Destroy(need.gameObject);
                        }
                        //Populate the needs frame
                        for (int j = 0; j < Level.instance.allPlayers[i].Incidents.Count; j++)
                        {
                            if (Level.instance.allPlayers[i].Incidents[j].Type == "Disaster")
                            {
                                string tag = Level.instance.allPlayers[i].Incidents[j].Tags[0];
                                GameObject needIcon = Instantiate(Resources.Load<GameObject>("Player_Need"), panelList[i].transform.Find("Player_Needs_Frame").transform);
                                needIcon.transform.Find("Need_IMG").GetComponent<Image>().sprite = Resources.Load<Sprite>(GetIcon(tag));
                            }
                        }
                        //Clear the talents frame
                        foreach (Transform talent in panelList[i].transform.Find("Player_Skills").transform)
                        {
                            Destroy(talent.gameObject);
                        }
                        //Populate the talent frame
                        for (int j = 0; j < Level.instance.allPlayers[i].Talents.Count; j++)
                        {
                            string tag = Level.instance.allPlayers[i].Talents[j].Tags[0];
                            GameObject talentIcon = Instantiate(Resources.Load<GameObject>("Player_Skill"), panelList[i].transform.Find("Player_Skills").transform);
                            talentIcon.transform.Find("Skill_IMG").GetComponent<Image>().sprite = Resources.Load<Sprite>(GetIcon(tag));
                        }

                    }
                }
            }
            else
            {
                mayorPanel.SetActive(false);
                hideButton.gameObject.SetActive(false);
                //print("Not Mayor");
            }


            
        }

        string GetName(string tag)
        {
            string name = "";
            switch(tag)
            {
                case "person0Name":
                    name = "Kalani";
                    break;
                case "person1Name":
                    name = "Nopala";
                    break;
                case "person2Name":
                    name = "Gaylen";
                    break;
                case "person3Name":
                    name = "Olafin";
                    break;
                case "person4Name":
                    name = "Indiram";
                    break;
                case "person5Name":
                    name = "Shandral";
                    break;
                case "person6Name":
                    name = "Hanovi";
                    break;
                case "person7Name":
                    name = "Mika";
                    break;
            }
            return name;
        }

        //Helper method to get the proper icons for needs
        string GetIcon(string tag)
        {
            string icon = "";
            switch(tag)
            {
                case "Fix":
                    icon = "Icons/pm_gui_icon_mechanic";
                    break;
                case "IT":
                    icon = "Icons/pm_gui_icon_nerd";
                    break;
                case "Nature":
                    icon = "Icons/pm_gui_icon_gardening";
                    break;
                case "Medical":
                    icon = "Icons/pm_gui_icon_medical";
                    break;
                case "Entertain":
                    icon = "Icons/pm_gui_icon_music";
                    break;
                case "Food":
                    icon = "Icons/pm_gui_icon_food";
                    break;
                case "Admin":
                    icon = "Icons/pm_gui_icon_admin";
                    break;
                case "Know":
                    icon = "Icons/pm_gui_icon_knowledge";
                    break;
            }
            return icon;
        }

        //Networking get information for all players
        [Server]
        public void CmdSendPlayers()
        {
            RpcGetPlayers();
        }
        [ClientRpc]
        public void RpcGetPlayers()
        {

        }


        public void InteractMenu()
        {
            if (mayorPanel.activeInHierarchy)
            {
                hideButton.interactable = false;
                StartCoroutine(HideMenu());
            }
            else
            {
                hideButton.interactable = false;
                StartCoroutine(ShowMenu());
            }
        }

        IEnumerator ShowMenu()
        {
            //Update the Panel
            UpdatePanel();
            panelAnimator.enabled = true;
            panelAnimator.Play("Open_Menu");
            yield return new WaitForSeconds(1f);
            hideButton.interactable = true;
        }

        IEnumerator HideMenu()
        {
            panelAnimator.Play("Close_Menu");
            yield return new WaitForSeconds(1f);
            hideButton.interactable = true;
        }

        void UpdatePanel()
        {
            panelAnimator.enabled = false;
            mayorPanel.SetActive(true);
            //Turn off all panels
            for(int i = 0; i < panelList.Count; i++)
            {
                panelList[i].SetActive(false);
            }

            //Turn on a panel for each player that is not the mayor
            for(int i = 0; i < Level.instance.allPlayers.Count; i++)
            {
                if(!Level.instance.allPlayers[i].Mayor)
                {
                    panelList[i].SetActive(true);                   
                }
            }
        }
    }
}

