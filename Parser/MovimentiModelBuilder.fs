module MovimentiModelBuilder
open System
open Types
open Parser
open DataAccessLayer
 
open SharedTypes

let private movimentiVm path file getIgnored = 
    let toVm (y:Types.Record) t = 
        let hash =
            let asStr = String.concat "" [y.Date.ToString(); y.Ammount.ToString(); y.Description]
            MovementSaver.getHash(asStr)
        { 
            Ammount = y.Ammount; 
            Date = y.Date; 
            Description = y.Description; 
            Causale = y.Causale; 
            Type = t ;
            Tagged = not (y.Tag = 0);
            Tag = y.Tag
            Hash = hash
            Category = 0
            Account = 0
        } 
    movimentiParser path file getIgnored
    |> Seq.map (fun x -> 
        match x with
        | Movement.Expence y -> toVm y CatType.Expence
        | Movement.Income y ->  toVm y CatType.Income)

let Movimenti path file=
    movimentiVm path file false
    |> System.Collections.Generic.List 

let Ignorati path file=
    movimentiVm path file true
    |> System.Collections.Generic.List 
