using UnityEngine;
using System;
using FMODUnity;
using System.Runtime.InteropServices;
using System.IO;

namespace QTea
{
	public class MicrophoneEventProgrammer : MonoBehaviour
	{
		private IPCMBuffer buffer;

		[SerializeField, EventRef]
		private string @event;
		private FMOD.Studio.EventInstance eventInstance;
		private FMOD.Studio.EVENT_CALLBACK programInstrumentCreatedEvent;
		private FMOD.SOUND_PCMREAD_CALLBACK pcmReadCallback;

		private bool playing;

		private void Start()
		{
			buffer = GetComponent<IPCMBuffer>();

			if(buffer == null)
			{
				Debug.LogError("The Microphone Event Programmer requires an assigned buffer. Please assign any buffer.");
			}

			WriteFile("Start GOT INSTANCE");
			eventInstance = FMODUnity.RuntimeManager.CreateInstance(@event);

			WriteFile("Start CREATING EVENT CALLBACK");
			programInstrumentCreatedEvent = new FMOD.Studio.EVENT_CALLBACK(FMOD_OnProgramInsturmentCreatedEvent);
			WriteFile("Start ASSIGNING CALLBACKS");
			var callbacks =
				FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND
				| FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND
				| FMOD.Studio.EVENT_CALLBACK_TYPE.STARTED
				| FMOD.Studio.EVENT_CALLBACK_TYPE.STOPPED;
			WriteFile("Start SETTINGS CALLBAKCS");
			eventInstance.setCallback(programInstrumentCreatedEvent, callbacks);

			WriteFile("Start STARTING INSTANCE");
			eventInstance.start();
			WriteFile("Start RELEASEING");
			eventInstance.release();
		}

		private void OnDestroy()
		{
			WriteFile("OnDestroy RELEASING");
			eventInstance.release();
			WriteFile("OnDestroy CLEAR HANDLE ");
			eventInstance.clearHandle();
		}

		[AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
		private FMOD.RESULT FMOD_OnProgramInsturmentCreatedEvent(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr eventPtr, IntPtr parameters)
		{
			// what the fuck am i doing
			switch (type)
			{
				case FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
					WriteFile("FMOD_OnProgramInsturmentCreatedEvent CREATE_PROGRAMMER_SOUND");

					Debug.Log("Programmer Sound Created");
					var properties = Marshal.PtrToStructure<FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES>(parameters);
					var result = CreateProgrammerInstrument(ref properties);
					if (result != FMOD.RESULT.OK)
					{
						Debug.Log(result);
						return result;
					}
					Marshal.StructureToPtr(properties, parameters, false);
					return result;
				case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
					WriteFile("FMOD_OnProgramInsturmentCreatedEvent DESTROY_PROGRAMMER_SOUND");

					var destroyParameters = Marshal.PtrToStructure<FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES>(parameters);
					FMOD.Sound sound = new FMOD.Sound(destroyParameters.sound);
					return sound.release();
				case FMOD.Studio.EVENT_CALLBACK_TYPE.STARTED:
					WriteFile("FMOD_OnProgramInsturmentCreatedEvent STARTED");
					playing = true;
					break;
				case FMOD.Studio.EVENT_CALLBACK_TYPE.STOPPED:
					WriteFile("FMOD_OnProgramInsturmentCreatedEvent STOPPED");
					playing = false;
					break;
			}

			return FMOD.RESULT.OK;
		}

		private FMOD.RESULT CreateProgrammerInstrument(ref FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES soundProperties)
		{
			WriteFile("CreateProgrammerInstrument()");

			FMOD.CREATESOUNDEXINFO exInfo = CreateSoundInfo();
			var result = FMODUnity.RuntimeManager.CoreSystem.createSound(
				"playback",
				FMOD.MODE.OPENUSER | FMOD.MODE.LOOP_NORMAL | FMOD.MODE.CREATESTREAM,
				ref exInfo, out FMOD.Sound sound);
			soundProperties.sound = sound.handle;
			soundProperties.subsoundIndex = -1;
			return result;
		}

		private FMOD.CREATESOUNDEXINFO CreateSoundInfo()
		{
			WriteFile("CreateSoundInfo()");

			FMOD.CREATESOUNDEXINFO exInfo = default;
			exInfo.cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO));
			exInfo.pcmreadcallback = (pcmReadCallback = new FMOD.SOUND_PCMREAD_CALLBACK(FMOD_ReadCallback));
			exInfo.format = FMOD.SOUND_FORMAT.PCMFLOAT;
			exInfo.numchannels = 1;          // REPLACE WITH OPUS SETTINGS
			exInfo.defaultfrequency = 48000; // REPLACE WITH OPUS SETTINGS
			int delay = 20;                  // REPLACE WITH OPUS SETTINGS
			exInfo.decodebuffersize = (uint)(exInfo.defaultfrequency / 1000 * delay); // REPLACE WITH OPUS SETTINGS
			exInfo.length = (uint)(exInfo.defaultfrequency / 1000 * delay * exInfo.numchannels * sizeof(float));
			return exInfo;
		}

		private FMOD.RESULT FMOD_ReadCallback(IntPtr sound, IntPtr data, uint segmentLength)
		{
			WriteFile("READ_CALLBACK BEFORE ANYTHING");
			float[] samples = buffer.PopSamples(segmentLength / 4);
			WriteFile("READ_CALLBACK POPPED SAMPLES");

			if(samples == null || samples.Length == 0)
			{
				WriteFile("READ_CALLBACK NO SAMPLES. RETURNING");
				return FMOD.RESULT.OK;
			}

			WriteFile($"READ_CALLBACK GOT SAMPLES. segmentLength: {segmentLength}. samples.Length: {samples.Length}");

			unsafe
			{
				float* pcm = (float*)data.ToPointer();
				for (int i = 0; i < samples.Length; i++)
				{
					*pcm++ = samples[i];
				}

				return FMOD.RESULT.OK;
			}
		}

		private void WriteFile(string content)
		{
			Debug.Log(content);
			//File.AppendAllText(@"C:\Users\Stepe\Documents\testtesttest.txt", content + '\n');
		}
	}
}
