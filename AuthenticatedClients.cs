using System.Collections.Generic;

namespace Curs
{
    public static class AuthenticatedClients
    {
        private static List<int> authenticatedClientIds = new List<int>();

        // Метод для добавления ID авторизовавшегося клиента
        public static void AddClientId(int clientId)
        {
            if (!authenticatedClientIds.Contains(clientId))
            {
                authenticatedClientIds.Add(clientId);
            }
        }

        // Метод для получения списка ID всех авторизовавшихся клиентов
        public static List<int> GetAuthenticatedClientIds()
        {
            return new List<int>(authenticatedClientIds);
        }

        // Метод для проверки, авторизован ли клиент
        public static bool IsClientAuthenticated(int clientId)
        {
            return authenticatedClientIds.Contains(clientId);
        }

        // Метод для очистки списка авторизовавшихся клиентов
        public static void ClearAuthenticatedClientIds()
        {
            authenticatedClientIds.Clear();
        }
    }
}
