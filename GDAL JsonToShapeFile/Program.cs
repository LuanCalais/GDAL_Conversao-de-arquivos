using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GDAL_JsonToShapeFile
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Teste de Conversão Interna arquivos JSON para Shapefile com GDAL ===");
            Console.WriteLine("\n Entre com qualquer tecla para continuar...");
            Console.ReadKey();
            Console.Clear();

            Console.WriteLine("Entre com o nome do arquivo json que será convertido: ");
            string Json = Console.ReadLine();
            Console.Clear();

            Console.WriteLine("Entre com o nome do arquivo shapefile que será gerado: ");
            string shape = Console.ReadLine();
            Console.Clear();

            bool verifica = new GdalUtilities().convertJsonToShapeFile(@"C:\Users\luanc\Conversão\" + Json + ".json", @"C:\Users\luanc\Conversão\Shapes\" + shape + ".shp");

            if (verifica == false)
            {
                Console.WriteLine("A operação fracassou!");
            }
            else
            {
                Console.WriteLine("Operação bem sucedida");
            }
        }
    }
}
