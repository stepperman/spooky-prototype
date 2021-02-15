using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DN.Service;
using UnityEngine;

namespace QTea
{
	[Service]
	public class MicrophoneService
	{
		private const bool DEBUG = false;

		public event Action<float[]> OnSampleReady;

		public IEnumerable<FMODRecordDriverInfo> Recorders { get; private set; }
		public FMODRecordDriverInfo CurrentDevice { get; private set; }

		public bool Recording
		{
			get
			{
				FMODUnity.RuntimeManager.CoreSystem.isRecording(CurrentDevice.Id, out bool recording);
				return recording;
			}
		}

		public uint Position
		{
			get
			{
				FMODUnity.RuntimeManager.CoreSystem.getRecordPosition(CurrentDevice.Id, out uint position);
				return position;
			}
		}

		public int Channels => CurrentDevice.Channels;

		private FMOD.CREATESOUNDEXINFO recordingInfo;
		private FMOD.Sound recordingSound;
		private uint length;
		private int frequency = 48000;

		System.IO.FileStream sw;

		public MicrophoneService()
		{
			FMOD.System coreSystem = FMODUnity.RuntimeManager.CoreSystem;
			var recorders = new List<FMODRecordDriverInfo>();
			int numDrivers;
			coreSystem.getRecordNumDrivers(out numDrivers, out _);

			for(int i = 0; i < numDrivers; i++)
			{
				var result = coreSystem.getRecordDriverInfo(
					i, out string name, 255, out Guid guid, out int systemrate, 
					out var speakerMode, out int channels, out var state);
				if(result == FMOD.RESULT.OK && !name.Contains("loopback"))
				{
					recorders.Add(new FMODRecordDriverInfo(i, name, guid, systemrate, speakerMode, channels, state));
				}
			}

			Recorders = recorders;
			CurrentDevice = Recorders.First();

			var callbackService = ServiceLocator.Locate<UnityCallbackService>();
			callbackService.OnUpdate += OnUpdateEvent;
			callbackService.OnDestroy += OnDestroyEvent;
		}

		private void OnDestroyEvent()
		{
			StopRecording();
			var result = recordingSound.release();
			Debug.Log($"<color=yellow>recordingSound.release()</color> {result}");
			recordingSound.clearHandle();
			sw.Dispose();

		}

		public void StartRecording()
		{
			FMOD.CHANNELORDER channelOrder = CurrentDevice.Channels == 1 ? FMOD.CHANNELORDER.ALLMONO : FMOD.CHANNELORDER.ALLSTEREO;
			recordingInfo = new FMOD.CREATESOUNDEXINFO
			{
				cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO)),
				//pcmreadcallback = OnReadCallback,
				numchannels = CurrentDevice.Channels,
				defaultfrequency = 48000, // TODO : replace with Opus Settings service
				//decodebuffersize = 48000, // TODO : replace with Opus Settings service
				format = FMOD.SOUND_FORMAT.PCMFLOAT,
				channelorder = channelOrder
			};
			recordingInfo.length = (uint)(recordingInfo.defaultfrequency * recordingInfo.numchannels * sizeof(float));
			length = recordingInfo.length;

			Debug.Log($"<color=red>RECORDING STARTED WITH</color> {CurrentDevice.Name} {CurrentDevice.Channels}\n" +
				$"<color=green>length</color> {length}");

