using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EngineNote
{
    public AudioSource source;
    public float minRPM;
    public float peakRPM;
    public float maxRPM;
    public float pitchReferenceRPM;
    public float minPitch;
    public float maxPitch;
    public float SetPitchAndGetVolumeForRPM(float rpm)
    {
        //source.pitch = rpm / pitchReferenceRPM;

        if(rpm < pitchReferenceRPM) {
            source.pitch = Mathf.Lerp(minPitch, 1, Mathf.InverseLerp(minRPM, pitchReferenceRPM, rpm));
        }
        else {
            source.pitch = Mathf.Lerp(1, maxPitch, Mathf.InverseLerp(pitchReferenceRPM, maxRPM, rpm));
        }
        if (rpm < minRPM || rpm > maxRPM) {
            return 0f;
        }

        if (rpm < peakRPM) {            
            return Mathf.InverseLerp(minRPM, peakRPM, rpm);
        }
        else {
            return Mathf.InverseLerp(maxRPM, peakRPM, rpm);
        }
    }

    public void SetVolume(float volume)
    {
        source.mute = (source.volume = volume) == 0;
    }

}

public class VehicleAudio : MonoBehaviour
{
    [SerializeField] private EngineNote[] engineNotes;
    [SerializeField] private Sound vehicleEnterSound;
    [SerializeField] private Transform vehicleEnginePosition;
    [SerializeField] private float vehicleEnterEngineSoundDelay;

    private float[] workingVolumes;
    private float vehicleEnterTime;

    
    private void Awake()
    {
        CarPhysx carPhysx = GetComponent<CarPhysx>();
        carPhysx.OnVehicleEnter += OnVehicleEnter;
        carPhysx.OnVehicleLeave += OnVehicleLeave;
        workingVolumes = new float[engineNotes.Length];
        for (int i = 0; i < engineNotes.Length; ++i) {
            engineNotes[i].SetVolume(0);
        }
    }

    public void HandleVehicleAudio(float rpm)
    {
        // The total volume calculated for all engine notes won't generally sum to 1.
        // Calculate what they do sum to and then scale the individual volumes to ensure
        // consistent volume across the RPM range.
        if (vehicleEnterEngineSoundDelay > Time.time - vehicleEnterTime)
            return;
        float totalVolume = 0f;
        for (int i = 0; i < engineNotes.Length; ++i) {
            totalVolume += workingVolumes[i] = engineNotes[i].SetPitchAndGetVolumeForRPM(rpm);
        }

        
        for (int i = 0; i < engineNotes.Length; ++i) {
            engineNotes[i].SetVolume(AudioManager.masterVolume * AudioManager.sfxVolume * workingVolumes[i] / totalVolume);
        }        
    }

    private void OnVehicleEnter()
    {
        AudioManager.PlaySound(vehicleEnterSound, vehicleEnginePosition.position, vehicleEnginePosition);
        vehicleEnterTime = Time.time;
    }

    private void OnVehicleLeave()
    {
        for (int i = 0; i < engineNotes.Length; ++i) {
            engineNotes[i].SetVolume(0);
        }
    }
}
