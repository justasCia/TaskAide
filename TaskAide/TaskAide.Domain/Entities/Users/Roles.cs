namespace TaskAide.Domain.Entities.Users
{
    public static class Roles
    {
        public const string Admin = nameof(Admin);
        public const string Client = nameof(Client);
        public const string Provider = nameof(Provider);
        public const string Company = nameof(Company);
        public const string CompanyWorker = nameof(CompanyWorker);

        public static readonly IReadOnlyCollection<string> All = new[] { Admin, Client, Provider, Company, CompanyWorker };
    }
}
