// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open Parser
open System
open ToshClient
[<EntryPoint>]
let main argv = 
    let movimenti = loadMovimenti "\\"
    for m in movimenti do 
        match m with 
        | Types.Movement.Expence x -> printf "Ex %s" x.Description
        | Types.Movement.Income x -> printf "In %s" x.Description
    addRule "asdf" "desc"

    init()
    let tags = getTags()
    let categories = getCategories();
    let accounts = getAccounts();

    let entry = 
        { amount = -1.
          currency = { code = "EUR" }
          date = DateTime.Now.ToString("yyyy-MM-dd")
          desc = ""
          account = "13106"
          category = "241531"
          tags = [ "17929762" ]
          completed = true }
    let setEntry = setEntry entry
    Console.ReadLine() |> ignore
    0 // return an integer exit code
