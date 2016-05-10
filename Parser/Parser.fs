module Parser

open System
open Types
open System.Globalization

let loadMovimenti path=
    let it = CultureInfo.CreateSpecificCulture("it-IT")
    Threading.Thread.CurrentThread.CurrentCulture <- it
    let movimentiCsv = EstrattoConto.Load("ListaMovimenti.csv")
    let record (x:EstrattoConto.Row) y= {
        Date = DateTime.Parse(x.DATA);
        Ammount = y; 
        Causale =(if x.``CAUSALE ABI``.HasValue then x.``CAUSALE ABI``.Value else 0); 
        Description = x.``DESCRIZIONE OPERAZIONE``
        }
    let getExpence (x:EstrattoConto.Row) = 
        Expence <| record x x.DARE
    let getIncome (x:EstrattoConto.Row) = 
        Income <| record x x.AVERE
    let isExpence (x:EstrattoConto.Row) =
        not <| Double.IsNaN x.DARE
    let ignored = 
        Ignored.Load(path + "MappingRules.csv") 
        |>( fun x -> x.Rows |> Seq.filter (fun y -> y.Rule = "ignore" ))

    let movimenti = 
        let isIgnored (s:string) = ignored |> Seq.fold (fun acc x -> acc || s.StartsWith x.Starts) false
        let isSaldo s =  s = "Saldo iniziale" || s = "Saldo contabile"
        let shouldSkip s = isSaldo s || isIgnored s

        movimentiCsv.Rows 
        |> Seq.filter (fun x -> not <| shouldSkip x.``DESCRIZIONE OPERAZIONE``) 
        |> Seq.map (fun x -> x |> if isExpence x then getExpence else getIncome)

//    for row in movimenti do
//        match row with 
//        | Expence x -> printf "Expence\t%s\t%.2f\t%d\t%s \n" (x.Date.ToShortDateString()) x.Ammount x.Causale x.Description
//        | Income x -> printf "Income\t%s\t%.2f\t%d\t%s \n" (x.Date.ToShortDateString()) x.Ammount x.Causale x.Description
//    
    movimenti

type CatType =
    | Expence
    | Income
type ReportVm =
    {Ammount : double; Date: DateTime; Description: string; Causale: int; Type: CatType}

let Movimenti (path)=
    let toVm (y:Record) t = { Ammount = y.Ammount; Date = y.Date; Description = y.Description; Causale = y.Causale; Type = t  } 
    loadMovimenti path
    |> Seq.map (fun x -> 
        match x with
        | Movement.Expence y -> toVm y CatType.Expence
        | Movement.Income y ->  toVm y CatType.Income)


type RuleType = 
    | Ignore = 0
    | Tagged = 1
let addGenericRule rule tagId startString path=
    let ruleFile = path + "MappingRules.csv"
    let newRow = [Ignored.Row(rule,tagId,startString)]
    let newRules = Ignored.Load(ruleFile).Append newRow
    System.IO.File.WriteAllText (ruleFile, newRules.SaveToString(';'))

let addIgnore startString path=
    addGenericRule "ignore" "" startString path

let addRule tagId startString path=
    addGenericRule "tag" tagId startString path