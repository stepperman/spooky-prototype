using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace QTea
{
	/// <summary>
	/// Plays a buffer from the <see cref="IPCMBuffer"/> by
	/// insert a DSP between the chain.
	/// </summary>
	public class MicrophoneEventProgrammer : MonoBehaviour
	{
		/// <summary>
		/// Buffer that the event programmer should use.
		/// It is fetched by getting the component with the implemented interface (any work).
		/// </summary>
		private IPCMBuffer buffer;

		[SerializeField, FMODUnity.EventRef]
		private string @event;
		private FMOD.Studio.EventInstance eventInstance;
		private bool playing;

		private FMOD.DSP_READCALLBACK dspReadCallback;
		private FMOD.DSP_SHOULDIPROCESS_CALLBACK dspShouldProcessCallback;
		private FMOD.DSP fmodDsp;

		private void Start()
		{
			buffer = GetComponent<IPCMBuffer>();

			if(buffer == null)
			{
				UnityEngine.Debug.LogError("The Microphone Event Programmer requires an assigned buffer. Please assign any buffer.");
			}

			eventInstance = FMODUnity.RuntimeManager.CreateInstance(@event);
			AttachInstance();

			dspReadCallback = new FMOD.DSP_READCALLBACK(FMOD_DSPREADCALLBACK);

			FMOD.DSP_DESCRIPTION dspdes = new FMOD.DSP_DESCRIPTION
			{
				read = dspReadCallback,
				numinputbuffers = 0,
				numoutputbuffers = 1,
				version = 1,
				name = System.Text.Encoding.ASCII.GetBytes("voice"),
				numparameters = 0
			};

			FMOD.RESULT result = FMODUnity.RuntimeManager.CoreSystem.createDSP(ref dspdes, out fmodDsp);
			UnityEngine.Debug.Log("Creating DPS " + result);

			eventInstance.start();

			StartCoroutine(DSPAddCoroutine());

			//eventInstance.release();
		}

		private IEnumerator DSPAddCoroutine()
		{
			// audio is a different thread so we need to wait until the channel group is created.

			yield return new WaitForSeconds(0.5f);
			var result = eventInstance.getChannelGroup(out var group);
			Debug.Log("getChannelGroup " + result);
			result = group.addDSP(0, fmodDsp);
			Debug.Log("addDSP " + result);
		}

		private void OnDestroy()
		{
			// before removing the DSP make sure it's gone from the channel group too!
			// (without this unity hangs)
			eventInstance.getChannelGroup(out var channelGroup);
			channelGroup.removeDSP(fmodDsp);

			fmodDsp.release();
			fmodDsp.clearHandle();
			eventInstance.release();
			eventInstance.clearHandle();
		}

		private void AttachInstance()
		{
			var rigidbody = GetComponent<Rigidbody>();
			eventInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform, rigidbody));
			FMODUnity.RuntimeManager.AttachInstanceToGameObject(eventInstance, transform, rigidbody);
		}

		private FMOD.RESULT FMOD_DSPREADCALLBACK(ref FMOD.DSP_STATE state, IntPtr @in, IntPtr @out, uint length, int channels, ref int outchannels)
		{
			var stateFunctions = Marshal.PtrToStructure<FMOD.DSP_STATE_FUNCTIONS>(state.functions);
			int rate = 0;
			uint blocksize = 0;
			int speakermode_mixer = 0, speakermode_output = 0;

			stateFunctions.getsamplerate(ref state, ref rate);
			stateFunctions.getblocksize(ref state, ref blocksize);
			stateFunctions.getspeakermode(ref state, ref speakermode_mixer, ref speakermode_output);

			float[] samples = buffer.PopSamples(length / (uint)channels);
			outchannels = 1;
			Debug.Log("read callback " + length / 4 + " " + channels + $"\n<color=yellow>rate</color> = {rate}. <color=yellow>blocksize</color> = {blocksize}. " +
				$"<color=yellow>speakermode_mixer</color> = {speakermode_mixer}. <color=yellow>speakermode_output</color> = {speakermode_output}. " +
				$"\n<color=yellow>channel in</color> = {channels}. <color=yellow>channel out</color> = {outchannels}");

			unsafe
			{
				float* data = (float*)@out.ToPointer();

				for (int i = 0; i < samples.Length; i++)
				{
					*data++ = samples[i];
				}
			}

			return FMOD.RESULT.OK;
		}
	}
}
