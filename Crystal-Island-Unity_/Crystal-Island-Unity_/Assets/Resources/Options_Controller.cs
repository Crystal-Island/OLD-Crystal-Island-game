using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine;
using System.Net;
using System.Net.Sockets;

public class Options_Controller : NetworkBehaviour
{
    NetworkIdentity m_Identity;
    public Button preset1, preset2, preset3;
    public Toggle preset1Toggle, preset2Toggle, preset3Toggle;
    public static bool presetFlag1, presetFlag2, presetFlag3;
    //Preset 1 (Personal Finance)=================================
    private bool preset1FlatTax = false;
    private bool preset1ProgTax = true;
    private int preset1ProgTaxUpper = 30;
    private int preset1BaseTaxRate = 20;
    private int preset1BaseTaxAmount = 100;
    private int preset1MaintCost = 50;
    private int preset1CrystalIncome = 10;
    private bool preset1WaterManualIntro = true;
    private int preset1WaterTurnIntro = 4;
    private float preset1DisasterSev = .5f;
    private int preset1DisasterFreq = 40;
    //Preset 2 (Community Finance)================================
    private bool preset2FlatTax = true;
    private bool preset2ProgTax = false;
    private int preset2BaseTaxRate = 0;
    private int preset2BaseTaxAmount = 100;
    private int preset2MaintCost = 70;
    private int preset2CrystalIncome = 10;
    private bool preset2WaterManualIntro = true;
    private int preset2WaterTurnIntro = 5;
    private float preset2DisasterSev = .5f;
    private int preset2DisasterFreq = 40;
    //Preset 3 ===================================================
    private bool preset3FlatTax = true;
    private bool preset3ProgTax = false;
    private int preset3BaseTaxRate = 0;
    private int preset3BaseTaxAmount = 150;
    private int preset3MaintCost = 60;
    private int preset3CrystalIncome = 10;
    private bool preset3WaterManualIntro = true;
    private int preset3WaterTurnIntro = 3;
    private float preset3DisasterSev = .5f;
    private int preset3DisasterFreq = 40;

    //Options Menu settings=======================================

    //Is the flat tax option enabled?
    public static bool flatTax = true;
    public static bool progTax = false;
    //The upper limit for the progressive tax
    public static int progressiveTaxUpper = 10;
    //The base tax percentage
    public static int baseTaxRate = 10;
    //The base tax amount
    public static int baseTaxAmount = 100;

    //The base maintenance cost
    public static int baseMaintCost = 50;

    //The base income for water crystals
    public static int baseWaterIncome = 10;
    public static bool manualIntroWater = true;
    public static int waterIntroTurn = 3;

    //Severity of the disaster low .5, med 1, high 2
    public static float severityFactor = 1;
    //Frequency of the disaster, default , low 10%, med 25%, high 40%
    public static int frequencyFactor = 0;
    //Chance to get a player match for disasters
    public static int disasterMatchChance = 80;

    //Show/Hide panel for all players
    public static bool showPanel = false;

    //============================================================


    //Options menu UI elements ===================================
    public GameObject optionsPanel;
    public Button closeButton, openButton;

    //Tax UI options
    public Toggle flatToggle;
    public Button flatEnable;
    public Toggle progToggle;
    public Button progEnable;
    public Slider progSlider;
    public Text progText, progMinText;
    public InputField baseTaxField;
    public InputField baseTaxAmountField;

    //Maintanence UI options
    public InputField baseMaintField;

    //Water UI options
    public InputField waterIncomeField;
    public Toggle waterManualIntro;
    public Button waterEnable;
    public InputField waterTurn;

    //Disater options
    public Dropdown disasterSev;
    public Dropdown disasterFreq;
    public InputField disasterChance;

    //Mayor Panel toggle
    public Toggle panelToggle;
    public Button panelButton;

    //============================================================
    

