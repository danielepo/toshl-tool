module Parser

open System
open Types
open System.Globalization
open System.IO


let loadMovimenti path (file:Stream) getIgnored=
    file.Position <- 0L
    let it = CultureInfo.CreateSpecificCulture("it-IT")
    Threading.Thread.CurrentThread.CurrentCulture <- it

    let rules str = Ignored.Load(path + "MappingRules.csv")  |>( fun x -> x.Rows |> Seq.filter (fun y -> y.Rule = str ))

    let movimentiCsv = EstrattoConto.Load(file)
    let record (x:EstrattoConto.Row) y= 
        let tag = 
            let rulesThatBegins =
                rules "tag" 
                |> Seq.filter (fun r -> r.Rule = "tag" && x.``DESCRIZIONE OPERAZIONE``.StartsWith(r.Starts))
            if Seq.isEmpty rulesThatBegins then 0 else Int32.Parse((Seq.head rulesThatBegins).TagId)
        let isCausale, causale = Int32.TryParse x.``CAUSALE ABI``
        {
            Date = x.DATA
            Ammount = y
            Causale = if isCausale then causale else 0
            Description = x.``DESCRIZIONE OPERAZIONE``
            Tag = tag
        }

    let getExpence (x:EstrattoConto.Row) = Expence <| record x x.DARE
    let getIncome (x:EstrattoConto.Row) = Income <| record x x.AVERE
    let isExpence (x:EstrattoConto.Row) = Double.IsNaN x.AVERE

    let movimenti = 
        let ignored = rules "ignore"
        let isIgnored (s:string) = ignored |> Seq.fold (fun acc x -> acc || s.StartsWith x.Starts) false
        let isSaldo s =  s = "Saldo iniziale" || s = "Saldo contabile"

        let shouldGet s = 
            let (!!!) x = if getIgnored then x else not x
            not (isSaldo s) && !!! (isIgnored s)
        
        

        movimentiCsv.Rows 
        |> Seq.filter (fun x -> shouldGet x.``DESCRIZIONE OPERAZIONE``)
        |> Seq.map (fun x -> x |> if isExpence x then getExpence else getIncome)

    movimenti

type CatType =
    | Expence
    | Income

type ReportVm ={
    Ammount : double; 
    Date: DateTime; 
    Description: string; 
    Causale: int; 
    Type: CatType; 
    Tagged: bool;
    Tag:int}

type RuleType = 
    | Ignore = 0
    | Tagged = 1

let movimentiVm path file getIgnored = 
    let toVm (y:Record) t = { 
        Ammount = y.Ammount; 
        Date = y.Date; 
        Description = y.Description; 
        Causale = y.Causale; 
        Type = t ;
        Tagged = not (y.Tag = 0);
        Tag = y.Tag} 
    loadMovimenti path file getIgnored
    |> Seq.map (fun x -> 
        match x with
        | Movement.Expence y -> toVm y CatType.Expence
        | Movement.Income y ->  toVm y CatType.Income)

let Movimenti path file=
    movimentiVm path file false

let Ignorati path file=
     movimentiVm path file true

let addGenericRule rule tagId startString path=
    let ruleFile = path + "MappingRules.csv"
    let newRow = [Ignored.Row(rule,tagId,startString)]
    let newRules = Ignored.Load(ruleFile).Append newRow
    System.IO.File.WriteAllText (ruleFile, newRules.SaveToString(';'))

let addIgnore startString path=
    addGenericRule "ignore" "" startString path

let addRule tagId startString path=
    addGenericRule "tag" tagId startString path