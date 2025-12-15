using System.IO;
using System.IO.Pipes;
using System;
using System.Text;
using System.Threading.Tasks;

class Program
{
    private const string PipeName = "CSharpPipe";
    static void Main(string[] args)
    {
        Console.WriteLine("--- Servidor de Comandos via Named Pipes ---");
        Console.WriteLine($"Esperando conexiones en el pipe: '{PipeName}'");
        Console.WriteLine("Presiona Ctrl+C para salir.");
    }

}