    // Use this for initialization
    void Start ()
    {
        openButton.onClick.AddListener(OpenPanel);
        closeButton.onClick.AddListener(ClosePanel);

        
        flatEnable.onClick.AddListener(EnableFlatTax);
        progEnable.onClick.AddListener(EnableProgTax);

        progSlider.onValueChanged.AddListener(delegate { UpdateSliderText(); });
        baseTaxField.onEndEdit.AddListener(delegate { UpdateBaseTaxRate(); });
        baseTaxAmountField.onEndEdit.AddListener(delegate { UpdateBaseTaxAmount(); });

        baseMaintField.onEndEdit.AddListener(delegate { UpdateBastMaintCost(); });

        waterIncomeField.onEndEdit.AddListener(delegate { UpdateWaterIncome(); });
        waterEnable.onClick.AddListener(EnableManualWaterIntro);
        waterTurn.onEndEdit.AddListener(delegate { UpdateIntroTurn(); });

        disasterSev.onValueChanged.AddListener(delegate { UpdateDisaterSeverity(); });
        disasterFreq.onValueChanged.AddListener(delegate { UpdateDisasterFreq(); });
        disasterChance.onValueChanged.AddListener(delegate { UpdateDisaterChance(); });

        panelButton.onClick.AddListener(UpdateShowPanel);

        InitializeSettings();

        preset1.onClick.AddListener(Preset1Enabled);
        preset2.onClick.AddListener(Preset2Enabled);
        preset3.onClick.AddListener(Preset3Enabled);

        ClosePanel();

    }
	
	// Update is called once per frame
	void Update ()
    {
        if (presetFlag1)
        {
            preset1Toggle.isOn = true;
            preset2Toggle.isOn = false;
            preset3Toggle.isOn = false;
        }
        else if (presetFlag2)
        {
            preset1Toggle.isOn = false;
            preset2Toggle.isOn = true;
            preset3Toggle.isOn = false;
        }
        if (presetFlag3)
        {
            preset1Toggle.isOn = false;
            preset2Toggle.isOn = false;
            preset3Toggle.isOn = true;
        }

        //Only display open button for server player
        if(!openButton.gameObject.activeInHierarchy)
        {
            if (GameObject.Find("NetworkManager").GetComponent<NetworkDiscovery>().isServer)
            {
                openButton.gameObject.SetActive(true);
            }
            else
            {
                openButton.gameObject.SetActive(false);
            }
        }
    }


    //Broadcasts settings to all the clients
    [Command]
    public void CmdSentSettings()
    {
        RpcUpdateSettings();
    }

    //Call this method when the menu is closed
    [ClientRpc]
    public void RpcUpdateSettings()
    {
        optionsPanel.SetActive(true);
        //Tax Options
        if (flatToggle.isOn)
        {
            flatTax = true;
            progTax = false;
        }
        else
        {
            flatTax = false;
            progTax = true;
        }
        UpdateSliderText();
        UpdateBaseTaxRate();
        UpdateBaseTaxAmount();

        //Maint Options
        UpdateBastMaintCost();

        //Water Crystals Options
        UpdateWaterIncome();
        UpdateIntroTurn();

        UpdateShowPanel();
        if (panelToggle.isOn)
        {
            showPanel = true;
        }
        else
        {
            showPanel = false;
        }

        UpdateDisaterChance();

        optionsPanel.SetActive(false);
    }

    [ClientRpc]
    public void RpcSetPanel(bool panel)
    {
        showPanel = panel;
    }
    
    [ClientRpc]
    public void RpcApplyPreset1()
    {
        flatTax = preset1FlatTax;
        progTax = preset1ProgTax;
        progressiveTaxUpper = preset1ProgTaxUpper;
        baseTaxRate = preset1BaseTaxRate;
        baseTaxAmount = preset1BaseTaxAmount;
        baseMaintCost = preset1MaintCost;
        baseWaterIncome = preset1CrystalIncome;
        manualIntroWater = preset1WaterManualIntro;
        waterIntroTurn = preset1WaterTurnIntro;
        severityFactor = preset1DisasterSev;
        frequencyFactor = preset1DisasterFreq;
    }

