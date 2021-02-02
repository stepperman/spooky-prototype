namespace QTea
{
	public interface IPCMBuffer
	{
		float[] PopSamples(uint count);
		void PushSamples(float[] samples);
		int Count { get; }
	}
}
