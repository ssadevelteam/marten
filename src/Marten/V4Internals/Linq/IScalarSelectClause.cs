namespace Marten.V4Internals.Linq
{
    public interface IScalarSelectClause
    {
        void ApplyOperator(string op);
        ISelectClause CloneToDouble();
    }
}
