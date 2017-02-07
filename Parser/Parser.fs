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

        let toAndFrom = 
            let getMaxAndMin (xs:'a seq)= 
                Seq.min xs,Seq.max xs

            movimentiCsv.Rows 
            |> Seq.map (fun x -> x.DATA)
            |> getMaxAndMin 

        let alreadyLoaded = 
            System.Diagnostics.Trace.WriteLine(sprintf "f: %A, S: %A" (fst toAndFrom) (snd toAndFrom))
            ToshClient.getEntries (fst toAndFrom) (snd toAndFrom)
            |> List.map 
                (fun x-> 
                    let descParts = x.desc.Split('\t')
                    if descParts.Length = 2 then Some descParts.[1] else None)

        let hash (x:EstrattoConto.Row) =
                let record = if isExpence x then getExpence x else getIncome x
                let asStr = 
                    match record with 
                    | Income i -> 
                        String.concat "" [i.Date.ToString(); i.Ammount.ToString(); i.Description]
                    | Expence i -> 
                        String.concat "" [i.Date.ToString(); i.Ammount.ToString(); i.Description]
                let hash = DataAccessLayer.MovementSaver.getHash(asStr)
                System.Diagnostics.Trace.WriteLine(asStr+"\t"+ hash)
                hash

        let WasLoaded (x:EstrattoConto.Row) =
            
            alreadyLoaded 
            |> List.tryFind (fun y -> 
                match y with 
                | Some r -> r = hash x
                | None -> false)
            |> Option.isSome
        
        System.Diagnostics.Trace.WriteLine("Dati Gia caricati")
        alreadyLoaded
        |> List.iter (fun x -> 
            match x with
            | Some s -> System.Diagnostics.Trace.WriteLine(s)
            | None -> System.Diagnostics.Trace.WriteLine("NONE"))
        
        System.Diagnostics.Trace.WriteLine("Dati Letti")
        movimentiCsv.Rows 
        |> Seq.iter (fun x -> hash x |> ignore)

        movimentiCsv.Rows 
        |> Seq.filter (fun x -> shouldGet (clean x.``DESCRIZIONE OPERAZIONE``))
        |> Seq.filter (fun x -> not <| WasLoaded x)
        |> Seq.map (fun x -> x |> if isExpence x then getExpence else getIncome)
        
    movimenti


