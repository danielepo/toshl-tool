module Parser

open System
open Types

let loadMovimenti ()=

    let movimentiCsv = EstrattoConto.Load("ListaMovimenti.csv")
    let record (x:EstrattoConto.Row) y= {
        Date = DateTime.Parse(x.DATA);
        Ammount = y; 
        Causale =(if x.``CAUSALE ABI``.HasValue then x.``CAUSALE ABI``.Value else 0); 
        Description = x.``DESCRIZIONE OPERAZIONE``
        }
    let movimenti = 
        movimentiCsv.Rows 
        |> Seq.filter (fun x -> x.``DESCRIZIONE OPERAZIONE`` <> "Saldo iniziale" && x.``DESCRIZIONE OPERAZIONE`` <> "Saldo contabile") 
        |> Seq.map (fun x -> if Double.IsNaN x.AVERE then Expence <| record x x.DARE else Income <| record x x.AVERE)

//    for row in movimenti do
//        match row with 
//        | Expence x -> printf "Expence\t%s\t%.2f\t%d\t%s \n" (x.Date.ToShortDateString()) x.Ammount x.Causale x.Description
//        | Income x -> printf "Income\t%s\t%.2f\t%d\t%s \n" (x.Date.ToShortDateString()) x.Ammount x.Causale x.Description
//    
    movimenti