    [ClientRpc]
    public void RpcApplyPreset2()
    {
        flatTax = preset2FlatTax;
        progTax = preset2ProgTax;
        baseTaxRate = preset2BaseTaxRate;
        baseTaxAmount = preset2BaseTaxAmount;
        baseMaintCost = preset2MaintCost;
        baseWaterIncome = preset2CrystalIncome;
        manualIntroWater = preset2WaterManualIntro;
        waterIntroTurn = preset2WaterTurnIntro;
        severityFactor = preset2DisasterSev;
        frequencyFactor = preset2DisasterFreq;
    }

    [ClientRpc]
    public void RpcApplyPreset3()
    {
        flatTax = preset3FlatTax;
        progTax = preset3ProgTax;
        baseTaxRate = preset3BaseTaxRate;
        baseTaxAmount = preset3BaseTaxAmount;
        baseMaintCost = preset3MaintCost;
        baseWaterIncome = preset3CrystalIncome;
        manualIntroWater = preset3WaterManualIntro;
        waterIntroTurn = preset3WaterTurnIntro;
        severityFactor = preset3DisasterSev;
        frequencyFactor = preset3DisasterFreq;
    }

    private void InitializeSettings()
    {
        //Update the tax settings
        if(flatTax)
        {
            flatToggle.isOn = true;
            progToggle.isOn = false;
            progSlider.enabled = false;
        }
        else if(progTax)
        {
            flatToggle.isOn = false;
            progToggle.isOn = true;
            progSlider.enabled = true;

            progSlider.value = progressiveTaxUpper;
            progText.text = progressiveTaxUpper.ToString() + "%";
        }
        progMinText.text = baseTaxRate.ToString() + "%";

        baseTaxField.text = baseTaxRate.ToString();
        baseTaxAmountField.text = baseTaxAmount.ToString();

        //Update maintenance settings
        baseMaintField.text = baseMaintCost.ToString();

        //Update Water Crystal settings
        waterIncomeField.text = baseWaterIncome.ToString();

        if(manualIntroWater)
        {
            waterManualIntro.isOn = true;

            waterIncomeField.enabled = true;
            waterTurn.text = waterIntroTurn.ToString();
        }
        else
        {
            waterManualIntro.isOn = false;

            waterIncomeField.enabled = false;
        }

        //Update the disaster chance text
        disasterChance.text = disasterMatchChance.ToString();

    }

    //Enables Flat Tax
    private void EnableFlatTax()
    {
        flatTax = true;
        progTax = false;
        InitializeSettings();
    }

    //Enables Progressive Tax
    private void EnableProgTax()
    {
        flatTax = false;
        progTax = true;
        InitializeSettings();
    }

    //Updates the progressive tax slider text
    private void UpdateSliderText()
    {
        progressiveTaxUpper = (int)progSlider.value;
        progText.text = progressiveTaxUpper.ToString() + "%";
    }

    //Updates the base tax rate
    private void UpdateBaseTaxRate()
    {
        baseTaxRate = int.Parse(baseTaxField.text);
        progMinText.text = baseTaxRate.ToString() + "%";
        progSlider.minValue = baseTaxRate;
    }

    //Udates the base tax amount
    private void UpdateBaseTaxAmount()
    {
        baseTaxAmount = int.Parse(baseTaxAmountField.text);
    }

    //Updates the base maintenance cost
    private void UpdateBastMaintCost()
    {
        baseMaintCost = int.Parse(baseMaintField.text);
    }

    //Updates the base income for water crystals
    private void UpdateWaterIncome()
    {
        baseWaterIncome = int.Parse(waterIncomeField.text);
    }

    //Is manual water crystals enabled?
    private void EnableManualWaterIntro()
    {
        manualIntroWater = true;
        InitializeSettings();
    }

    //Updates the water crystal introduction turn
    private void UpdateIntroTurn()
    {
        waterIntroTurn = int.Parse(waterTurn.text);
        RpcUpdateWaterCrystal(int.Parse(waterTurn.text));
    }

    [ClientRpc]
    public void RpcUpdateWaterCrystal(int turn)
    {
        Debug.Log("Setting manual turn to " + turn);
        waterIntroTurn = turn;
    }

