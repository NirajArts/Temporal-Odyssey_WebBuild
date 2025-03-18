using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Nethereum.Hex.HexTypes;
using Nethereum.Unity.Contracts;
using Nethereum.Unity.Rpc;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Unity.VisualScripting;

public class NFTMinter : MonoBehaviour
{
    [Header("Wallet Connector Reference")]
    public WalletStore walletStore;

    [Header("Minting Settings")]
    [Tooltip("Level ID for minting (as string). Can be set externally.")]
    public string levelIdToMint;
    [Tooltip("Metadata URI for minting. Can be set externally.")]
    public string metadataURIToMint;

    [Header("Contract Settings")]
    [Tooltip("RPC endpoint for non-WebGL testing")]
    public string rpcURL = "https://rpc.test2.btcs.network/";
    [Tooltip("Test account private key (do not hardcode sensitive info)")]
    public string testPrivateKey = "";
    [Tooltip("Chain ID for non-WebGL testing")]
    public BigInteger chainId = 444444444500;
    [Tooltip("NFT contract address")]
    public string contractAddress = "0x72b0e0C76cd6Ae41979B728282C3e748D0C2A278";

    [Header("Optional UI Elements")]
    public Button mintNFTButton;      // Optionally call minting from a UI button.
    public Button fetchNFTsButton;    // Optionally call fetching from a UI button.
    public Transform nftListView;     // Container for NFT images if needed.

    void Start()
    {
        if (mintNFTButton != null)
            mintNFTButton.onClick.AddListener(() => StartCoroutine(MintNFT()));
        if (fetchNFTsButton != null)
            fetchNFTsButton.onClick.AddListener(() => StartCoroutine(FetchAvailableNFTs()));

        walletStore = FindFirstObjectByType<WalletStore>();
    }

    public void MintNFTWithCoroutine(string levelTOMintId, string metadataURIMint)
    {
        levelIdToMint = levelTOMintId;
        metadataURIToMint = metadataURIMint;
        StartCoroutine(MintNFT());
    }

    /// <summary>
    /// Mints an NFT using the preset public strings.
    /// You can also call MintNFT(string levelId, string metadataURI) externally.
    /// </summary>
    public IEnumerator MintNFT()
    {
        yield return MintNFT(levelIdToMint, metadataURIToMint);
        Debug.Log("Mint Data: " + levelIdToMint + " " + metadataURIToMint);
    }

    /// <summary>
    /// Mints an NFT using provided levelId and metadataURI.
    /// </summary>
    public IEnumerator MintNFT(string levelId, string metadataURI)
    {
        var contractTransactionUnityRequest = GetContractTransactionUnityRequest();
        if (contractTransactionUnityRequest != null)
        {
            var mintFunction = new MintNFTFunction()
            {
                Player = walletStore.PermaWalletAddressText.text,
                LevelId = BigInteger.Parse(levelId),
                MetadataURI = metadataURI
            };

            yield return contractTransactionUnityRequest.SignAndSendTransaction<MintNFTFunction>(mintFunction, contractAddress);
            if (contractTransactionUnityRequest.Exception == null)
            {
                Debug.Log("Minting successful. Transaction hash: " + contractTransactionUnityRequest.Result);
            }
            else
            {
                Debug.Log("error");
            }
        }
    }

    /// <summary>
    /// Fetches available NFTs for the connected wallet.
    /// Assumes the contract implements a view function 'getNFTsOfPlayer(address player)' returning an array of uint256.
    /// </summary>
    public IEnumerator FetchAvailableNFTs()
    {
        // If you only need to read data, you can omit the dataAccountKey 
        // and chainId by passing null. For example:
        // var queryRequest = new QueryUnityRequest<GetNFTsOfPlayerFunction, GetNFTsOfPlayerOutputDTO>(
        //     rpcURL
        // );

        // If you want to specify the 'from' address (dataAccountKey) or chain ID:
        var queryRequest = new QueryUnityRequest<GetNFTsOfPlayerFunction, GetNFTsOfPlayerOutputDTO>(
            rpcURL,
            walletStore.PermaWalletAddressText.text   // dataAccountKey (optional)
        );

        var getNFTsFunction = new GetNFTsOfPlayerFunction
        {
            Player = walletStore.PermaWalletAddressText.text
        };

        // Perform the query
        yield return queryRequest.Query(getNFTsFunction, contractAddress);

        // Check for exceptions
        if (queryRequest.Exception == null)
        {
            var result = queryRequest.Result;
            if (result.TokenIds != null)
            {
                string tokenList = "Player NFTs: ";
                foreach (var tokenId in result.TokenIds)
                {
                    tokenList += tokenId.ToString() + " ";
                }
                Debug.Log(tokenList);
                // Optionally, update UI images based on the token IDs.
            }
        }
        else
        {
            Debug.Log("error");
        }
    }

    public IUnityRpcRequestClientFactory GetUnityRpcRequestClientFactory()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (Nethereum.Unity.Metamask.MetamaskWebglInterop.IsMetamaskAvailable())
        {
            return new Nethereum.Unity.Metamask.MetamaskWebglCoroutineRequestRpcClientFactory(walletStore.PermaWalletAddressText.text, null, 1000);
        }
        else
        {
            // walletConnector.DisplayError("Metamask is not available, please install it");
            return null;
        }
#else
        return new UnityWebRequestRpcClientFactory(rpcURL);
#endif
    }

    public IContractTransactionUnityRequest GetContractTransactionUnityRequest()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (Nethereum.Unity.Metamask.MetamaskWebglInterop.IsMetamaskAvailable())
        {
            return new Nethereum.Unity.Metamask.MetamaskTransactionCoroutineUnityRequest(walletStore.PermaWalletAddressText.text, GetUnityRpcRequestClientFactory());
        }
        else
        {
            // walletConnector.DisplayError("Metamask is not available, please install it");
            return null;
        }
#else
        return new TransactionSignedUnityRequest(rpcURL, testPrivateKey, chainId);
#endif
    }
}

/// <summary>
/// Represents the mintNFT function call.
/// Solidity: function mintNFT(address player, uint256 levelId, string metadataURI) public returns (uint256)
/// </summary>
[Function("mintNFT", "uint256")]
public class MintNFTFunction : FunctionMessage
{
    [Parameter("address", "player", 1)]
    public string Player { get; set; }

    [Parameter("uint256", "levelId", 2)]
    public BigInteger LevelId { get; set; }

    [Parameter("string", "metadataURI", 3)]
    public string MetadataURI { get; set; }
}

/// <summary>
/// Represents the query function to get NFTs for a player.
/// Solidity: function getNFTsOfPlayer(address player) public view returns (uint256[])
/// </summary>
[Function("getNFTsOfPlayer", typeof(GetNFTsOfPlayerOutputDTO))]
public class GetNFTsOfPlayerFunction : FunctionMessage
{
    [Parameter("address", "player", 1)]
    public string Player { get; set; }
}

/// <summary>
/// Output DTO for getNFTsOfPlayer function.
/// </summary>
public class GetNFTsOfPlayerOutputDTO : IFunctionOutputDTO
{
    [Parameter("uint256[]", "", 1)]
    public System.Collections.Generic.List<BigInteger> TokenIds { get; set; }
}
