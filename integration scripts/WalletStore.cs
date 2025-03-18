using UnityEngine;
using System.Numerics;  // Needed for BigInteger
using TMPro;            // For TextMeshPro UI elements

public class WalletStore : MonoBehaviour
{
    // Singleton instance for global access
    public static WalletStore Instance { get; private set; }

    // Stored wallet data
    public string walletAddress;
    public BigInteger chainId;
    public bool metamaskInitialised;

    // UI TextMeshPro fields to display wallet information
    public TMP_Text PermaWalletAddressText;
    public TMP_Text walletAddressText;
    public TMP_Text chainIdText;
    public TMP_Text metamaskStatusText;

    // Reference to the WalletConnector script (assumed to be in the scene)
    private WalletConnector walletConnector;

    // Make sure this GameObject persists across scenes
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Locate the WalletConnector in the scene (it should already be present)
    void Start()
    {
        walletConnector = FindObjectOfType<WalletConnector>();
        if (walletConnector == null)
        {
            Debug.LogError("WalletConnector not found in the scene!");
        }
    }

    // This method is intended to be called from the WalletConnector script.
    // It copies the relevant wallet data from WalletConnector to this store and updates the UI.
    public void StoreWalletInfo()
    {
        if (walletConnector != null)
        {
            // Collecting data from WalletConnector (ensure these fields/properties are accessible)
            walletAddress = walletConnector.WalletAddress;
            chainId = walletConnector.currentChainId;           // Assuming 'currentChainId' is accessible
            metamaskInitialised = walletConnector.isMetamaskInitialised; // Assuming 'isMetamaskInitialised' is accessible

            // Update UI text fields if they have been assigned
            if (PermaWalletAddressText != null)
            {
                PermaWalletAddressText.text = walletAddress;
            }
            if (walletAddressText != null)
            {
                walletAddressText.text = "Wallet Address: " + walletAddress;
            }
            if (chainIdText != null)
            {
                chainIdText.text = "Chain ID: " + chainId.ToString();
            }
            if (metamaskStatusText != null)
            {
                metamaskStatusText.text = "Metamask Initialized: " + metamaskInitialised.ToString();
            }

            Debug.Log("Wallet info stored. Address: " + walletAddress);
        }
        else
        {
            Debug.LogError("Cannot store wallet info because WalletConnector reference is missing.");
        }
    }

    // Public getters so any script can access the stored wallet info
    public string GetWalletAddress()
    {
        return walletAddress;
    }

    public BigInteger GetChainId()
    {
        return chainId;
    }

    public bool IsMetamaskInitialised()
    {
        return metamaskInitialised;
    }

    // Optional: Public setters to update additional info from WalletConnector
    public void SetChainId(BigInteger newChainId)
    {
        chainId = newChainId;
        if (chainIdText != null)
        {
            chainIdText.text = "Chain ID: " + chainId.ToString();
        }
    }

    public void SetMetamaskInitialised(bool status)
    {
        metamaskInitialised = status;
        if (metamaskStatusText != null)
        {
            metamaskStatusText.text = "Metamask Initialized: " + metamaskInitialised.ToString();
        }
    }
}
