using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NftMintButton : MonoBehaviour
{
    NFTMinter nFTMinter;
    public int thisNFTLevel = 0;
    private int nftIndex;
    private bool allowMint = false;
    public Button button;
    public Animator animator;

    [Header("Minting Settings")]
    [Tooltip("Level ID for minting (as string). Can be set externally.")]
    public string levelIdToMint;
    [Tooltip("Metadata URI for minting. Can be set externally.")]
    public string metadataURIToMint = "ipfs://bafkreihftsqvo4wg6bgf7n5cs3664wb5bm6r3wrsq537h6xf4ut4nxqdoi";
    void Start()
    {
        nFTMinter = FindAnyObjectByType<NFTMinter>();
        nftIndex = PlayerPrefs.GetInt("NFT Index", 0);

        if(thisNFTLevel <= nftIndex)
            allowMint = true;
        else
            allowMint = false;

        if(allowMint)
            button.interactable = true;
        else
            button.interactable = false;
            
    }

    public void MintGivenNFT()
    {
        if(nFTMinter!=null)
        {
     //       nFTMinter.MintNFTDirect(levelIdToMint, metadataURIToMint);
            nFTMinter.MintNFTWithCoroutine(levelIdToMint, metadataURIToMint);
        }
    }
    
    public void AnimateIn(){
        if(animator!= null && allowMint)
            animator.SetTrigger("PopUp");
    }
    public void AnimateOut(){
        if(animator!= null && allowMint)
            animator.SetTrigger("PopDown");
    }

}
