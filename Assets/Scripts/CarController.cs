using UnityEngine;
using System.Collections; // Required for potential future coroutines, though not strictly needed for current logic

// This attribute ensures that an AudioSource component is present on the GameObject.
// If it's not, Unity will automatically add one when this script is attached.
[RequireComponent(typeof(AudioSource))]
public class CarController : MonoBehaviour
{
    // --- Enums ---
    /// <summary>
    /// Defines the possible gears for the car.
    /// </summary>
    public enum CarGear
    {
        Neutral,
        First,
        Second,
        Third,
        Fourth,
        Reverse,
    }

    // --- Inspector Visible Properties ---
    [Header("Engine & Gear State")]
    [Tooltip("Is the car's engine currently running? Can only be started if in Neutral.")]
    public bool isStarted = false;

    [Tooltip("The currently selected gear of the car.")]
    public CarGear currentGear = CarGear.Neutral;

    [Header("Audio Clips")]
    [Tooltip("Sound to play when the engine starts.")]
    public AudioClip engineStartSound;

    [Tooltip("Sound to play when the engine stops.")]
    public AudioClip engineStopSound;

    [Tooltip("Looping sound when the engine is running and in Neutral.")]
    public AudioClip engineNeutralLoopSound;

    [Tooltip("Looping sound when the engine is running and in First gear.")]
    public AudioClip engineFirstGearLoopSound;

    [Tooltip("Looping sound when the engine is running and in Second gear.")]
    public AudioClip engineSecondGearLoopSound;

    [Tooltip("Looping sound when the engine is running and in Third gear.")]
    public AudioClip engineThirdGearLoopSound;

    [Tooltip("Looping sound when the engine is running and in Reverse gear.")]
    public AudioClip engineReverseLoopSound;

    // --- Private Fields ---
    private AudioSource engineAudioSource; // Reference to the AudioSource component
    private bool previousIsStartedState; // Tracks the engine state from the previous frame/validation
    private CarGear previousGearState; // Tracks the gear state from the previous frame/validation

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Used for initialization.
    /// </summary>
    void Awake()
    {
        // Get the AudioSource component attached to this GameObject
        engineAudioSource = GetComponent<AudioSource>();

        // Initialize previous states to the current Inspector values
        // This is important for the first Update call and OnValidate.
        previousIsStartedState = isStarted;
        previousGearState = currentGear;
    }

    /// <summary>
    /// Called on the frame when a script is enabled just before any of the Update methods are called the first time.
    /// </summary>
    void Start()
    {
        // Set initial audio state based on Inspector values
        if (isStarted)
        {
            // If starting in a running state (e.g., set in editor and game starts)
            // and not in neutral (which should be prevented by OnValidate but good to double check)
            if (currentGear != CarGear.Neutral)
            {
                Debug.LogWarning("Car was set to 'isStarted' in editor but not in Neutral. Forcing stop.");
                isStarted = false; // Force stop
                previousIsStartedState = false; // Ensure state is consistent
                engineAudioSource.Stop();
            }
            else
            {
                // Play the start sound (briefly) then switch to the neutral loop
                // This assumes the start sound is short. If it's long, you might want a coroutine.
                PlayOneShotSound(engineStartSound); // Play start sound
                UpdateEngineLoopingSound(); // Then immediately set the correct loop
            }
        }
        else
        {
            engineAudioSource.Stop(); // Ensure no sound is playing if not started
        }
    }

    /// <summary>
    /// This function is called in the editor when the script is loaded or a value is changed in the Inspector.
    /// It's used here to enforce the "start only in neutral" rule at edit time.
    /// </summary>
    void OnValidate()
    {
        // We need to ensure engineAudioSource is not null if OnValidate is called before Awake (can happen in editor)
        if (engineAudioSource == null)
        {
            engineAudioSource = GetComponent<AudioSource>();
        }

        // --- Rule: Can only check 'isStarted' (change from false to true) if in Neutral. ---
        // Check if 'isStarted' was just ticked ON by the user in the Inspector.
        if (isStarted && !previousIsStartedState) // If 'isStarted' is true now, but was false before this validation
        {
            if (currentGear != CarGear.Neutral)
            {
                Debug.LogWarning("Cannot start engine: Car must be in Neutral. Reverting 'isStarted' to false.");
                isStarted = false; // Force it back off
                                   // Note: Unity might sometimes fight this immediate change in OnValidate.
                                   // The runtime check in Update() is a more robust backup.
            }
        }

        // Update previous states for the *next* OnValidate call or the first Update call.
        // This should ideally happen after all checks.
        // For runtime, previousIsStartedState is updated at the end of Update().
        // For OnValidate, this makes previousIsStartedState reflect the state *before* this current validation pass for the next one.
        // This can be a bit tricky. The most reliable way is often a custom editor.
        // For now, we'll update it here, and also at the end of Update().
        previousIsStartedState = isStarted;
        previousGearState = currentGear; // Also update previous gear for consistency
    }


