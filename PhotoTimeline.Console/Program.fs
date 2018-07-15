// Learn more about F# at http://fsharp.org

open System
open PhotoTimeline

[<EntryPoint>]
let main _ =
    let sourcefiles = [| "C:\\Temp"; "C:\\Temp\\Store" |];

    TimelineCreator.processFolder sourcefiles "C:\\Temp\\Timeline" |> ignore

    Console.WriteLine "Done, press a key to exit"
    Console.ReadKey |> ignore
    0 // return an integer exit code
