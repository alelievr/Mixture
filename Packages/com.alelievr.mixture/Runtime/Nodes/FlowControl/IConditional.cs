namespace Mixture
{
    public interface ILoopStart
    {
        void PrepareLoopStart();
        bool IsLastIteration();
    }

    public interface ILoopEnd
    {
        /// <summary>
        /// Function executed the first time a Loop end is encountered. I.e. at the end of the first loop iteration
        /// </summary>
        void PrepareLoopEnd(ILoopStart loopStartNode);

        void FinalIteration();
    }

	public interface INeedLoopReset
	{
		void PrepareNewIteration();
	}
}