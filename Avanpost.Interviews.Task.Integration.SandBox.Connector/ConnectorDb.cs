using Avanpost.Interviews.Task.Integration.Data.DbCommon.DbModels;
using Avanpost.Interviews.Task.Integration.Data.Models;
using Avanpost.Interviews.Task.Integration.Data.Models.Models;
using Avanpost.Interviews.Task.Integration.SandBox.Connector.Database;
using System.Data;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector
{
    public class ConnectorDb : IConnector
    {
        private const string RequestRightGroupName = "Request";
        private const string ItRoleRightGroupName = "Role";
        private const string Delimeter = ":";

        private Dictionary<string, string> connectionParams;
        private DbContextFactory contextFactory;

        public ILogger Logger { get; set; }

        public ConnectorDb()
        {
            
        }

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

            using var dataContext = contextFactory.GetContext();
            var rights = dataContext.RequestRights.ToList();
            var roles = dataContext.ITRoles.ToList();

            return rights
                .Select(t => new Permission($"{t.Id}", t.Name, string.Empty))
                .Union(roles.Select(t => new Permission($"{t.Id}", t.Name, t.CorporatePhoneNumber)));
        }

        public IEnumerable<string> GetUserPermissions(string userLogin)
        {
            Logger.Debug($"Получение прав пользователя: {userLogin}");
            
            using var dataContext = contextFactory.GetContext();
            dataContext.CheckUserExists(userLogin);

            var userRights = (from right in dataContext.RequestRights
                              join rightLink in dataContext.UserRequestRights on right.Id equals rightLink.RightId
                              where rightLink.UserId == userLogin
                              select right.Name).ToList();

            var userRoles = (from role in dataContext.ITRoles
                             join roleLink in dataContext.UserITRoles on role.Id equals roleLink.RoleId
                             where roleLink.UserId == userLogin
                             select role.Name).ToList();

            return userRights.Union(userRoles);
        }

        public void AddUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Добавление прав пользователю: {userLogin}");

            using var dataContext = contextFactory.GetContext();
            dataContext.CheckUserExists(userLogin);

            foreach (var rightId in rightIds)
            {
                (var rightType, var rightIdInt) = ParseRightId(rightId);


                if (rightType == RequestRightGroupName)
                {
                    if (dataContext.RequestRights.Any(t => t.Id == rightIdInt))
                    {
                        if (dataContext.UserRequestRights.Any(t => t.RightId == rightIdInt && t.UserId == userLogin))
                            Logger.Warn($"У пользователя {userLogin} уже существует роль: '{rightId}'");
                        else
                            dataContext.UserRequestRights.Add(new UserRequestRight() { RightId = rightIdInt, UserId = userLogin });
                    }
                    else
                        throw new Exception($"Не найдена роль: '{rightId}'");
                }
                else if (rightType == ItRoleRightGroupName)
                {
                    if (dataContext.ITRoles.Any(t => t.Id == rightIdInt))
                    {
                        if (dataContext.UserITRoles.Any(t => t.RoleId == rightIdInt && t.UserId == userLogin))
                            Logger.Warn($"У пользователя {userLogin} уже существует роль: '{rightId}'");
                        else
                            dataContext.UserITRoles.Add(new UserITRole() { RoleId = rightIdInt, UserId = userLogin });
                    }
                    else
                        throw new Exception($"Не найдена роль: '{rightId}'");
                }
                else
                    throw new Exception($"Неизвестный тип роли: {rightType}");
            }

            dataContext.SaveChanges();
        }

        public void RemoveUserPermissions(string userLogin, IEnumerable<string> rightIds)
        {
            Logger.Debug($"Изъятие прав у пользователя: {userLogin}");

            using var dataContext = contextFactory.GetContext();

            foreach (var rightId in rightIds)
            {
                (var rightType, var rightIdInt) = ParseRightId(rightId);

                if(rightType == RequestRightGroupName)
                {
                    var right = dataContext.UserRequestRights.FirstOrDefault(t => t.RightId == rightIdInt && t.UserId == userLogin);
                    if (right != null)
                        dataContext.UserRequestRights.Remove(right);
                    else
                        Logger.Warn($"У пользователя {userLogin} не найдена роль: '{rightId}'");
                }
                else if(rightType == ItRoleRightGroupName)
                {
                    var role = dataContext.UserITRoles.FirstOrDefault(t => t.RoleId == rightIdInt && t.UserId == userLogin);
                    if (role != null)
                        dataContext.UserITRoles.Remove(role);
                    else
                        Logger.Warn($"У пользователя {userLogin} не найдена роль: '{rightId}'");
                }
                else
                    throw new Exception($"Неизвестный тип роли: {rightType}");
            }

            dataContext.SaveChanges();
        }

        private (string rightType, int rightIdInt) ParseRightId(string rightIdText)
        {
            var parts = rightIdText.Split(Delimeter);
            if (parts.Length < 2)
                throw new Exception($"Неподходящая сигнатура параметра rightId: {rightIdText}");

            var rightType = parts[0];
            if (rightType != RequestRightGroupName && rightType != ItRoleRightGroupName)
                throw new Exception($"Неизвестный тип роли: {rightType}");


            rightIdText = parts[1];
            if (!int.TryParse(rightIdText, out var rightIdInt))
                throw new Exception($"rightId '{rightIdText}' не является числом");

            return (rightType, rightIdInt);
        }
        #endregion
    }
}