namespace Learnly.Application.Interfaces
{
    public interface IBuscaMaterialService
    {
        Task<string> ResolverUrlAsync(string termo);
    }
}
