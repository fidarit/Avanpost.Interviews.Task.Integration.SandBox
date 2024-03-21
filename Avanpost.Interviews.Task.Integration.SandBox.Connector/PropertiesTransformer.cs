using Avanpost.Interviews.Task.Integration.Data.DbCommon.DbModels;
using Avanpost.Interviews.Task.Integration.Data.Models.Models;

namespace Avanpost.Interviews.Task.Integration.SandBox.Connector
{
    internal static class PropertiesTransformer
    {
        public static Property[] GetPropertyDescriptions()
        {
            return new Property[]
            {
                //new(nameof(User.Login),        "Логин"),
                new(nameof(User.FirstName),    "Имя"),
                new(nameof(User.MiddleName),   "Отчество"), //Возможно ошибся
                new(nameof(User.LastName),     "Фамиллия"),
                new(nameof(User.TelephoneNumber), "Телефонный номер"),
                new(nameof(User.IsLead),       "Руководитель"),
                //new(nameof(Sequrity.Password), "Пароль"),
            };
        }

        public static UserProperty[] GetUserProperties(User user, Sequrity sequrity)
        {
            return new UserProperty[]
            {
                //new(nameof(User.Login),        user.Login),
                new(nameof(User.FirstName),    user.FirstName),
                new(nameof(User.MiddleName),   user.MiddleName),
                new(nameof(User.LastName),     user.LastName),
                new(nameof(User.TelephoneNumber), user.TelephoneNumber),
                new(nameof(User.IsLead),       user.IsLead.ToString()),
                //new(nameof(Sequrity.Password), sequrity.Password),
            };
        }

        public static void SetUserProperties(UserToCreate userData, ref User user, ref Sequrity sequrity)
        {
            user.Login      = userData.Login;
            user.IsLead     = userData.Properties.GetBoolOrNull(nameof(User.IsLead))         ?? false;
            user.FirstName  = userData.Properties.GetStringOrNull(nameof(User.FirstName))    ?? string.Empty; //В предоставленной модели БД эти столбцы NOT NUL
            user.MiddleName = userData.Properties.GetStringOrNull(nameof(User.MiddleName))   ?? string.Empty; //Хотя в readme говорится обратное
            user.LastName   = userData.Properties.GetStringOrNull(nameof(User.LastName))     ?? string.Empty;
            user.TelephoneNumber = userData.Properties.GetStringOrNull(nameof(User.TelephoneNumber)) ?? string.Empty;

            sequrity.UserId     = userData.Login;
            sequrity.Password   = userData.HashPassword;
        }

        public static void SetUserProperties(IEnumerable<UserProperty> properties, ref User user, ref Sequrity sequrity)
        {
            user.Login      = properties.GetStringOrNull(nameof(User.Login))        ?? user.Login;
            user.IsLead     = properties.GetBoolOrNull(nameof(User.IsLead))         ?? user.IsLead;
            user.FirstName  = properties.GetStringOrNull(nameof(User.FirstName))    ?? user.FirstName; 
            user.MiddleName = properties.GetStringOrNull(nameof(User.MiddleName))   ?? user.MiddleName;
            user.LastName   = properties.GetStringOrNull(nameof(User.LastName))     ?? user.LastName;
            user.TelephoneNumber = properties.GetStringOrNull(nameof(User.TelephoneNumber)) ?? user.TelephoneNumber;

            sequrity.UserId     = properties.GetStringOrNull(nameof(User.Login))        ?? user.Login;
            sequrity.Password   = properties.GetStringOrNull(nameof(Sequrity.Password)) ?? sequrity.Password;
        }
    }
}
