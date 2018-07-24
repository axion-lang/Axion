using Axion;

namespace Wrapper {
    internal static class Program {
        public static void Main(string[] args) {
            //try {
            Compiler.Launch(args);
            //}
            //catch(Exception ex) {
            //   Console.WriteLine("Error: " + ex.Message);
            //   Console.WriteLine("Press any key to close app.");
            //   Console.ReadKey();
            //}
        }
    }
}