module Parser 
open System
open Types
open System.Globalization
open System.IO


let movimentiParser path (file:Stream) getIgnored=
    file.Position <- 0L
    let it = CultureInfo.CreateSpecificCulture("it-IT")
    Threading.Thread.CurrentThread.CurrentCulture <- it
    let clean (str:string) = 
        String.Join(" ", str.Split([|' '|],StringSplitOptions.RemoveEmptyEntries))
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
            Description = clean x.``DESCRIZIONE OPERAZIONE`` 
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

        let first = movimentiCsv.Rows |> Seq.head
        let alreadyLoaded= 
            DataAccessLayer.MovementSaver.getMonth (first.DATA)
            |> Seq.map DataAccessLayer.toRecord

        let WasLoaded (x:EstrattoConto.Row) =
            let desc = clean x.``DESCRIZIONE OPERAZIONE``
            alreadyLoaded 
            |> Seq.tryFind (fun y -> 
                    y.Description = desc &&
                    if y.Ammount < 0.0 then y.Ammount = -x.DARE else y.Ammount = x.AVERE)
            |> Option.isSome
        
        movimentiCsv.Rows 
        |> Seq.filter (fun x -> shouldGet (clean x.``DESCRIZIONE OPERAZIONE``))
        |> Seq.filter (fun x -> not <| WasLoaded x)
        |> Seq.map (fun x -> x |> if isExpence x then getExpence else getIncome)
        
    movimenti


