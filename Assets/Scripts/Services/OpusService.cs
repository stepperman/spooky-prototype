using DN;
using POpusCodec;
using POpusCodec.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Service]
public class OpusService 
{
	public OpusEncoder Encoder { get; private set; }
	public OpusDecoder Decoder { get; private set; }

	private MicrophoneService microphoneService;
	private OpusSettingsService opusSettings;

	public OpusService()
	{
		opusSettings = ServiceLocator.Locate<OpusSettingsService>();
		microphoneService = ServiceLocator.Locate<MicrophoneService>();

		Encoder = new OpusEncoder(SamplingRate, Channel, opusSettings.BitRate);
		Encoder.EncoderDelay = opusSettings.Delay;
		opusSettings.FrameSize = Encoder.FrameSizePerChannel * (int)Channel;

		Decoder = new OpusDecoder(SamplingRate, Channel);
	}

	public byte[] Encode(float[] samples) => Encoder.Encode(samples);
	public float[] Decode(byte[] encodedSamples) => Decoder.DecodePacketFloat(encodedSamples);

	public void Dispose()
	{
		Encoder.Dispose();
		Decoder.Dispose();
	}

	private SamplingRate SamplingRate
	{
		get
		{
			switch (opusSettings.Frequency)
			{
				case 16000:
					return SamplingRate.Sampling16000;
				case 48000:
					return SamplingRate.Sampling48000;
				default:
					throw new NotImplementedException();
			}
		}
	}

	private Channels Channel => (Channels)microphoneService.Channels;
}

