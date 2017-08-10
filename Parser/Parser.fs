module Parser 
open System
open Types
open System.Globalization
open System.IO

let setCulture cult = 
    let it = System.Globalization.CultureInfo.CreateSpecificCulture(cult)
    System.Threading.Thread.CurrentThread.CurrentCulture <- it
    System.Threading.Thread.CurrentThread.CurrentUICulture <- it
    System.Diagnostics.Trace.WriteLine System.Threading.Thread.CurrentThread.CurrentCulture.DisplayName


let movimentiParser path (file:Stream) getIgnored csvType=
    file.Position <- 0L
    setCulture "it-IT"
    let clean (str:string) = 
        String.Join(" ", str.Split([|' '|],StringSplitOptions.RemoveEmptyEntries))
    let rules str = Ignored.Load(path + "MappingRules.csv")  |>( fun x -> x.Rows |> Seq.filter (fun y -> y.Rule = str ))

    let movimentiCsv =
        let loadContoCorrente() = 
            let mov = EstrattoConto.Load(file)
            let bancaToGen (x:EstrattoConto.Row) = 
                let isCausale, causale = Int32.TryParse x.``CAUSALE ABI``
                {
                    Data = x.VALUTA
                    Dare= x.DARE
                    Avere = x.AVERE
                    Descrizione = x.``DESCRIZIONE OPERAZIONE``
                    Causale = if isCausale then causale else 0
                }
            mov.Rows |> Seq.map bancaToGen
        
        let loadCarta() = 
            let mov = EstrattoContoCarta.Load(file)
            let bancaToGen (x:EstrattoContoCarta.Row) = 
                
                {
                    Data = Some x.``DATA ACQUISTO``
                    Dare= float x.``IMPORTO IN EURO``
                    Avere = Double.NaN
                    Descrizione = x.``DESCRIZIONE DELLE OPERAZIONI``
                    Causale = 0
                }
            mov.Rows |> Seq.map bancaToGen

        match csvType with
        | CsvType.ContoCorrente -> loadContoCorrente()
        | CsvType.CartaCredito -> loadCarta()

    let record (x:MovimentoGenerico) y= 
        let tag = 
            let rulesThatBegins =
                rules "tag" 
                |> Seq.filter (fun r -> r.Rule = "tag" && x.Descrizione.StartsWith(r.Starts))
            if Seq.isEmpty rulesThatBegins then 0 else Int32.Parse((Seq.head rulesThatBegins).TagId)
        
        {
            Date = x.Data.Value
            Ammount = y
            Causale = x.Causale
            Description = clean x.Descrizione
            Tag = tag
        }

    let getRecordFromMov(x:MovimentoGenerico) = 
        match Double.IsNaN x.Avere with
        | true -> Expence <| record x x.Dare
        | false -> Income <| record x x.Avere
        
    let getExpence (x:MovimentoGenerico) = Expence <| record x x.Dare
    let getIncome (x:MovimentoGenerico) = Income <| record x x.Avere
    let isExpence (x:MovimentoGenerico) = Double.IsNaN x.Avere

    let movimenti = 
        let rowsWithValuta =
            movimentiCsv
            |> Seq.filter (fun x -> x.Data.IsSome)
        
        let ignored = rules "ignore"
        let isIgnored (s:string) = ignored |> Seq.fold (fun acc x -> acc || s.StartsWith x.Starts) false
        let isSaldo s =  s = "Saldo iniziale" || s = "Saldo contabile"

        let shouldGet s = 
            let (!!!) x = if getIgnored then x else not x
            not (isSaldo s) && !!! (isIgnored s)

        let toAndFrom = 
            let getMaxAndMin (xs:'a seq)= 
                Seq.min xs,Seq.max xs

            rowsWithValuta
            |> Seq.map (fun x -> x.Data.Value)
            |> getMaxAndMin 

        let alreadyLoaded = 
            System.Diagnostics.Trace.WriteLine(sprintf "f: %A, S: %A" (fst toAndFrom) (snd toAndFrom))
            ToshClient.getEntries (fst toAndFrom) (snd toAndFrom)
            |> List.map 
                (fun x-> 
                    let descParts = x.desc.Split('\t')
                    if descParts.Length = 2 then Some descParts.[1] else None)

        let hash (x:MovimentoGenerico) =
                let record = getRecordFromMov x 
                let asStr = 
                    let conc (i:Record)= 
                        String.concat "" [i.Date.ToString("yyyy-MM-dd"); i.Ammount.ToString(); i.Description]
                    match record with 
                    | Income i -> conc i
                    | Expence i -> conc i
                let hash = DataAccessLayer.MovementSaver.getHash(asStr)
                System.Diagnostics.Trace.WriteLine("'"+asStr+"'\t"+ hash)
                hash

        let WasLoaded (x:MovimentoGenerico) =
            
            alreadyLoaded 
            |> List.tryFind (fun y -> 
                match y with 
                | Some r -> r = hash x
                | None -> false)
            |> Option.isSome
        
        setCulture "it-IT"
        System.Diagnostics.Trace.WriteLine("Dati Gia caricati")

        alreadyLoaded
        |> List.iter (fun x -> 
            match x with
            | Some s -> System.Diagnostics.Trace.WriteLine(s)
            | None -> System.Diagnostics.Trace.WriteLine("NONE"))
        
        System.Diagnostics.Trace.WriteLine("Dati Letti")
                
        rowsWithValuta
        |> Seq.filter (fun x -> shouldGet (clean x.Descrizione))
        |> Seq.filter (fun x -> not <| WasLoaded x)
        |> Seq.map getRecordFromMov
        
    movimenti


