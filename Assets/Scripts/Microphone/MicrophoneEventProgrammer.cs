using UnityEngine;
using FMODUnity;
using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using FMOD;
using Debug = UnityEngine.Debug;

namespace QTea
{
	/// <summary>
	/// Plays a buffer from the <see cref="IPCMBuffer"/> by
	/// insert a DSP between the chain.
	/// </summary>
	public class MicrophoneEventProgrammer : MonoBehaviour
	{
		private const bool DEBUG = false;
		private const bool VERBOSE = false;
		private const string pluginName = "Resonance Audio Source";
		static private uint? _resonanceAudioSourceHandle = null;
		
		private uint ResonanceAudioSourceHandle
		{
			get
			{
				if(!_resonanceAudioSourceHandle.HasValue)
				{
					_resonanceAudioSourceHandle = GetResonancePluginHandle();
				}

				return _resonanceAudioSourceHandle.Value; 
			}
		}

		/// <summary>
		/// Buffer that the event programmer should use.
		/// It is fetched by getting the component with the implemented interface (any work).
		/// </summary>
		private IPCMBuffer buffer;

		private FMOD.ChannelGroup channelGroup;
		private FMOD.DSP resonanceDSP;
		private FMOD.Sound sound;
		//private FMOD.Channel channel;

		private FMOD.SOUND_PCMREAD_CALLBACK readCallback;

		private bool playing = false;

		private void Start()
		{
			buffer = GetComponent<IPCMBuffer>();

			if (buffer == null)
			{
				UnityEngine.Debug.LogError("The Microphone Event Programmer requires an assigned buffer. Please assign any buffer.");
				Destroy(this);
				return;
			}

			CreateChannelGroup();
			CreateDSP();
			CreateSound();

			// and finally, play the sound
			PlaySound();
		}

		private void PlaySound()
		{
			RESULT result = RuntimeManager.CoreSystem.playSound(sound, default/*channelGroup*/, false, out var channel); // yo
		}

		/// <summary>
		/// Creates the channel group where the DSP will be added in.
		/// </summary>
		private void CreateChannelGroup()
		{
			FMOD.RESULT result = FMODUnity.RuntimeManager.CoreSystem.createChannelGroup("voice", out channelGroup);
			channelGroup.setMode(MODE._3D);
			FDEBUG("createChannelGroup", result);
		}

		/// <summary>
		/// Creates the Resonance DSP and adds it to the channel group.
		/// </summary>
		private void CreateDSP()
		{
			RESULT result;
			// this might have to be applied every frame? (Everytime it reads?)
			var tdAttributes = FMODUnity.RuntimeUtils.To3DAttributes(transform, GetComponent<Rigidbody>());
			result = channelGroup.set3DAttributes(ref tdAttributes.position, ref tdAttributes.velocity);
			FDEBUG("set3DAttributes", result);

			// create the DSP
			result = RuntimeManager.CoreSystem.createDSPByPlugin(ResonanceAudioSourceHandle, out resonanceDSP);
			FDEBUG("createDSPByPlugin", result);
			// TODO : allow changing of plugin settings.

			// add it to the channelGroup
			//channelGroup.addDSP(0, resonanceDSP);
		}

		/// <summary>
		/// creates the sound and adds the channel group to it.
		/// </summary>
		private void CreateSound()
		{
			RESULT result;

			CREATESOUNDEXINFO exInfo = new CREATESOUNDEXINFO();
			exInfo.cbsize = Marshal.SizeOf(exInfo);
			exInfo.format = SOUND_FORMAT.PCMFLOAT;
			exInfo.numchannels = 1; 
			exInfo.defaultfrequency = 48000;
			exInfo.decodebuffersize = 48000/1000;
			exInfo.length = (uint)(exInfo.defaultfrequency * sizeof(float) * 10);
			readCallback = new SOUND_PCMREAD_CALLBACK(FMOD_PCM_READ_CALLBACK);
			exInfo.pcmreadcallback = readCallback;
			result = RuntimeManager.CoreSystem.createSound("voice chat", MODE.LOOP_NORMAL | MODE.CREATESTREAM | MODE.OPENUSER/* | MODE.OPENRAW*/, ref exInfo, out sound);
			
			FDEBUG("createSound", result);
		}

		private FMOD.RESULT FMOD_PCM_READ_CALLBACK(IntPtr sound, IntPtr data, uint datalen)
		{
			if(buffer.Count == 0)
			{
				FDEBUG("FMOD_PCM_READ_CALLBACK EMPTY BUFFER", RESULT.OK);
				return RESULT.OK;
			}

			if(!playing)
			{
				playing = buffer.Count > 48000 / 100;
				return RESULT.OK;
			}


			float[] samples = buffer.PopSamples(datalen);

			unsafe
			{
				float* fData = (float*)data.ToPointer();

				for (int i = 0; i < samples.Length; i++)
				{
					*fData++ = samples[i];
				}
			}

			FDEBUG("FMOD_PCM_READ_CALLBACK", RESULT.OK);
			return RESULT.OK;
		}

		private uint GetResonancePluginHandle()
		{
			RuntimeManager.CoreSystem.getNumPlugins(FMOD.PLUGINTYPE.DSP, out int numplugins);

			if (numplugins == 0) throw new Exception("Resonance plugin not loaded.");

			for (int i = 0; i < numplugins; i++)
			{
				RuntimeManager.CoreSystem.getPluginHandle(FMOD.PLUGINTYPE.DSP, i, out uint handle);
				RuntimeManager.CoreSystem.getPluginInfo(handle, out _, out string name, 50, out _);
				if(name == pluginName) return handle;
			}

			throw new Exception("Resonance plugin not loaded.");
		}

		private void OnDestroy()
		{
			Debug.Log("releasing microphone stuff..");
			RESULT result;

			// properly free FMOD memory and instances here.
			result = sound.release();
			FDEBUG("sound.release()", result);
			//channelGroup.removeDSP(resonanceDSP);
			result = resonanceDSP.release();
			FDEBUG("resonanceDSP.release()", result);
			result =channelGroup.release();
			FDEBUG("channelGroup.release()", result);
			buffer.Clear();
		}

		private void FDEBUG(string action, FMOD.RESULT result)
		{
#pragma warning disable CS0162 // Unreachable code detected
			if (!DEBUG) return;


			if (result != FMOD.RESULT.OK)
			{
				UnityEngine.Debug.LogError($"<color=red{action}</color> {result}");
			}
			else if(VERBOSE)
			{
				UnityEngine.Debug.Log($"<color=yellow>{action}</color> {result}");
			}
#pragma warning restore CS0162 // Unreachable code detected
		}
	}
}
