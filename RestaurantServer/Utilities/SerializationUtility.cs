using Newtonsoft.Json;
using RestaurantLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RestaurantServer.Utilities
{
    internal static class SerializationUtility
    {
        internal static List<Dish> ReadDishes()
        {
            string filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Data\dishes.json";
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    return JsonConvert.DeserializeObject<List<Dish>>(reader.ReadToEnd());
                }
            }
            else
            {
                return new List<Dish>();
            }
        }
    }
}
