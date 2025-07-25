namespace Web.UseCases.GetRandomNumber
{
    internal sealed record GetRandomNumberQuery(long MinValue, long MaxValue) : IQuery<long>;
}
