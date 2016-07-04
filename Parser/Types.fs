module Types
open FSharp.Data
open System

type Ignored = CsvProvider<"MappingRules.csv",";">
type EstrattoConto = CsvProvider<"ListaMovimenti.csv",";",Culture="it-It">
//type EstrattoConto = 
//    CsvProvider<"ListaMovimenti.csv",";",Culture="it-It", 
//        Schema=",,float ,float ,,,">
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