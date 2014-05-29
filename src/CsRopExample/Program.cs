using System;

namespace CsRopExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Start OWIN host 
            const string baseAddress = "http://localhost:9000/";
            using (var app = Microsoft.Owin.Hosting.WebApp.Start<Startup>(baseAddress))
            {
                Console.WriteLine("Listening at {0}",baseAddress);
                Console.WriteLine("Press any key to stop");

                //wait
                Console.ReadLine();
            }
        }
    }
}
