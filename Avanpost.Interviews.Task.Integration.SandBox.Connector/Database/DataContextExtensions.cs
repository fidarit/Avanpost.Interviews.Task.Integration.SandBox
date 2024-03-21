namespace Avanpost.Interviews.Task.Integration.SandBox.Connector.Database
{
    internal static class DataContextExtensions
    {
        public static void CheckUserExists(this DbContext context, string userLogin)
        {
            if (!context.Users.Any(t => t.Login == userLogin))
                throw new Exception($"Пользователь не существует: {userLogin}");
        }
    }
}
