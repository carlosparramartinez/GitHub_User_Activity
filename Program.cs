using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

class Program
{
    static async Task Main(string[] args)
    {
        // Verifica que se haya ingresado al menos el nombre de usuario
        if (args.Length == 0)
        {
            Console.WriteLine("Uso: github-activity <nombre_usuario> [tipo_evento]");
            return;
        }

        string username = args[0];
        // Tipo de evento opcional para filtrar (ej. PushEvent, IssuesEvent)
        string eventTypeFilter = args.Length > 1 ? args[1] : null;

        using var client = new HttpClient();
        // GitHub requiere un User-Agent
        client.DefaultRequestHeaders.UserAgent.ParseAdd("request");

        try
        {
            // Hace la solicitud a la API de GitHub
            var response = await client.GetAsync($"https://api.github.com/users/{username}/events");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: No se pudo obtener la actividad para el usuario {username}.");
                return;
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseBody);
            JsonElement activities = doc.RootElement;

            // Recorre y muestra la actividad
            foreach (var activity in activities.EnumerateArray())
            {
                string type = activity.GetProperty("type").GetString();

                // Si hay filtro y no coincide, omite
                if (eventTypeFilter != null && type != eventTypeFilter)
                    continue;

                string repo = activity.GetProperty("repo").GetProperty("name").GetString();
                string createdAt = activity.GetProperty("created_at").GetString();

                // Traducción de eventos comunes
                string action = type switch
                {
                    "PushEvent" => "Empujó commits a",
                    "IssuesEvent" => "Abrió una incidencia en",
                    "WatchEvent" => "Marcó como favorito",
                    _ => $"Realizó {type} en"
                };

                // Imprime actividad formateada
                Console.WriteLine("📁 Repositorio: " + repo);
                Console.WriteLine("🔧 Acción: " + action);
                Console.WriteLine("🕒 Fecha: " + createdAt);
                Console.WriteLine(new string('-', 30));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ocurrió un error: " + ex.Message);
        }
    }
}