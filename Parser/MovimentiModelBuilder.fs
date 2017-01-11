module MovimentiModelBuilder
open System
open Types
open Parser

type CatType =
    | Expence
    | Income

type ReportVm ={
    Ammount : double
    Date: DateTime
    Description: string
    Causale: int 
    Type: CatType
    Tagged: bool
    Tag:int}

type RuleType = 
    | Ignore = 0
    | Tagged = 1

let private movimentiVm path file getIgnored = 
    let toVm (y:Record) t = { 
        Ammount = y.Ammount; 
        Date = y.Date; 
        Description = y.Description; 
        Causale = y.Causale; 
        Type = t ;
        Tagged = not (y.Tag = 0);
        Tag = y.Tag} 
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
