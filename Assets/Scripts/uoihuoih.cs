using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uoihuoih : MonoBehaviour
{
    private MicrophoneService misamdi;
    private float[] samples;

    void Start()
    {
        misamdi = ServiceLocator.Locate<MicrophoneService>();
        //NetworkedMicrophone.OnNewSampleWow += (a) => samples = a;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (samples == null) return;

        samples.CopyTo(data, 0);
        samples = null;
    }
}
