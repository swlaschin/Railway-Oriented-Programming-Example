// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open System
open FsRopExample

[<EntryPoint>]
let main _ = 
    let baseAddress = "http://localhost:9001/"
    use app = Microsoft.Owin.Hosting.WebApp.Start<Startup>(baseAddress)

    Console.WriteLine("Listening at {0}",baseAddress)
    Console.WriteLine("Press any key to stop")

    //wait
    Console.ReadLine() |> ignore

    // exit with 0
    0
