module MovimentiModelBuilder
open System
open Types
open Parser
open DataAccessLayer
 
open SharedTypes

let private movimentiVm path file getIgnored csvType = 
    let toVm (y:Types.Record) t = 
        let hash =
            let asStr = String.concat "" [y.Date.ToString("yyyy-MM-dd"); y.Ammount.ToString(); y.Description]
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
    movimentiParser path file getIgnored csvType
    |> Seq.map (fun x -> 
        match x with
        | Movement.Expence y -> toVm y CatType.Expence
        | Movement.Income y ->  toVm y CatType.Income)

let Movimenti path file csvType=
    movimentiVm path file false csvType
    |> System.Collections.Generic.List 

let Ignorati path file csvType=
    movimentiVm path file true csvType
    |> System.Collections.Generic.List 
