namespace FurCoNZ.Services
{
    public interface IPaymentProvider
    {
        string Name { get; }
        string SupportedMethods { get; }
        string Description { get; }
    }
}