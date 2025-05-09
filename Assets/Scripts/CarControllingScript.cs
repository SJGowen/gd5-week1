using System;
using System.Collections;
using System.Linq.Expressions;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CarControllingScript : MonoBehaviour
{
    [Header("Audio Clips")] 
    public AudioClip[] carSounds;
    public AudioSource audioSource;

    //private GearSelection _gearSelected = GearSelection.Neutral;
    [SerializeField]
    public GearSelection GearSelected = GearSelection.Neutral;

    private bool _carStarted;
    [SerializeField]
    public bool CarStarted = false;

    void Start()
    {
        Debug.Log($"Car created with speed: {GetSpeed()}");
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (CarStarted != _carStarted)
        {
            StartAndStopCar();
        }
        if (CarStarted)
        {
            if (audioSource.clip != carSounds[Math.Abs((int)GearSelected)])
            {
                audioSource.Stop();
                audioSource.clip = carSounds[Math.Abs((int)GearSelected)];
                audioSource.Play();
            }
            MoveCar();
        }
    }

    private void StartAndStopCar()
    {
        if (CarStarted && GearSelected == GearSelection.Neutral)
        {
            audioSource.clip = carSounds[carSounds.Length - 1];
            audioSource.PlayOneShot(audioSource.clip);
            AudioSourceDelay();
            audioSource.loop = true;
            audioSource.clip = carSounds[(int)GearSelection.Neutral];
            audioSource.Play();
            _carStarted = CarStarted;
        }
        else if (CarStarted && GearSelected != GearSelection.Neutral)
        {
            Debug.Log($"Cannot start car while it is in {GearSelected} gear!");
            CarStarted = false;
        }
        else if (!CarStarted)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false;
            _carStarted = CarStarted;
        }
    }

    private void AudioSourceDelay(int delay = 1)
    {
        var now = DateTime.Now;
        while (audioSource.isPlaying && DateTime.Now.Subtract(now).TotalSeconds < delay)
        {
            // Wait for the sound to finish playing
        }
    }

    private void MoveCar()
    {
        transform.Rotate(0, Input.GetAxis("Horizontal") * 70 * Time.deltaTime, 0);
        transform.Translate(0, 0, GetSpeed() * Time.deltaTime);
    }

    private int GetSpeed()
    {
        return (int)GearSelected;
    }
}

public enum GearSelection
{
    Neutral = 0,
    First = 1,
    Second = 2,
    Third = 3,
    Fourth = 4,
    Reverse = -1,
}
