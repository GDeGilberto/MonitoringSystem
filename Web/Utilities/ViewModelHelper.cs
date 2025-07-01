using System.ComponentModel;
using System.Reflection;

namespace Web.Utilities
{
    /// <summary>
    /// Clase utilitaria para trabajar con ViewModels usando reflexión
    /// </summary>
    public static class ViewModelHelper
    {
        /// <summary>
        /// Obtiene los títulos de las propiedades de un ViewModel usando los atributos DisplayName
        /// </summary>
        /// <typeparam name="T">Tipo del ViewModel</typeparam>
        /// <returns>Colección de títulos extraídos de los atributos DisplayName</returns>
        public static IEnumerable<string> GetTitlesFromViewModel<T>()
        {
            return typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .Select(GetDisplayName);
        }

        /// <summary>
        /// Obtiene las propiedades públicas de un tipo que pueden ser leídas
        /// </summary>
        /// <typeparam name="T">Tipo del cual obtener las propiedades</typeparam>
        /// <returns>Colección de PropertyInfo</returns>
        public static IEnumerable<PropertyInfo> GetProperties<T>()
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                           .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);
        }

        /// <summary>
        /// Obtiene el nombre de visualización de una propiedad usando el atributo DisplayName
        /// </summary>
        /// <param name="property">PropertyInfo de la propiedad</param>
        /// <returns>DisplayName si existe, otherwise el nombre de la propiedad</returns>
        public static string GetDisplayName(PropertyInfo property)
        {
            var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();
            return displayNameAttribute?.DisplayName ?? property.Name;
        }

        /// <summary>
        /// Obtiene los valores de las propiedades de un objeto como strings
        /// </summary>
        /// <typeparam name="T">Tipo del objeto</typeparam>
        /// <param name="obj">Objeto del cual extraer los valores</param>
        /// <returns>Colección de valores como strings</returns>
        public static IEnumerable<string> GetPropertyValues<T>(T obj)
        {
            if (obj == null) yield break;

            foreach (var property in GetProperties<T>())
            {
                var value = property.GetValue(obj);
                yield return value?.ToString() ?? string.Empty;
            }
        }
    }
}