    //Updates the disaster severity
    private void UpdateDisaterSeverity()
    {
        switch(disasterSev.value)
        {
            //Low
            case 0:
                severityFactor = .5f;
                break;
            case 1:
                severityFactor = 1f;
                break;
            case 2:
                severityFactor = 2f;
                break;
        }
    }

    //Updates the disaster frequency
    private void UpdateDisasterFreq()
    {
        switch (disasterSev.value)
        {
            //default
            case 0:
                frequencyFactor = 0;
                break;
            case 1:
                frequencyFactor = 10;
                break;
            case 2:
                frequencyFactor = 25;
                break;
            case 3:
                frequencyFactor = 40;
                break;
        }
    }

    private void UpdateShowPanel()
    {
        if(showPanel)
        {
            showPanel = false;
            panelToggle.isOn = false;
        }
        else
        {
            showPanel = true;
            panelToggle.isOn = true;
        }
        RpcSetPanel(showPanel);
    }

    private void UpdateDisaterChance()
    {
        disasterMatchChance = int.Parse(disasterChance.text);
    }

    //Opens the option menu
    private void OpenPanel()
    {
        //Check if the player clicking is the host
        if(GameObject.Find("NetworkManager").GetComponent<NetworkDiscovery>().isServer)
        {
            optionsPanel.SetActive(true);
        }          
    }

    //Close the Option menu
    private void ClosePanel()
    {
        
        if (m_Identity.connectionToClient.connectionId > 1)
        {
            //Send the settings to the clients
            RpcUpdateSettings();

            //Send the panel info to clients
            RpcSetPanel(showPanel);

            //Send the manual intro info to clients

        }

        optionsPanel.SetActive(false);
    }

    //Apply preset 1 settings
    private void Preset1Enabled()
    {
        flatTax = preset1FlatTax;
        progTax = preset1ProgTax;
        progressiveTaxUpper = preset1ProgTaxUpper;
        baseTaxRate = preset1BaseTaxRate;
        baseTaxAmount = preset1BaseTaxAmount;
        baseMaintCost = preset1MaintCost;
        baseWaterIncome = preset1CrystalIncome;
        manualIntroWater = preset1WaterManualIntro;
        waterIntroTurn = preset1WaterTurnIntro;
        severityFactor = preset1DisasterSev;
        frequencyFactor = preset1DisasterFreq;
        presetFlag1 = true;
        presetFlag2 = false;
        presetFlag3 = false;
        if (m_Identity.connectionToClient.connectionId > 1)
        {
            RpcApplyPreset1();
        }
        InitializeSettings();
    }

    private void Preset2Enabled()
    {
        flatTax = preset2FlatTax;
        progTax = preset2ProgTax;
        baseTaxRate = preset2BaseTaxRate;
        baseTaxAmount = preset2BaseTaxAmount;
        baseMaintCost = preset2MaintCost;
        baseWaterIncome = preset2CrystalIncome;
        manualIntroWater = preset2WaterManualIntro;
        waterIntroTurn = preset2WaterTurnIntro;
        severityFactor = preset2DisasterSev;
        frequencyFactor = preset2DisasterFreq;
        presetFlag2 = true;
        presetFlag1 = false;
        presetFlag3 = false;
        if (m_Identity.connectionToClient.connectionId > 1)
        {
            RpcApplyPreset2();
        }
        InitializeSettings();
    }

    private void Preset3Enabled()
    {
        flatTax = preset3FlatTax;
        progTax = preset3ProgTax;
        baseTaxRate = preset3BaseTaxRate;
        baseTaxAmount = preset3BaseTaxAmount;
        baseMaintCost = preset3MaintCost;
        baseWaterIncome = preset3CrystalIncome;
        manualIntroWater = preset3WaterManualIntro;
        waterIntroTurn = preset3WaterTurnIntro;
        severityFactor = preset3DisasterSev;
        frequencyFactor = preset3DisasterFreq;
        presetFlag3 = true;
        presetFlag1 = false;
        presetFlag2 = false;
        if (m_Identity.connectionToClient.connectionId > 1)
        {
            RpcApplyPreset3();
        }
        InitializeSettings();
    }
}