			FMODUnity.RuntimeManager.CoreSystem.createSound("mic", FMOD.MODE.LOOP_NORMAL | FMOD.MODE.OPENUSER, ref recordingInfo, out recordingSound);
			/*sw = System.IO.File.Open("C:\\Users\\Stepe\\Documents\\rawdatatesting.txt", System.IO.FileMode.Append, System.IO.FileAccess.Write);*/
			FMODUnity.RuntimeManager.CoreSystem.recordStart(CurrentDevice.Id, recordingSound, true);	
		}

		int lastPos = 0;
		private int channels = 1;
		object @lock = null;

		private void OnUpdateEvent()
		{
			/*void WriteRawData(float[] data)
			{
				for (int i = 0; i < data.Length; i++)
				{
					byte[] bytes = BitConverter.GetBytes(data[i]);
					sw.Write(bytes, 0, bytes.Length);
				}
			}*/

			// get the last 20 ms in samples.
			// 20 ms in samples is Frequency / 1000 * channels * 20 = 960 if 1 channel @ 48khz
			// 20 ms in position data is... Frequency / 1000 * channels * sizeof(float) > 4 * 20... I think = 3840
			// so convert 3840 to float, which then goes to 960 samples! 
			// We have to wait 3840 samples before reading the last 3840 samples. (and read the rest that was missed too.)

			// UPDATE : in hindsight it doesn't matter at all how much we read... it would be better if it just ready *anything*
			// and then offloaded that to the buffer. I was under the impression that I was going to do everything samples
			// of 960, but that makes no sense after all.

			int pos = (int)Position;
			int sampleCount = 0; // sample count that will be read

			// check if data is available first.
			bool dataAvailable = false;
			int difPos = pos - lastPos;
			if (difPos == 0)
			{
				return; // no new data!.
			}

			if(difPos < 0)
			{
				// meaning we wrapped, so extra calculation is needed for the size.
				difPos = (int)length - lastPos + pos;
			}

			int curPos = lastPos;
			StringBuilder logMessage = new StringBuilder();
			logMessage.AppendLine($"<color=red>curPos: {curPos}</color> <color=green>Pos: {Position}</color>");
			float[] buffer = new float[difPos / 4];

			IntPtr ptrData1, ptrData2;
			uint ptrLength1, ptrLength2;
			var result = recordingSound.@lock((uint)curPos, (uint)difPos, out ptrData1, out ptrData2, out ptrLength1, out ptrLength2);

			logMessage.AppendLine($"<color=yellow>Read result: l1 [ {ptrLength1} ] l2 [ {ptrLength2} ]</color>");
			logMessage.AppendLine($"Read until [ {curPos + difPos} ] (excluding wrap)");
			logMessage.AppendLine($"Lock {result}");

			float[] data1 = new float[ptrLength1 / sizeof(float)];

			unsafe
			{
				float *pcm = (float *)ptrData1.ToPointer();
				for(int j = 0; j < data1.Length; j++)
				{
					data1[j] = pcm[j];
				}
			}

			logMessage.AppendLine($"<color=blue>data1 length: {data1.Length} {data1[0]}</color>");

			result = recordingSound.unlock(ptrData1, ptrData2, ptrLength1, ptrLength2);
			logMessage.AppendLine($"Unlock {result}");

			OnSampleReady?.Invoke(data1);
			//WriteRawData(data1);
			curPos += difPos;
			if(curPos >= length)
			{
				curPos -= (int)length;
			}

			if(DEBUG) Debug.Log(logMessage.ToString());
			lastPos = curPos;
		}

		public void StopRecording()
		{
			if (Recording)
			{
				FMODUnity.RuntimeManager.CoreSystem.recordStop(CurrentDevice.Id);
			}
		}
	}

	public struct FMODRecordDriverInfo
	{ 
		public int Id { get; }
		public string Name { get; }
		public Guid Guid { get; }
		public int SystemRate { get; }
		public FMOD.SPEAKERMODE SpeakerMode { get; }
		public int Channels { get; }
		public FMOD.DRIVER_STATE DriverState { get; }

		public FMODRecordDriverInfo(
			int id,
			string name,
			Guid guid,
			int systemRate,
			FMOD.SPEAKERMODE speakerMode,
			int channels,
			FMOD.DRIVER_STATE driverState)
		{
			Id = id;
			Name = name;
			Guid = guid;
			SystemRate = systemRate;
			SpeakerMode = speakerMode;
			Channels = channels;
			DriverState = driverState;
		}
	}
}