    /// <summary>
    /// Called every frame.
    /// Used for runtime game logic.
    /// </summary>
    void Update()
    {
        HandleEngineStateChanges();
        HandleGearChanges();

        // Update previous states for the next frame's comparison
        previousIsStartedState = isStarted;
        previousGearState = currentGear;
    }

    /// <summary>
    /// Handles logic related to the engine starting or stopping.
    /// </summary>
    private void HandleEngineStateChanges()
    {
        if (isStarted != previousIsStartedState) // If the engine state has changed since last frame
        {
            if (isStarted) // Engine was just turned ON
            {
                // Runtime check: Can only start if in Neutral.
                if (currentGear == CarGear.Neutral)
                {
                    PlayOneShotSound(engineStartSound);
                    UpdateEngineLoopingSound(); // Start the neutral loop
                    Debug.Log("Engine Started.");
                }
                else
                {
                    Debug.LogWarning("Runtime: Attempted to start engine while not in Neutral. Reverting.");
                    isStarted = false; // Force it back off
                                       // No sound change needed as it shouldn't have started.
                }
            }
            else // Engine was just turned OFF
            {
                PlayOneShotSound(engineStopSound);
                engineAudioSource.loop = false; // Stop any current looping sound
                engineAudioSource.Stop(); // Explicitly stop
                Debug.Log("Engine Stopped.");
            }
        }
    }

    /// <summary>
    /// Handles logic related to gear changes while the engine is running.
    /// </summary>
    private void HandleGearChanges()
    {
        if (currentGear != previousGearState && isStarted) // If gear changed AND engine is running
        {
            Debug.Log($"Gear changed to: {currentGear}");
            UpdateEngineLoopingSound(); // Update to the new gear's looping sound
        }
    }

    /// <summary>
    /// Updates the looping engine sound based on the current gear.
    /// This is called when the engine starts, or when the gear changes while the engine is running.
    /// </summary>
    private void UpdateEngineLoopingSound()
    {
        if (!isStarted)
        {
            if (engineAudioSource.isPlaying && engineAudioSource.loop)
            {
                engineAudioSource.Stop();
            }
            return;
        }

        AudioClip clipToLoop = null;
        switch (currentGear)
        {
            case CarGear.Neutral:
                clipToLoop = engineNeutralLoopSound;
                break;
            case CarGear.First:
                clipToLoop = engineFirstGearLoopSound;
                break;
            case CarGear.Second:
                clipToLoop = engineSecondGearLoopSound;
                break;
            case CarGear.Third:
                clipToLoop = engineThirdGearLoopSound;
                break;
            case CarGear.Reverse:
                clipToLoop = engineReverseLoopSound;
                break;
            default:
                // No specific sound for this gear, or an undefined gear.
                // Stop any looping sound.
                if (engineAudioSource.isPlaying && engineAudioSource.loop)
                {
                    engineAudioSource.Stop();
                }
                return;
        }

        if (clipToLoop != null)
        {
            // Only change and play if the new looping sound is different,
            // or if it's not playing, or if it's not set to loop (e.g., after a one-shot).
            if (engineAudioSource.clip != clipToLoop || !engineAudioSource.isPlaying || !engineAudioSource.loop)
            {
                engineAudioSource.clip = clipToLoop;
                engineAudioSource.loop = true;
                engineAudioSource.Play();
            }
        }
        else
        {
            // If no clip is assigned for the current gear, stop any looping sound.
            if (engineAudioSource.isPlaying && engineAudioSource.loop)
            {
                engineAudioSource.Stop();
            }
        }
    }

    /// <summary>
    /// Helper method to play a one-shot (non-looping) sound.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    private void PlayOneShotSound(AudioClip clip)
    {
        if (clip != null && engineAudioSource != null)
        {
            engineAudioSource.PlayOneShot(clip);
        }
    }
}
