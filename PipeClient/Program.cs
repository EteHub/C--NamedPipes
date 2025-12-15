using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

public class PipeClient
{
    private const string PipeName = "CSharpPipe"; // ¡Debe ser el mismo nombre que en el servidor!

    public static async Task Main(string[] args)
    {
        Console.WriteLine("--- Cliente de Comandos via Named Pipes ---");

        while (true)
        {
            Console.Write("\nEscribe un comando (o 'exit' para salir): ");
            string? command = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(command))
            {
                continue;
            }

            if (command.ToLowerInvariant() == "exit")
            {
                break;
            }

            try
            {
                // 1. Crear el cliente y conectar al servidor.
                await using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

                Console.WriteLine("Conectando al servidor...");
                await client.ConnectAsync(5000); // Timeout de 5 segundos
                Console.WriteLine("¡Conectado!");

                // 2. Enviar el comando.
                // CAMBIO: Se elimina { AutoFlush = true }
                await using var writer = new StreamWriter(client, Encoding.UTF8);
                await writer.WriteLineAsync(command);
                await writer.FlushAsync(); // <-- CAMBIO AÑADIDO: Forzar el envío del comando.

                // 3. Leer la respuesta.
                using var reader = new StreamReader(client, Encoding.UTF8);
                string? response = await reader.ReadLineAsync();

                Console.WriteLine($"Respuesta del servidor: {response}");
            }
            catch (TimeoutException)
            {
                Console.WriteLine("ERROR: No se pudo conectar al servidor. ¿Está en ejecución?");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"ERROR de comunicación: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR inesperado: {ex.Message}");
            }
        }

        Console.WriteLine("Cliente finalizado.");
    }
}