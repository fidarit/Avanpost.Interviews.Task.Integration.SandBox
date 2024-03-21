using Avanpost.Interviews.Task.Integration.Data.DbCommon.DbModels;
using Avanpost.Interviews.Task.Integration.Data.Models;
using Avanpost.Interviews.Task.Integration.Data.Models.Models;
using Avanpost.Interviews.Task.Integration.SandBox.Connector.Database;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector
{
    public class ConnectorDb : IConnector
    {
        private Dictionary<string, string> connectionParams;
        private DbContextFactory contextFactory;

        public ILogger Logger { get; set; }

        public void StartUp(string connectionString)
        {
            connectionParams = ConnectionStringExtractor.ParseConnectionString(connectionString);
            contextFactory = DbContextFactory.Create(connectionParams);
        }

        public void CreateUser(UserToCreate user)
        {
            Logger.Debug($"Создание пользователя: {user.Login}");

            using var dataContext = contextFactory.GetContext();
            var password = new Sequrity();
            var newUser = new User();

            PropertiesTransformer.SetUserProperties(user, ref newUser, ref password);

            dataContext.Users.Add(newUser);
            dataContext.Passwords.Add(password);
            dataContext.SaveChanges();
        }

        public bool IsUserExists(string userLogin)
        {
            Logger.Debug($"Проверка на существование пользователя: {userLogin}");

            using var dataContext = contextFactory.GetContext();

            return dataContext.Users.Any(t => t.Login == userLogin);
        }

        #region Свойства
        public IEnumerable<Property> GetAllProperties()
        {
            Logger.Debug("Получение всех свойств.");

            return PropertiesTransformer.GetPropertyDescriptions();
        }

        public IEnumerable<UserProperty> GetUserProperties(string userLogin)
        {
            Logger.Debug($"Получение всех свойств пользователя: {userLogin}");

            using var dataContext = contextFactory.GetContext();
            var user = dataContext.Users.FirstOrDefault(t => t.Login == userLogin) ?? throw new Exception($"Пользователь не найден: {userLogin}");
            var password = dataContext.Passwords.Single(u => u.UserId == userLogin);

            return PropertiesTransformer.GetUserProperties(user, password);
        }

        public void UpdateUserProperties(IEnumerable<UserProperty> properties, string userLogin)
        {
            Logger.Debug($"Обновление свойств пользователя: {userLogin}");

            using var dataContext = contextFactory.GetContext();
            var user = dataContext.Users.FirstOrDefault(t => t.Login == userLogin) ?? throw new Exception($"Пользователь не найден: {userLogin}");
            var password = dataContext.Passwords.Single(u => u.UserId == userLogin);

            PropertiesTransformer.SetUserProperties(properties, ref user, ref password);

            dataContext.SaveChanges();
        }
        #endregion

        #region Права
        public IEnumerable<Permission> GetAllPermissions()
        {
            Logger.Debug("Получение списка всех прав.");

            throw new NotImplementedException();
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger.Debug($"Получение прав пользователя: {userLogin}");

            throw new NotImplementedException();
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Добавление прав пользователю: {userLogin}");

            throw new NotImplementedException();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Изъятие прав у пользователя: {userLogin}");

            throw new NotImplementedException();
        }
        #endregion
    }
}