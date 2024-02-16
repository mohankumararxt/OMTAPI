namespace OMT.DataAccess.Context
{
    public interface IDbInitializer
    {
        void Initialize();
        void SeedDatabase();
    }
}
