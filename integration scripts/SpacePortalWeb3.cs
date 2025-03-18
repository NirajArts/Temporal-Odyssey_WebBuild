using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpacePortalWeb3 : MonoBehaviour
{
    private bool allowTeleport = false;
    private PlayerInput input;
    private GameObject player;

    public float scaleSpeed = 1f;         // Speed of scaling down
    public float timeToTeleport = 1f;     // Time before teleporting

    public GameObject teleportEffect;
    public AudioClip teleportSound;          // Sound to play on spawn
    private AudioSource audioSource;      // Audio source for playing spawn sound

    public bool isDependentOnCrystal = false;
    private TimeCrystal timeCrystal;
    public GameObject TeleportCircle;
    private SphereCollider TeleportTrigger;

    private GameLevelManager gameLevelManager;
    private GamePauseManager gamePauseManager;

    [Header("SpeedRun best time variables")]
    public bool allowSavingBestDutation = false;
    private SpeedrunTimer speedrunTimer;

    public int nftIndexToAdd = 1;

    void Start()
    {
        input = FindObjectOfType<PlayerInput>();  // Find player input component
        audioSource = GetComponent<AudioSource>();

        gameLevelManager = FindAnyObjectByType<GameLevelManager>();
        gamePauseManager = FindAnyObjectByType<GamePauseManager>();
        speedrunTimer = FindAnyObjectByType<SpeedrunTimer>();

        timeCrystal = FindAnyObjectByType<TimeCrystal>();
        TeleportTrigger = GetComponent<SphereCollider>();

        if (isDependentOnCrystal)
        {
            TeleportCircle.SetActive(false);
            TeleportTrigger.enabled = false;
        }
    }

    void Update()
    {
        if (allowTeleport)
        {
            timeToTeleport -= Time.deltaTime;
            if (timeToTeleport <= 0)
            {
                // Scale down player from its current scale to zero at the speed of scaleSpeed
                Vector3 newScale = Vector3.Lerp(player.transform.localScale, Vector3.zero, scaleSpeed * Time.deltaTime);
                player.transform.localScale = newScale;

                // When the player's scale is near zero, trigger teleportation or disable the player
                if (player.transform.localScale.magnitude < 0.01f)
                {
                    player.transform.localScale = Vector3.zero;
                    TeleportPlayer();  // Trigger teleportation or other actions
                }
            }
        }
        if (isDependentOnCrystal)
        {
            if (timeCrystal != null)
            {
                return;
            }
            else
            {
                TeleportCircle.SetActive(true);
                TeleportTrigger.enabled = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            allowTeleport = true;
            if (other.TryGetComponent<PlayerInput>(out input))
            {
                input.enabled = false;  // Disable player input to stop movement during teleport
                player = other.gameObject;
            }
            if (speedrunTimer != null)
            {
                speedrunTimer.StopTimer();
                if(allowSavingBestDutation)
                    SaveTimer();
            }

            SpawnEffect();
            
            if (gamePauseManager != null)
                gamePauseManager.enabled = false;

            gameLevelManager.SendMessage("Teleported");
            TeleportTrigger.enabled = false;

            PlayerPrefs.SetInt("NFT Index", nftIndexToAdd);
        }
    }

    // Save the best timer
    public void SaveTimer()
    {
        float bestDuration = PlayerPrefs.GetFloat("BestDuration", 0);

        // Check if totalTime is a new record (lower than bestDuration) or if no best time is set
        if (bestDuration == 0 || speedrunTimer.totalTime < bestDuration)
        {
            PlayerPrefs.SetFloat("BestDuration", speedrunTimer.totalTime);
            PlayerPrefs.Save();
            Debug.Log("New Best Time Saved: " + speedrunTimer.totalTime);
        }
        // Add js external call here
    }

    // This function is called when the player is fully scaled down (teleported)
    void TeleportPlayer()
    {
        // Add your teleportation logic here, like changing player position or scene
        Debug.Log("Teleporting player...");
        // Example: Move the player to a new location (teleport destination)
//        player.transform.position = new Vector3(0, 0, 0);  // Example position, adjust as needed
    }

    void SpawnEffect()
    {
        // Play spawn effect
        if (teleportEffect != null)
        {
            Instantiate(teleportEffect, transform.position, Quaternion.identity);
        }
        if (teleportSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }
    }

    void MintNft()
    {
        
    }
}