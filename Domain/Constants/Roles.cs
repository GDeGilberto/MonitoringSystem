namespace Domain.Constants
{
    public static class Roles
    {
        public const string Admin = "Administrador";
        public const string User = "Usuario";

        public static readonly List<string> AllRoles = new List<string> { User, Admin };
    }
}
