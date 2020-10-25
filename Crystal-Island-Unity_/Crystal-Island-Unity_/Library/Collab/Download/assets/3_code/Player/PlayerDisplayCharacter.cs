using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KoboldTools;
using KoboldTools.Logging;

namespace Polymoney
{
    public class PlayerDisplayCharacter : VCBehaviour<Player>
    {
        private enum State
        {
            PERSON = 1,
            HOME = 2,
            JOB = 3,
            TALENTS = 4,
        }

        [Header("Direct Access Buttons")]
        public Button PersonButton;
        public Button HomeButton;
        public Button JobButton;
        public Button TalentsButton;

        [Header("Navigation Buttons")]
        public Button PreviousButton;
        public Button NextButton;
        public Button ScrollButton;
        public Button CloseButton;

        [Header("Data Panels")]
        public Panel PersonPanel;
        public Panel HomePanel;
        public Panel JobPanel;
        public Panel TalentsPanel;

        [Header("Other")]
        public Text HeaderText;
        public string PersonHeaderTextId = "youAre";
        public string HomeHeaderTextId = "youLive";
        public string JobHeaderTextId = "youWork";
        public string TalentsHeaderTextId = "yourTalent";
        public Transform TalentTemplate;

        private State CurrentState;
        private Pool<Transform> TalentPool;

        public void Awake()
        {
            this.TalentPool = new Pool<Transform>(this.TalentTemplate);
        }

        public override void onModelChanged()
        {
            this.CurrentState = State.PERSON;
            this.model.PlayerStateChanged.AddListener(this.onPlayerStateChanged);
            this.onPlayerStateChanged();
            this.PersonButton.onClick.AddListener(this.onClickPersonButton);
            this.HomeButton.onClick.AddListener(this.onClickHomeButton);
            this.JobButton.onClick.AddListener(this.onClickJobButton);
            this.TalentsButton.onClick.AddListener(this.onClickTalentsButton);
            this.PreviousButton.onClick.AddListener(this.onClickPreviousButton);
            this.NextButton.onClick.AddListener(this.onClickNextButton);
        }

        public override void onModelRemoved()
        {
            this.model.PlayerStateChanged.RemoveListener(this.onPlayerStateChanged);
            this.PersonButton.onClick.RemoveListener(this.onClickPersonButton);
            this.HomeButton.onClick.RemoveListener(this.onClickHomeButton);
            this.JobButton.onClick.RemoveListener(this.onClickJobButton);
            this.TalentsButton.onClick.RemoveListener(this.onClickTalentsButton);
            this.PreviousButton.onClick.RemoveListener(this.onClickPreviousButton);
            this.NextButton.onClick.RemoveListener(this.onClickNextButton);
        }

        private void onPlayerStateChanged()
        {
            // Update the model data for all panels.
            VC<Person>.addModelToAllControllers(this.model.Person, this.PersonPanel.gameObject);
            VC<Home>.addModelToAllControllers(this.model.Home, this.HomePanel.gameObject);
            VC<Job>.addModelToAllControllers(this.model.Job, this.JobPanel.gameObject);
            this.TalentPool.releaseAll();
            foreach (Talent talent in this.model.Talents)
            {
                Transform talentUi = this.TalentPool.pop();
                talentUi.gameObject.SetActive(true);
                VC<Talent>.addModelToAllControllers(talent, talentUi.gameObject);
            }
            this.showPanels();
        }

        private void onClickPersonButton()
        {
            this.CurrentState = State.PERSON;
            this.showPanels();
        }

        private void onClickHomeButton()
        {
            this.CurrentState = State.HOME;
            this.showPanels();
        }

        private void onClickJobButton()
        {
            this.CurrentState = State.JOB;
            this.showPanels();
        }

        private void onClickTalentsButton()
        {
            this.CurrentState = State.TALENTS;
            this.showPanels();
        }

        private void onClickPreviousButton()
        {
            if (this.CurrentState > State.PERSON)
            {
                this.CurrentState -= 1;
                this.showPanels();
            }
        }

        private void onClickNextButton()
        {
            if (this.CurrentState < State.TALENTS)
            {
                this.CurrentState += 1;
                this.showPanels();
            }
        }

        private void showPanels()
        {
            // Display the selected panel.
            switch (this.CurrentState)
            {
                case State.PERSON:
                    this.PersonButton.Select();
                    if (this.ScrollButton != null)
                    {
                        this.ScrollButton.gameObject.SetActive(false);
                    }
                    this.PreviousButton.interactable = false;
                    this.NextButton.interactable = true;
                    this.PersonPanel.onOpen();
                    this.HeaderText.text = Localisation.instance.getLocalisedText(this.PersonHeaderTextId);
                    break;
                case State.HOME:
                    this.HomeButton.Select();
                    if (this.ScrollButton != null)
                    {
                        this.ScrollButton.gameObject.SetActive(false);
                    }
                    this.PreviousButton.interactable = true;
                    this.NextButton.interactable = true;
                    this.HomePanel.onOpen();
                    this.HeaderText.text = Localisation.instance.getLocalisedText(this.HomeHeaderTextId);
                    break;
                case State.JOB:
                    this.JobButton.Select();
                    if (this.ScrollButton != null)
                    {
                        this.ScrollButton.gameObject.SetActive(false);
                    }
                    this.PreviousButton.interactable = true;
                    this.NextButton.interactable = true;
                    this.JobPanel.onOpen();
                    this.HeaderText.text = Localisation.instance.getLocalisedText(this.JobHeaderTextId);
                    break;
                case State.TALENTS:
                    this.TalentsButton.Select();
                    if (this.ScrollButton != null)
                    {
                        this.ScrollButton.gameObject.SetActive(true);
                    }
                    this.PreviousButton.interactable = true;
                    this.NextButton.interactable = false;
                    this.TalentsPanel.onOpen();
                    this.HeaderText.text = Localisation.instance.getLocalisedText(this.TalentsHeaderTextId);
                    break;
            }
        }
    }
}
