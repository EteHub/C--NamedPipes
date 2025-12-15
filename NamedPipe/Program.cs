using System.IO;
using System.IO.Pipes;
using System;
using System.Text;
using System.Threading.Tasks;

class Program
{
    private const string PipeName = "CSharpPipe";
    static async Task Main(string[] args)
    {
        Console.WriteLine("--- Servidor de Comandos via Named Pipes ---");
        Console.WriteLine($"Esperando conexiones en el pipe: '{PipeName}'");
        Console.WriteLine("Presiona Ctrl+C para salir.");
        while (true)
        {
            try
            {
                await using var server = new NamedPipeServerStream(
                PipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);
                Console.WriteLine("\nEsperando cliente...");
                await server.WaitForConnectionAsync();
                Console.WriteLine("\nCliente Conectado");

                await HandleClientCommunicationAsync(server);

                Console.WriteLine("Cliente desconectado.");

            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error de E/S: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado en el servidor: {ex.Message}");
            }
        }
    }
    private static async Task HandleClientCommunicationAsync(NamedPipeServerStream server)
    {
        try
        {
            
            await using var writer = new StreamWriter(server, Encoding.UTF8) { AutoFlush = true };
            using var reader = new StreamReader(server, Encoding.UTF8);

            
            string? commandFromClient = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(commandFromClient))
            {
                Console.WriteLine("El cliente envió un comando vacío.");
                await writer.WriteLineAsync("ERROR: Comando vacío.");
                return;
            }

            Console.WriteLine($"Comando recibido: '{commandFromClient}'");

            
            string response = ProcessCommand(commandFromClient);

            
            await writer.WriteLineAsync(response);
            Console.WriteLine($"Respuesta enviada: '{response}'");
        }
        catch (Exception ex)
        {
            
            try
            {
                await using var writer = new StreamWriter(server, Encoding.UTF8) { AutoFlush = true };
                await writer.WriteLineAsync($"ERROR: Fallo interno del servidor - {ex.Message}");
            }
            catch
            {
                
                Console.WriteLine("No se pudo enviar el error al cliente (probablemente se desconectó).");
            }
        }
    }
    private static string ProcessCommand(string command)
    {
        try
        {
            
            var parts = command.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            var action = parts[0].ToLowerInvariant();
            var parameters = parts.Length > 1 ? parts[1] : string.Empty;

            
            return action switch
            {
                "saludar" => $"OK: ¡Hola, {(!string.IsNullOrEmpty(parameters) ? parameters : "mundo")}!",
                "sumar" => Sumar(parameters),
                "hora" => $"OK: La hora actual es {DateTime.Now:HH:mm:ss}",
                "salir" => "OK: Servidor finalizando.", 
                _ => $"ERROR: Comando '{action}' no reconocido."
            };
        }
        catch (Exception ex)
        {
            
            return $"ERROR: {ex.Message}";
        }
    }

    private static string Sumar(string parameters)
    {
        var numberParts = parameters.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (numberParts.Length != 2)
        {
            throw new ArgumentException("El comando 'sumar' requiere exactamente dos números separados por un espacio.");
        }

        if (int.TryParse(numberParts[0], out int num1) && int.TryParse(numberParts[1], out int num2))
        {
            int result = num1 + num2;
            return $"OK: El resultado de {num1} + {num2} es {result}.";
        }
        else
        {
            throw new FormatException("Ambos parámetros para 'sumar' deben ser números enteros válidos.");
        }
    }
}
