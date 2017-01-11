module Types
open FSharp.Data
open System

type Ignored = CsvProvider<"MappingRules.csv",";">
type EstrattoConto = CsvProvider<"ListaMovimenti.csv",";",Culture="it-It">

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