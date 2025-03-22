using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftController : MonoBehaviour
{
    [SerializeField] Transform lift;
    [SerializeField] Transform liftVisual;
    [SerializeField] float liftSpeed;
    [SerializeField] float maxHeight;
    [SerializeField] float minHeight;
    float currentHeight { get; set; } = 2.4f;

    [SerializeField] AudioSource liftAudioSource;
    [SerializeField] AudioClip liftSoundUp;
    [SerializeField] AudioClip liftSoundDown;
    [SerializeField] float volume;
    private bool lastInputWasUp = true;

    private bool buttonUp;
    private bool buttonDown;
    private void Awake()
    {
        liftAudioSource.clip = liftSoundUp;
        CarPhysx carPhysx = GetComponent<CarPhysx>();

        carPhysx.OnVehicleEnter += () => { 
            UIManager.possibleActionsUI.AddAction("E - move lift up");
            UIManager.possibleActionsUI.AddAction("Q - move lift down");
            liftAudioSource.mute = false;
            liftAudioSource.Play();
        };
        carPhysx.OnVehicleLeave += () => { 
            UIManager.possibleActionsUI.RemoveAction("E - move lift up");         
            UIManager.possibleActionsUI.RemoveAction("Q - move lift down");
            liftAudioSource.mute = true;
            liftAudioSource.Pause();
        };
    }

    private void Update()
    {
        if (buttonDown == buttonUp) {
            liftAudioSource.volume = 0;
            buttonUp = false;
            buttonDown = false;
            return;
        }
        liftAudioSource.volume = volume * AudioManager.sfxVolume;
        if (lastInputWasUp && buttonDown) {
            lastInputWasUp = false;
            liftAudioSource.clip = liftSoundDown;
            liftAudioSource.Play();
        }
        if (!lastInputWasUp && buttonUp) {
            lastInputWasUp = true;
            liftAudioSource.clip = liftSoundUp;
            liftAudioSource.Play();
        }
        buttonUp = false;
        buttonDown = false;
    }

    public void HandleInput(bool buttonUp, bool buttonDown){
        if (buttonDown == buttonUp) {
            return;
        }

        if (buttonUp && currentHeight < maxHeight) {
            this.buttonUp = true;
            currentHeight += Time.deltaTime * liftSpeed;
        }
        if (buttonDown && currentHeight > minHeight) {
            this.buttonDown = true;
            currentHeight -= Time.deltaTime * liftSpeed;
        }

        lift.transform.localPosition = new Vector3(lift.transform.localPosition.x, currentHeight, lift.transform.localPosition.z);
        liftVisual.transform.localPosition = lift.transform.localPosition;
    }
}
