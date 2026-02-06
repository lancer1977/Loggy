namespace PolyhydraGames.Loggy
{
    public readonly struct LoggyId
    {
        public Guid Value { get; }

        public LoggyId(Guid value) => Value = value;
    }
}
