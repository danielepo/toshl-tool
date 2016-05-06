﻿module Types
open FSharp.Data
open System

type EstrattoConto = CsvProvider<"ListaMovimenti.csv",";">
type Record = {
    Date: DateTime;
    Ammount: float;
    Causale: int;
    Description: string;
}

type Movement = 
    | Income of Record
    | Expence of Record