using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using Nethereum.Hex.HexTypes;
using Nethereum.Unity.Metamask;
using Nethereum.Metamask;
using TMPro;

public class WalletConnector : MonoBehaviour
{
    [Header("Wallet UI Elements")]
    public Button metamaskConnectButton;
    public TMP_Text accountSelectedLabel;  // Displays the connected account
    public TMP_Text errorLabel;            // Displays error messages
    public TMP_Text walletAddressTMPText;  // Also displays the connected account

    [Header("Panels")]
    public GameObject connectPanel;
    public GameObject successPanel;

    // Internal state variables
    private string selectedAccountAddress;
    public bool isMetamaskInitialised = false;
    public BigInteger currentChainId = 1114;

    private WalletStore walletStore;

    void Awake()
    {
        walletStore = FindAnyObjectByType<WalletStore>();
    }

    void Start()
    {
        if (metamaskConnectButton != null)
            metamaskConnectButton.onClick.AddListener(() => ConnectWallet());
    }

    public void ConnectWallet()
    {
        if (errorLabel != null)
            errorLabel.gameObject.SetActive(false);

#if UNITY_WEBGL && !UNITY_EDITOR
        if (MetamaskWebglInterop.IsMetamaskAvailable())
        {
            MetamaskWebglInterop.EnableEthereum(gameObject.name, nameof(EthereumEnabled), nameof(DisplayError));
        }
        else
        {
            DisplayError("Metamask is not available, please install it");
        }
#else
        DisplayError("ConnectWallet is only available on WebGL builds.");
#endif
    }

    public void EthereumEnabled(string addressSelected)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (!isMetamaskInitialised)
        {
            MetamaskWebglInterop.EthereumInit(gameObject.name, nameof(NewAccountSelected), nameof(ChainChanged));
            MetamaskWebglInterop.GetChainId(gameObject.name, nameof(ChainChanged), nameof(DisplayError));
            isMetamaskInitialised = true;
        }
        NewAccountSelected(addressSelected);
#else
        NewAccountSelected(addressSelected);
#endif
    }

    public void ChainChanged(string chainId)
    {
        Debug.Log(chainId);
        currentChainId = new HexBigInteger(chainId).Value;
        // You can add additional chain-change handling here if needed.
    }

    public void NewAccountSelected(string accountAddress)
    {
        selectedAccountAddress = accountAddress;
        if (accountSelectedLabel != null)
        {
            accountSelectedLabel.text = accountAddress;
            accountSelectedLabel.gameObject.SetActive(true);
        }
        if (walletAddressTMPText != null)
        {
            walletAddressTMPText.text = accountAddress;
            walletAddressTMPText.gameObject.SetActive(true);
        }

        if (connectPanel != null)
            connectPanel.SetActive(false);
        if (successPanel != null)
            successPanel.SetActive(true);

        if (errorLabel != null)
            errorLabel.gameObject.SetActive(false);

        if(walletStore!= null)
            walletStore.StoreWalletInfo();

    }

    public void DisplayError(string errorMessage)
    {
        if (errorLabel != null)
        {
            errorLabel.text = errorMessage;
            errorLabel.gameObject.SetActive(true);
        }
    }

    // Public property to allow other scripts to retrieve the wallet address.
    public string WalletAddress
    {
        get { return selectedAccountAddress; }
    }
}
