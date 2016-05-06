// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Parser
open System
[<EntryPoint>]
let main argv = 
    loadMovimenti()
    Console.ReadLine() |> ignore
    0 // return an integer exit code
