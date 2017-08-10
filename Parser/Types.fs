module Types
open FSharp.Data
open System

type Ignored = CsvProvider<"MappingRules.csv",";">
type EstrattoConto = CsvProvider<"ListaMovimenti.csv",";",Culture="it-It">
type EstrattoContoCarta = CsvProvider<"movimentiCarta.csv","\t",Culture="it-It">

type Record = {
    Date: DateTime;
    Ammount: float;
    Causale: int;
    Description: string;
    Tag: int;
}

type Movement = 
    | Income of Record
    | Expence of Record

type MovimentoGenerico = {
    Data: DateTime option
    Dare: float
    Avere: float    
    Descrizione: string
    Causale: int
}

type CsvType = 
    | ContoCorrente = 0
    | CartaCredito = 1
