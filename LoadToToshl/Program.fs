// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Parser
open System
open ToshClient
[<EntryPoint>]
let main argv = 
    let tags = getTags()
    let categories = getCategories();
    let setEntry = setEntry 1.6 (DateTime(2016,05,09,9,0,0)) "caffe" 
    Console.ReadLine() |> ignore
    0 // return an integer exit code
